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

        public event IVPNClientProxy.AlternateDNSChangedHandler AlternateDNSChanged = delegate { };

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
            __ServiceProxy.KillSwitchStatus += (bool? enabled, bool? isPersistant, bool? isAllowLAN, bool? isAllowMulticast) =>
            {
                if (enabled != null)
                    UpdateKillSwitchIsEnabled((bool)enabled);
                if (isPersistant!=null)
                    UpdateKillSwitchIsPersistent((bool)isPersistant);
            };

            __ServiceProxy.AlternateDNSChanged += (string dns) =>
            {
                AlternateDNSChanged(dns);
            };

            __State = ServiceState.Uninitialized;
            __InitializationSignal = new ManualResetEvent(false);

            Servers = servers;

            SetProxyHandlers();
        }

        public async Task<bool> InitializeAsync(int port, UInt64 secret, Requests.RawCredentials creds)
        {
            if (__State != ServiceState.Uninitialized)
                return true;

            __InitializationSignal.Reset();

            __ServiceProxy.Initialize(port, secret, creds);
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

                ServiceInitialized(this, new EventArgs());
            }

            return State == ServiceState.Disconnected;
        }

        private void SetProxyHandlers()
        {
            Servers.OnPingUpdateRequired += Proxy.PingServers;

            __ServiceProxy.SessionInfoChanged += (SessionInfo s) => AppState.Instance().SetSession(s);
            

            __ServiceProxy.ServerListChanged += (VpnServersInfo servers) =>
            {
                // If still no servers info - save it immediately (do not use "Invoke" mechanism) 
                // It needed for situations when VPN already connected (on application start)
                //      All servers must be initialised on the moment when VPN connection status will be received.
                if (Servers.ServersList == null || Servers.ServersList.Count == 0)
                    Servers.UpdateServers(servers);

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
            Logging.Info($"Proxy EXCEPTION: {exception} - {exception.StackTrace}");
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
        /// If long time no response from server during connection (from WAIT to next event from openvpn managment interfrace) 
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

            var ct = __ConnectionTarget;
            if (ct == null
                || ct.PortsToReconnect.Count <= 0 
                || ct.OpenVpnProxyOptions != null) // in case of Proxy - no sense to change port
                return;

            if (!(State == ServiceState.Connecting
                  || State == ServiceState.ReconnectingOnClient
                  || State == ServiceState.ReconnectingOnService))
                return;

            State = ServiceState.ReconnectingOnClient;  // Mark that we have to reconnect after disconnection
            ct.ChangeToNextPort();      // Select new port for connection
            Proxy.Disconnect();         // Stop connection

            ReportProgress($"Reconnecting {ct.Port}...");
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
            __SyncInvoke.BeginInvoke(new Action(() =>
            {
                ProcessNewConnectionState(state, stateAdditionalInfo);
            }), null);            
        }

        private void ServiceProxy_Connected(ulong timeSecFrom1970, string clientIP, string serverIP, VpnType vpnType, string exitServerID)
        {
            if (State == ServiceState.CancellingConnection)
            {
                Disconnect();
                return;
            }

            ServerLocation servLocation = Servers.GetServerByIP(IPAddress.Parse(serverIP), vpnType);
            ServerLocation exitServLocation = null;
            if (vpnType == VpnType.OpenVPN && string.IsNullOrEmpty(exitServerID) == false)
            {
                exitServLocation = Servers.GetServerByOpenVpnMultihopID(exitServerID);
                if (exitServLocation == null)
                    Logging.Info("ERROR: unable to find exit server ID (Multihop): " + exitServerID);
            }

            ConnectionInfo newConnectionInfo = new ConnectionInfo(
                servLocation,
                exitServLocation,
                new DateTime(1970, 1, 1).AddSeconds(timeSecFrom1970),
                clientIP,
                serverIP,
                vpnType
            );

            ConnectionResult result = new ConnectionResult(true) {ConnectionInfo = newConnectionInfo};
            __ConnectionTCS?.TrySetResult(result);

            __SyncInvoke.BeginInvoke(new Action(() =>
                {
                    ReportProgress("Connected");
                    State = ServiceState.Connected;
                }), null);

            Connected(newConnectionInfo);
        }

        private void ServiceProxy_Disconnected(bool failure, DisconnectionReason reason, string reasonDescription)
        {
            __SyncInvoke.BeginInvoke(new Action(async () =>
            {
                var connTarget = __ConnectionTarget;
                if (connTarget != null
                    && reason !=  DisconnectionReason.DisconnectRequested
                    && (
                        State == ServiceState.Connected ||
                        State == ServiceState.ReconnectingOnService ||
                        State == ServiceState.ReconnectingOnClient // ReconnectingOnClient - was set by watchdog. Current connection failed - try to reconnect
                    )) 
                {
                    if (State != ServiceState.ReconnectingOnClient)
                    {
                        ReportProgress("Reconnecting ...");
                        State = ServiceState.ReconnectingOnClient;
                    }

                    // Do some delay before next connection. But stop in case of disconnection request
                    for (int i=0; i<30 && State == ServiceState.ReconnectingOnClient; i++) await Task.Delay(100);
                    
                    if (__IsSuspended)
                    {
                        if (!await WaitUntilUnsuspended())
                        {
                            SetStatusDisconnected(failure, reason, reasonDescription);
                            return;
                        }
                    }

                    if (State == ServiceState.Disconnecting)
                    {
                        SetStatusDisconnected(failure, reason, reasonDescription);
                        return;
                    }

                    DoConnect(connTarget);
                    return;
                }

                SetStatusDisconnected(failure, reason, reasonDescription);
            }), null);
        }

        private void SetStatusDisconnected(bool failure, DisconnectionReason reason, string reasonDescription)
        {
            ConnectionResult result = new ConnectionResult(false, reasonDescription);
            __ConnectionTCS?.TrySetResult(result);

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
                        connectionTarget.OpenVpnMultihopExitSrvId,
                        connectionTarget.Port,
                        connectionTarget.CurrentManualDns);
                else
                    __ServiceProxy.ConnectOpenVPN(
                        svr,
                        connectionTarget.OpenVpnMultihopExitSrvId,
                        connectionTarget.Port,
                        connectionTarget.CurrentManualDns,
                        connectionTarget.OpenVpnProxyOptions.Type,
                        connectionTarget.OpenVpnProxyOptions.Server,
                        connectionTarget.OpenVpnProxyOptions.Port,
                        connectionTarget.OpenVpnProxyOptions.Username,
                        connectionTarget.OpenVpnProxyOptions.UnsafePassword);
            }
            else if (connectionTarget.Server.VpnServer is WireGuardVpnServerInfo)
            {
                WireGuardVpnServerInfo svr = connectionTarget.Server.VpnServer as WireGuardVpnServerInfo;
                __ServiceProxy.ConnectWireGuard(svr, 
                    connectionTarget.Port,
                    connectionTarget.CurrentManualDns);
            }
            else
                throw new Exception($"[{nameof(DoConnect)}] Internal exception. Unexpected type of connectionTarget ({connectionTarget.GetType()})");

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
                //await UpdateKillSwitchIsEnabled();
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
                UpdateKillSwitchIsEnabled(await __ServiceProxy.KillSwitchGetIsEnabled());
            }
            catch (Exception ex)
            {
                ProcessProxyException(ex);
            }
        }

        private void UpdateKillSwitchIsEnabled(bool isEnabled)
        {
            __KillSwitchIsEnabled = isEnabled;
            DoPropertyChanged(nameof(KillSwitchIsEnabled));
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
                //await UpdateKillSwitchIsPersistent();
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
                UpdateKillSwitchIsPersistent(await __ServiceProxy.KillSwitchGetIsPersistent());
            }
            catch (Exception ex)
            {
                ProcessProxyException(ex);
            }
        }

        private void UpdateKillSwitchIsPersistent(bool isPersistent)
        {
            __KillSwitchIsPersistent = isPersistent;
            DoPropertyChanged(nameof(KillSwitchIsPersistent));
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

                var connTarget = __ConnectionTarget;
                if (connTarget != null)
                    connTarget.CurrentManualDns = dnsIp;

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

        public async Task<Responses.SessionNewResponse> Login(string accountID, bool forceDeleteAllSesssions)
        {
            return await __ServiceProxy.LogIn(accountID, forceDeleteAllSesssions);
        }

        public async Task Logout()
        {
            await __ServiceProxy.LogOut();
        }

        public async Task<Responses.SessionStatusResponse> SessionStatus()
        {
            return await __ServiceProxy.SessionStatus();
        }

        public async Task WireGuardGeneratedKeys(bool generateIfNecessary)
        {
            await __ServiceProxy.WireGuardGeneratedKeys(generateIfNecessary);
        }

        public async Task WireGuardKeysSetRotationInterval(Int64 interval)
        {
            await __ServiceProxy.WireGuardKeysSetRotationInterval(interval);
        }
    }
}
