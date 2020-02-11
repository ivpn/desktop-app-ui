using IVPN.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IVPN.VpnProtocols;
using IVPN.VpnProtocols.OpenVPN;
using IVPNCommon.Interfaces;
using IVPN.VpnProtocols.WireGuard;

namespace IVPN.Models
{
    public delegate void ServiceExceptionDelegate(Service sender, Exception exception);

    public class Service : ModelBase, IService
    {
        public event ServiceExceptionDelegate ServiceExceptionRaised = delegate { };
        public event EventHandler ServiceDisconnected = delegate { };

        public event VPNDisconnected Disconnected = delegate { };
        public event VPNConnected Connected = delegate { };

        public event EventHandler ServiceInitialized = delegate { };
        public event EventHandler ServiceExited = delegate { };

        private readonly IVPNClientProxy __ServiceProxy;
        private ConnectionTarget __ConnectionTarget;
        private readonly object __ConnectionTargetLocker = new object();

        private TaskCompletionSource<ConnectionResult> __ConnectionTCS;
        private IProgress<string> __ConnectionProgress;

        private ServiceState __State;
        bool __IsSuspended;

        private readonly EventWaitHandle __InitializationSignal;
        
        private readonly ISynchronizeInvoke __SyncInvoke;
        private bool __KillSwitchIsEnabled;
        private bool __KillSwitchIsPersistent;

        private readonly HashSet<IProgress<string>> __ProgressListeners = new HashSet<IProgress<string>>();

        public Service(ISynchronizeInvoke syncInvoke, IServers servers)
        {
            __SyncInvoke = syncInvoke;
            __ServiceProxy = new IVPNClientProxy();

            __ServiceProxy.Connected += ServiceProxy_Connected;
            __ServiceProxy.ConnectionState += ServiceProxy_ConnectionState;
            __ServiceProxy.Disconnected += ServiceProxy_Disconnected;

            __State = ServiceState.Uninitialized;
            __InitializationSignal = new ManualResetEvent(false);

            Servers = servers;

            SetProxyHandlers();
        }

        public async Task<bool> InitializeAsync(int port)
        {
            if (__State != ServiceState.Uninitialized)
                return true;

            __InitializationSignal.Reset();

            __ServiceProxy.Initialize(port);
            await Task.Run(() =>
            {
                while (!__ServiceProxy.IsExiting)
                {
                    if (__InitializationSignal.WaitOne(100))
                        break;
                }
            });

            if (__ServiceProxy.IsExiting)
                return false;

            if (State != ServiceState.Uninitialized)
            {
                await UpdateKillSwitchIsEnabled();
                await UpdateKillSwitchIsPersistent();

                // request server for DNS filter status (is it disabled or not?)
                // response can be received asynchronously (without 'await')
                //UpdateIsDnsFilterDisabled();

                ServiceInitialized(this, new EventArgs());
            }

            return State == ServiceState.Disconnected;
        }

        private void SetProxyHandlers()
        {
            Servers.OnPingUpdateRequired += Proxy.PingServers;
            __ServiceProxy.ServerListChanged += (VpnServersInfo servers) =>
            {
                __SyncInvoke.BeginInvoke(new Action(() =>
                {
                    // Update servers information
                    Servers.UpdateServers(servers);

                    // update API services: alternate host IPs
                    if (servers?.Config?.Api?.IPs != null)
                        IVPNCommon.Api.ApiServices.Instance.SetAlternateHostIPs(new List<string> (servers.Config.Api.IPs));

                }), null);
            };

            __ServiceProxy.ServersPingsUpdated += (Dictionary<string, int> pingResults) => 
            {
                __SyncInvoke.BeginInvoke(new Action(() =>
                {
                    Servers.UpdateServersPings(pingResults);
                }), null);
            };

            __ServiceProxy.Preferences += (Dictionary<string, string> preferences) =>
            {
                // ignore preferences from service.
                // client will set its own settings
            };

            __ServiceProxy.ServiceStatusChange += (bool connected) =>
            {
                __SyncInvoke.BeginInvoke(new Action(() =>
                {
                    ServiceStatusChange(connected);
                }), null);
            };

            __ServiceProxy.ServiceExiting += () =>
            {
                __SyncInvoke.BeginInvoke(new Action(ServiceExiting), null);
            };

            __ServiceProxy.ClientException += ProcessProxyException;

            __ServiceProxy.ClientProxyDisconnected += ServiceProxy_ClientProxyDisconnected;

            __ServiceProxy.EventsEnabled = true;
        }

        private void ProcessProxyException(Exception exception)
        {
            __SyncInvoke.BeginInvoke(new Action(() =>
            {
                // There is a problems with communication to service(agent)
                // Close connections and stop communication thread.
                __ServiceProxy.Exit ();

                ServiceProxyException(exception);
            }), null);
        }

        void ServiceProxy_ClientProxyDisconnected(object sender, EventArgs e)
        {
            __SyncInvoke.BeginInvoke(new Action(() =>
                {
                    State = ServiceState.Uninitialized;
                    __InitializationSignal.Set();
                    
                    ServiceDisconnected(this, new EventArgs());
                }), null);
        }

        private void ServiceProxyException(Exception exception)
        {
            State = ServiceState.Uninitialized;
            __InitializationSignal.Set();

            ServiceExceptionRaised(this, exception);
        }

        private void ServiceExiting()
        {
            ServiceExited(this, new EventArgs());
        }

        private void ServiceStatusChange(bool connectionEstablished)
        {
            IsConnectedToService = connectionEstablished;

            if (connectionEstablished && State == ServiceState.Uninitialized)
                State = ServiceState.Disconnected;

            __InitializationSignal.Set();
        }

        public IServers Servers { get; private set; }
        
        public ServiceState State
        {
            get => __State;
            set
            {
                __State = value;
                DoPropertyChanged();
            }
        }

        public IVPNClientProxy Proxy => __ServiceProxy;

        public void Disconnect()
        {
            if (State == ServiceState.CancellingConnection
                || State == ServiceState.Disconnecting)
                return;

            if (State != ServiceState.Connected &&
                State != ServiceState.ReconnectingOnService &&
                State != ServiceState.ReconnectingOnClient &&
                State != ServiceState.Connecting)
            {
                Logging.Info("Warning: Cannot disconnect when not connected to VPN");
                return;
            }

            if (State == ServiceState.Connecting ||
                State == ServiceState.ReconnectingOnService ||
                State == ServiceState.ReconnectingOnClient)
            {
                ReportProgress("Cancelling...");
                State = ServiceState.CancellingConnection;

                if (__IsSuspended)
                    __IsSuspended = false;
            }

            if (State == ServiceState.Connected)
            {
                ReportProgress("Disconnecting...");
                State = ServiceState.Disconnecting;
            }

            Proxy.Disconnect();
        }

        public void Exit()
        {
            Proxy.Exit();
        }

        private bool __IsConnectedToService;

        public bool IsConnectedToService
        {
            get => __IsConnectedToService;
            set
            {
                __IsConnectedToService = value;
                DoPropertyChanged();
            }
        }

        #region Connection process watchdog timer
        /// <summary>
        /// Connection watchdog timer: 
        /// If long time no response from server during connection (from WAIT to next event from openvpm managment interfrace) 
        /// - watchdog can cancel current connection process and try to connect to another port
        /// </summary>
        private Timer __ConnectingProcessWatchDogTimer;
        private void InitConnectionProcessWatchDogTimer()
        {
            StopConnectionProcessWatchDogTimer();

            Timer watchdog = __ConnectingProcessWatchDogTimer;
            watchdog?.Dispose();

            __ConnectingProcessWatchDogTimer = 
                new Timer
                (
                    (state) => { TryConnectOnAnotherPortIfConnecting(); }, 
                    null, Constants.ConnectionWatchDogTimerTimeoutMs, Timeout.Infinite
                );
        }

        private void StopConnectionProcessWatchDogTimer()
        {
            Timer timer = __ConnectingProcessWatchDogTimer;
            timer?.Dispose();
            __ConnectingProcessWatchDogTimer = null;
        }
        #endregion //Connection process watchdog timer

        private void TryConnectOnAnotherPortIfConnecting()
        {
            StopConnectionProcessWatchDogTimer();

            if (__ConnectionTarget == null || __ConnectionTarget.PortsToReconnect.Count <= 0)
                return;
            
            if (!(State == ServiceState.Connecting
                  || State == ServiceState.ReconnectingOnClient
                  || State == ServiceState.ReconnectingOnService))
                return;

            State = ServiceState.ReconnectingOnClient;  // Mark that we have to reconnect after disconnection
            __ConnectionTarget.ChangeToNextPort();      // Select new port for connection
            Proxy.Disconnect();                         // Stop connection

            ReportProgress($"Reconnecting {__ConnectionTarget.Port}...");
        }
        
        private void ProcessNewConnectionState(string state, string stateAdditionalInfo)
        {
            // TODO: Move code to OpenVPN specific place

            // TCP successful connection:   "TCP_CONNECT" -> "WAIT" -> "AUTH" -> "GET_CONFIG" -> "ASSIGN_IP"
            // TCP blocked port connection: "TCP_CONNECT" -> "RECONNECTING init_instance"
            // UDP successful connection:   "WAIT" -> "AUTH" -> "GET_CONFIG" -> "ASSIGN_IP"
            // UDP blocked port connection: "WAIT" -> "RECONNECTING tls-error"

            // TODO: 'TCP_CONNECT' not implemented in golang service! Feature will not work!
            if (string.Equals(state,"WAIT") || string.Equals(state, "TCP_CONNECT"))
                InitConnectionProcessWatchDogTimer();
            else
                StopConnectionProcessWatchDogTimer();

            string normalizedState = state.Replace("_", "");
            if (Constants.STATE_DESCRIPTIONS.ContainsKey(normalizedState))
                ReportProgress(Constants.STATE_DESCRIPTIONS[normalizedState]);

            if ( state == "RECONNECTING"
                 && (State != ServiceState.Disconnected             // if we are in disconnecting stage - ignore RECONNECTING from openVPN
                     && State != ServiceState.Disconnecting         
                     && State != ServiceState.CancellingConnection))
            {
                State = ServiceState.ReconnectingOnService;

                // When 'tls-error':
                //      it could be because of TLS handshake failed 
                //      (occurs for UDP connection after 'hand-window XXX' seconds from openvpn configuration )
                // When 'init_instance':
                //      (windows) - occurs for TCP connection after long time (~2min) 
                //      (mac) - occurs for TCP connection immediately 
                // Probably, port is blocked. Try to reconnect with new port.
                if (string.Equals(stateAdditionalInfo, "tls-error")
                    || string.Equals(stateAdditionalInfo, "init_instance"))
                    TryConnectOnAnotherPortIfConnecting();
            }
        }
               
        private void ServiceProxy_ConnectionState(string state, string stateAdditionalInfo)
        {
            if (__ConnectionTarget == null || __ConnectionProgress == null || __ConnectionTCS == null)
                return;

            __SyncInvoke.BeginInvoke(new Action(() =>
            {
                ProcessNewConnectionState(state, stateAdditionalInfo);
            }), null);            
        }

        private void ServiceProxy_Connected(ulong timeSecFrom1970, string clientIP, string serverIP)
        {
            if (__ConnectionTarget == null || __ConnectionProgress == null || __ConnectionTCS == null)
                return;

            if (State == ServiceState.CancellingConnection)
            {
                Disconnect();
                return;
            }

            string vpnProtocolInfo;
            if (__ConnectionTarget != null) // TODO: to think: we must use another mechanism for determining VPN protocol in use!
                vpnProtocolInfo = (__ConnectionTarget.Server.VpnServer is OpenVPNVpnServer) ? VpnType.OpenVPN.ToString() : VpnType.WireGuard.ToString();
            else
                vpnProtocolInfo = "unknown error";

            ConnectionInfo newConnectionInfo = new ConnectionInfo(
                __ConnectionTarget.Server,
                new DateTime(1970, 1, 1).AddSeconds(timeSecFrom1970),
                clientIP,
                serverIP,
                vpnProtocolInfo
            );

            ConnectionResult result = new ConnectionResult(true) {ConnectionInfo = newConnectionInfo};
            __ConnectionTCS.TrySetResult(result);

            __SyncInvoke.BeginInvoke(new Action(() =>
                {
                    ReportProgress("Connected");
                    State = ServiceState.Connected;
                }), null);

            Connected(newConnectionInfo);
        }

        private void ServiceProxy_Disconnected(bool failure, IVPNServer.DisconnectionReason reason, string reasonDescription)
        {

            if (__ConnectionTarget == null || __ConnectionProgress == null || __ConnectionTCS == null)
                return;

            __SyncInvoke.BeginInvoke(new Action(async () =>
                {
                    if (State == ServiceState.Connected ||
                        State == ServiceState.ReconnectingOnService ||
                        State == ServiceState.ReconnectingOnClient) // ReconnectingOnClient - was set by watchdog. Current connection failed - try to reconnect
                    {
                        State = ServiceState.ReconnectingOnClient;

                        if (__IsSuspended)
                        {
                            if (!await WaitUntilUnsuspended())
                            {
                                SetStatusDisconnected(failure, reason, reasonDescription);
                                return;
                            }
                        }

                        DoConnect(__ConnectionTarget);
                        return;
                    }

                SetStatusDisconnected(failure, reason, reasonDescription);
                }), null);
        }

        private void SetStatusDisconnected(bool failure, IVPNServer.DisconnectionReason reason, string reasonDescription)
        {
            ConnectionResult result = new ConnectionResult(false, reasonDescription);
            __ConnectionTCS.TrySetResult(result);

            ReportProgress("Disconnected");
            State = ServiceState.Disconnected;
                
            Disconnected(failure, reason, reasonDescription);
        }

        private async Task<bool> WaitUntilUnsuspended()
        {
            await Task.Run(() =>
            {
                while (__IsSuspended && State == ServiceState.ReconnectingOnClient)
                {
                    Thread.Sleep(100);
                }
            });

            if (State != ServiceState.ReconnectingOnClient)
                return false;

            return true;
        }


        Task<ConnectionResult> IService.Connect(
                                IProgress<string> progress,
                                CancellationToken cancellationToken,
                                ConnectionTarget connectionTarget)
        {
            ConnectionTarget = connectionTarget;
            __ConnectionProgress = progress;
            __ConnectionTCS = new TaskCompletionSource<ConnectionResult>();

            ReportProgress("Connecting...");
            State = ServiceState.Connecting;

            DoConnect(connectionTarget);
            return __ConnectionTCS.Task;
        }

        private void DoConnect(ConnectionTarget connectionTarget)
        {
            if (connectionTarget.Server.VpnServer is OpenVPNVpnServer)
            {
                OpenVPNVpnServer svr = connectionTarget.Server.VpnServer as OpenVPNVpnServer;
                if (connectionTarget.OpenVpnProxyOptions == null)
                    __ServiceProxy.ConnectOpenVPN(
                        svr,
                        connectionTarget.Port,
                        connectionTarget.CurrentManualDns,
                        connectionTarget.OpenVpnUsername,
                        connectionTarget.OpenVpnPassword);
                else
                    __ServiceProxy.ConnectOpenVPN(
                        svr,
                        connectionTarget.Port,
                        connectionTarget.CurrentManualDns,
                        connectionTarget.OpenVpnUsername,
                        connectionTarget.OpenVpnPassword,
                        connectionTarget.OpenVpnProxyOptions.Type,
                        connectionTarget.OpenVpnProxyOptions.Server,
                        connectionTarget.OpenVpnProxyOptions.Port,
                        connectionTarget.OpenVpnProxyOptions.Username,
                        connectionTarget.OpenVpnProxyOptions.UnsafePassword);
            }
            else if (connectionTarget.Server.VpnServer is WireGuardVpnServerInfo)
            {
                if (string.IsNullOrEmpty(connectionTarget.WireGuardInternalClientIp) || string.IsNullOrEmpty(connectionTarget.WireGuardLocalPrivateKey))
                    throw new Exception("Unable to connect: WireGuard configuration not defined");

                WireGuardVpnServerInfo svr = connectionTarget.Server.VpnServer as WireGuardVpnServerInfo;
                __ServiceProxy.ConnectWireGuard(svr, 
                    connectionTarget.Port,
                    connectionTarget.CurrentManualDns,
                    connectionTarget.WireGuardInternalClientIp,
                    connectionTarget.WireGuardLocalPrivateKey);
            }
            else
                throw new Exception($"[{nameof(DoConnect)}] Internal exception. Unexpected type of connectionTarget ({connectionTarget.GetType()})");

        }

        public void UpdateWireguardLocalCredentias(string internalClientIp, string privateKey)
        {
            var old = ConnectionTarget;
            if (old == null)
                return;

            // '__ConnectionTarget' always should contain actuall connection information. It can be used for reconnection (e.g. in method 'ServiceProxy_Disconnected()')
            ConnectionTarget = new ConnectionTarget(old.Server, old.Port, old.PortsToReconnect, old.CurrentManualDns, old.OpenVpnUsername, old.OpenVpnPassword, old.OpenVpnProxyOptions,
                internalClientIp, privateKey);

            // UpdateWireGuardCredentials -  doing disconnect/connect on a service side (only if WG is connected).
            // But it does not notify client 'Disconnected' evet and do not disable KillSwitch during reconnection
            __ServiceProxy.UpdateWireGuardCredentials(internalClientIp, privateKey);
        }

        /// <summary>
        /// Register connection progress object
        /// All registered objects will be notified about progress during connection
        /// </summary>
        public void RegisterConnectionProgressListener(IProgress<string> progress)
        {
            __ProgressListeners.Add(progress);
        }

        private void ReportProgress(string progressText)
        {
            __ConnectionProgress?.Report(progressText);

            foreach(var progressObj in __ProgressListeners)
                progressObj.Report(progressText);
        }

        #region KillSwitch
        public bool KillSwitchIsEnabled
        {
            get
            {
                if (__State == ServiceState.Uninitialized)
                    return false;

                return __KillSwitchIsEnabled;
            }
            set
            {
                Logging.Info($"KillSwitchIsEnabled = {value.ToString()}");

                SetKillSwitchEnabled(value);
            }
        }

        private async void SetKillSwitchEnabled(bool value)
        {
            try
            { 
                await __ServiceProxy.KillSwitchSetEnabled(value);
                await UpdateKillSwitchIsEnabled();
            }
            catch (Exception ex)
            {
                ProcessProxyException(ex);
            }
        }

        private async Task UpdateKillSwitchIsEnabled()
        {
            try
            {
                __KillSwitchIsEnabled = await __ServiceProxy.KillSwitchGetIsEnabled();
                DoPropertyChanged(nameof(KillSwitchIsEnabled));
            }
            catch (Exception ex)
            {
                ProcessProxyException(ex);
            }
        }
        #endregion //KillSwitch

        public bool KillSwitchIsPersistent
        {
            get => __KillSwitchIsPersistent;

            set => SetKillSwitchIsPesistent(value);
        }

        private async void SetKillSwitchIsPesistent(bool value)
        {
            try
            {
                await __ServiceProxy.KillSwitchSetIsPersistent(value);
                await UpdateKillSwitchIsPersistent();
            }
            catch (Exception ex)
            {
                ProcessProxyException(ex);
            }
        }

        private async Task UpdateKillSwitchIsPersistent()
        {
            try
            { 
                __KillSwitchIsPersistent = await __ServiceProxy.KillSwitchGetIsPersistent();
                DoPropertyChanged(nameof(KillSwitchIsPersistent));
            }
            catch (Exception ex)
            {
                ProcessProxyException(ex);
            }
        }

        public bool KillSwitchAllowLAN
        {
            set => __ServiceProxy.KillSwitchSetAllowLAN(value);
        }

        public bool KillSwitchAllowLANMulticast
        {
            set => __ServiceProxy.KillSwitchSetAllowLANMulticast(value);
        }

        public void Suspend()
        {           
            if (__State == ServiceState.Connected)
            {
                __ServiceProxy.Disconnect();
                __IsSuspended = true;
                Logging.Info ("Suspend");
			}
        }

        public bool IsSuspended => __IsSuspended;

        public void Resume()
        {           
            __IsSuspended = false;
            Logging.Info ("Resume");
        }

        public ConnectionTarget ConnectionTarget
        {
            get
            {
                lock (__ConnectionTargetLocker)
                {
                    return __ConnectionTarget;
                }
            }
            private set
            {
                lock (__ConnectionTargetLocker)
                {
                    __ConnectionTarget = value;
                }
            }
        }
        /// <summary>
        /// Request service to remove service-crash file
        /// (Platform.ServiceCrashInfoFilePath)
        /// 
        /// Client itseld can not remove it, because file was creaded by service with admin rights
        /// </summary>
        public void RemoveServiceCrashFile()
        {
            __ServiceProxy.RemoveServiceCrashFile();
        }

        public async Task PauseOn()
        {
            await __ServiceProxy.PauseConnection();
        }

        public async Task PauseOff()
        {
            await __ServiceProxy.ResumeConnection();
        }

        #region DNS filter
        
        public async Task<bool> SetDns(IPAddress dnsIp)
        {
            bool isSuccess = false;
            try
            {
                if (dnsIp == null)
                    dnsIp = IPAddress.None;
                
                // waiting for response (can take long time)
                isSuccess = await __ServiceProxy.SetAlternateDns(dnsIp);
                Logging.Info($"SetDns isSuccess={isSuccess}");
            }
            catch (Exception ex)
            {
                ProcessProxyException(ex);
            }
            
            return isSuccess;
        }
        #endregion // DNS filter
    }
}
