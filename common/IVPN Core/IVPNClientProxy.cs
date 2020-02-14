using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using IVPN.VpnProtocols;
using IVPN.VpnProtocols.OpenVPN;
using IVPN.VpnProtocols.WireGuard;
using IVPN.Exceptions;

namespace IVPN
{
    public enum ClientState
    {
        WAIT,
        RESOLVE,
        AUTH,
        GET_CONFIG,
        ASSIGN_IP,
        ADD_ROUTES,
        CONNECTED,
        RECONNECTING,
        DISCONNECTED
    }

    /// <summary>
    /// Client-application-side communication with service (IVPNProtocolClient)
    /// </summary>
    public class IVPNClientProxy
    {
        //private const int SYNC_RESPONSE_TIMEOUT_MS = 40000;
        private const int SYNC_RESPONSE_TIMEOUT_MS = 1000 * 60 * 6;

        public delegate void ServerListChangedHandler(VpnServersInfo vpnServers);
        public delegate void ServersPingsUpdatedHandler(Dictionary<string, int> pingResults);
        public delegate void ConnectedHandler(ulong timeSecFrom1970, string clientIP, string serverIP);
        public delegate void ConnectionStateHandler(string state, string stateAdditionalInfo);
        public delegate void DisconnectedHandler(bool failure, IVPNServer.DisconnectionReason reason, string reasonDescription);
        public delegate void SecurityPolicyHandler(string type, string message);
        public delegate void ServiceStatusChangeHandler(bool connected);
        public delegate void ServiceExitingHandler();
        public delegate void PreferencesHandler(Dictionary<string, string> preferences);
        public delegate void ConnectToLastServerHandler();
        public delegate void DiagnosticsSubmissionStatusHandler(bool success, string error);
        public delegate void DiagnosticsGeneratedHandler(IVPNDiagnosticsGeneratedResponse diagInfoResponse);
        public delegate void ExceptionHappenedHandler(Exception exception);
        public event ServerListChangedHandler ServerListChanged = delegate { };
        public event ServersPingsUpdatedHandler ServersPingsUpdated = delegate { };
        public event ConnectedHandler Connected = delegate { };
        public event ConnectionStateHandler ConnectionState = delegate { };
        public event DisconnectedHandler Disconnected = delegate { };
        public event ServiceStatusChangeHandler ServiceStatusChange = delegate { };
        public event ServiceExitingHandler ServiceExiting = delegate { };
        public event PreferencesHandler Preferences = delegate { };
        public event DiagnosticsGeneratedHandler DiagnosticsGenerated = delegate { };
        public event ExceptionHappenedHandler ClientException = delegate { };
        public event EventHandler ClientProxyDisconnected = delegate { };

        private TcpClient __Client;
        private StreamReader __StreamReader;
        private StreamWriter __StreamWriter;
        private Thread __Thread;
        private bool __IsExiting;

        private bool __EventsEnabled;
        private readonly EventWaitHandle __HoldSignal = new AutoResetEvent(false);
        private bool __ServiceConnected;

        private readonly SemaphoreSlim __SyncCallSemaphore = new SemaphoreSlim(1);
        private readonly BlockingCollection<IVPNResponse> __BlockingCollection = new BlockingCollection<IVPNResponse>();
        private CancellationTokenSource __CancellationToken;

        public void Initialize(int port)
        {
            __IsExiting = false;
            if (__Thread != null)
                throw new InvalidOperationException("client is already running");

            ServicePort = port;

            __Thread = new Thread(ClientThread) {Name = "IVPN Client Proxy", IsBackground = true};
            __Thread.Start();
        }

        private void ClientThread()
        {
            try
            {
                __CancellationToken = new CancellationTokenSource();
                ConnectToService();

                // send hello
                SendRequest(new IVPNHelloRequest { Version = Platform.Version });

                while (HandleRequest())
                {

                }

                Logging.Info("Handle request loop finished");
            }
            catch (Exception ex)
            {
                Logging.Info("error: " + ex.StackTrace);
                ClientException(ex);
            }
            finally
            {
                Logging.Info("closing socket");

                __Client?.Close();

                ServiceConnected = false;

                __CancellationToken.Cancel();
                __Thread = null;

                ClientProxyDisconnected(this, new EventArgs());
            }
        }

        private void ConnectToService()
        {
            // READING PORT NUMBER FROM FILE
            string portfile = "";
            // ServicePort is already initialized for macOS 
            if (Platform.IsWindows)
                portfile = Path.Combine(Platform.SettingsDirectory, "port.txt");

#if DEBUG
#warning Getting port name from hardcoded file path! "port.txt" (DEBUG MODE)
#warning "  Windows:    C:/Program Files/IVPN Client/etc/port.txt"
#warning "  macOS:      /Library/Application Support/IVPN/port.txt"
            // useful for debugging
            if (Platform.IsWindows)
                portfile = @"C:/Program Files/IVPN Client/etc/port.txt";
            else
                portfile = @"/Library/Application Support/IVPN/port.txt";
#endif

            // if we are obtaining port number from file - try to read it (and connect to service) several times
            // there is a chance that service is starting now and 'port.txt' is not created or contain old info 
            Exception retException = null;
            int connectionRetries = (string.IsNullOrEmpty(portfile)) ? 1 : 4;
            for (int retry = 0; retry < connectionRetries; retry++)
            {
                retException = null;
                if (retry > 0)
                    Thread.Sleep(1000);

                if (!string.IsNullOrEmpty(portfile))
                {
                    try
                    {
                        ServicePort = Convert.ToUInt16(File.ReadAllText(portfile));
                    }
                    catch (Exception ex)
                    {
                        retException = ex;
                        continue;
                    }
                }

                Logging.Info(string.Format("connecting to {0}:{1}", System.Net.IPAddress.Loopback, ServicePort));
                               
                try
                {
                    __Client = new TcpClient { NoDelay = true };
                    __Client.Connect(IPAddress.Loopback.ToString(), ServicePort);
                }
                catch (Exception ex)
                {
                    retException = ex;
                    continue;
                }

                __StreamReader = new StreamReader(__Client.GetStream());
                __StreamWriter = new StreamWriter(__Client.GetStream()) { AutoFlush = true };

                break;
            }

            if (retException != null)
                throw retException;
        }

        private bool HandleRequest()
        {
            if (!EventsEnabled)
            {
                Logging.Info("pausing events until events are reenabled");
                __HoldSignal.WaitOne();
            }

            var line = __StreamReader.ReadLine();
            if (string.IsNullOrEmpty(line))
                return false;

            IVPNResponse response = JsonConvert.DeserializeObject<IVPNResponse>(line, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
            Logging.Info("received " + response);

            switch (response.GetType().ToString())
            {
                case "IVPN.IVPNHelloResponse":

                    // Uncommment to test eror during connection to client proxy

                    // Thread.Sleep(2000);
                    // throw new Exception("This is some text exception with very long text");

                    Logging.Info("got hello, server version is " + ((IVPNHelloResponse)response).Version);
                    break;

                case "IVPN.IVPNServerListResponse":
                    Logging.Info($"Got servers info [{((IVPNServerListResponse)response).VpnServers.OpenVPNServers.Count} openVPN; {((IVPNServerListResponse)response).VpnServers.WireGuardServers.Count} WireGuard]");

                    VpnServersInfo retServers = ((IVPNServerListResponse)response).VpnServers;

                    // When no servers received:
                    // - on a initialization (ServiceConnected == false): throw an exception
                    // - if we already initialized (servers already initialized) - just ignore this empty response
                    if (!retServers.OpenVPNServers.Any() || !retServers.WireGuardServers.Any() )
                    {
                        if (ServiceConnected == false)
                            throw new ServersNotLoaded();
                        break;
                    }

                    VpnServerList = retServers;
                    ServerListChanged(VpnServerList);

                    if (ServiceConnected != true)
                        ServiceConnected = true; // final GUI initialization performs only after receiving servers-list
                    
                    break;

                case "IVPN.IVPNPingServersResponse":
                    IVPNPingServersResponse resp = (IVPNPingServersResponse)response;
                    Logging.Info($"Got ping response for {resp.pingResults.Count} servers");
                    ServersPingsUpdated(resp.pingResults);
                    break;

                case "IVPN.IVPNStateResponse":
                    ConnectionState(((IVPNStateResponse)response).State, ((IVPNStateResponse)response).StateAdditionalInfo);
                    break;

                case "IVPN.IVPNConnectedResponse":
                    IVPNConnectedResponse connectedRes = (IVPNConnectedResponse)response;
                    Connected(connectedRes.TimeSecFrom1970, connectedRes.ClientIP, connectedRes.ServerIP);
                    break;

                case "IVPN.IVPNDisconnectedResponse":
                    IVPNDisconnectedResponse discRes = response as IVPNDisconnectedResponse;
                    Disconnected(discRes.Failure, discRes.Reason, discRes.ReasonDescription);
                    break;

                case "IVPN.IVPNGetPreferencesResponse":
                    IVPNGetPreferencesResponse prefsRes = response as IVPNGetPreferencesResponse;
                    Preferences(prefsRes.Preferences);
                    break;

                case "IVPN.IVPNDiagnosticsGeneratedResponse":
                    {
                        IVPNDiagnosticsGeneratedResponse diag = response as IVPNDiagnosticsGeneratedResponse;
                        DiagnosticsGenerated(diag);
                    }
                    break;

                case "IVPN.IVPNServiceExitingResponse":
                    __IsExiting = true;
                    ServiceExiting();
                    break;

                case "IVPN.IVPNKillSwitchGetStatusResponse":
                case "IVPN.IVPNKillSwitchGetIsPestistentResponse":
                case "IVPN.IVPNSetAlternateDnsResponse":
                case "IVPN.IVPNEmptyResponse":
                case "IVPN.IVPNErrorResponse":
                    __BlockingCollection.Add(response);
                    break;
            }

            return true;
        }

        private void CheckConnected()
        {
            if (!ServiceConnected)
                throw new IVPNClientProxyNotConnectedException("Proxy is not connected to service");
        }

        private void SendRequest(IVPNRequest request)
        {
            Logging.Info("Sending: " + request);

            // Hello request can be sent without being in "connected
            if (!(request is IVPNHelloRequest))
                CheckConnected();

            lock (__StreamWriter)
            {
                __StreamWriter.WriteLine(JsonConvert.SerializeObject(request,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));
                __StreamWriter.Flush();
            }            
        }

        public void SetPreference(string key, string value)
        {
            SendRequest(new IVPNSetPreferenceRequest { Key = key, Value = value });
        }

        public void ConnectOpenVPN(OpenVPNVpnServer vpnServer, DestinationPort port, IPAddress manualDns, string username, string password, string proxyType = "none", string proxyAddress = null, int proxyPort = 0, string proxyUsername = null, string proxyPassword = null)
        {
            Logging.Info($"[OpenVPN] Connect: {vpnServer}:{port} as user: {username} (proxy: {proxyType}: {proxyAddress})");

            SendRequest(new IVPNConnectRequest
            {
                VpnType = VpnType.OpenVPN,
                CurrentDns = manualDns.ToString(),
                OpenVpnParameters = new OpenVPNConnectionParameters()
                { 
                    EntryVpnServer = vpnServer,
                    Port = port,
                    Username = username,
                    Password = password,
                    ProxyType = proxyType,
                    ProxyAddress = proxyAddress,
                    ProxyPort = proxyPort,
                    ProxyUsername = proxyUsername,
                    ProxyPassword = proxyPassword
                }
            });
        }

        public void ConnectWireGuard(WireGuardVpnServerInfo vpnServer, DestinationPort port, IPAddress manualDns, string internalClientIp, string privateKey)
        {
            Logging.Info($"[WireGuard] Connect: {vpnServer})");

            SendRequest(new IVPNConnectRequest
            {
                VpnType = VpnType.WireGuard,
                CurrentDns = manualDns.ToString(),
                WireGuardParameters = new WireGuardConnectionParameters
                {
                    EntryVpnServer = vpnServer,
                    InternalClientIp = internalClientIp,
                    Port = port,
                    LocalPrivateKey = privateKey
                }
            });
        }

        public void Disconnect()
        {
            Logging.Info("Disconnecting...");

            SendRequest(new IVPNDisconnectRequest());
        }

        public void PingServers(int pingTimeOutMs, int pingRetriesCount)
        {
            if (ServiceConnected)
                SendRequest(new IVPNPingServers { timeOutMs = pingTimeOutMs, retryCount = pingRetriesCount });
        }

        public void GenerateDiagnosticLogs(VpnType vpnProtocolType)
        {
            SendRequest(new IVPNGenerateDiagnosticsRequest {VpnProtocolType = vpnProtocolType});
        }

        private T GetSyncResponse<T>() where T : IVPNResponse
        {
            var result = __BlockingCollection.TryTake(out var response, SYNC_RESPONSE_TIMEOUT_MS, __CancellationToken.Token);
            if (!result)
                throw new TimeoutException("SyncResponse took more time than expected.");

            if (response is IVPNErrorResponse errorResponse)
                throw new IVPNClientProxyException(errorResponse.ErrorMessage);

            return (T)response;
        }

        public async Task<bool> KillSwitchGetIsEnabled()
        {
            return (await SendSyncRequestAsync<IVPNKillSwitchGetStatusResponse>(new IVPNKillSwitchGetStatusRequest())).IsEnabled;
        }

        public async Task KillSwitchSetEnabled(bool isEnabled)
        {            
            var request = new IVPNKillSwitchSetEnabledRequest() {IsEnabled = isEnabled};
            await SendSyncRequestAsync<IVPNEmptyResponse>(request);
        }

        public void KillSwitchSetAllowLAN(bool allowLAN)
        {
            var request = new IVPNKillSwitchSetAllowLANRequest {AllowLAN = allowLAN};
            SendRequest(request);
        }

        public void KillSwitchSetAllowLANMulticast(bool allowLANMulticast)
        {
            var request = new IVPNKillSwitchSetAllowLANMulticastRequest {AllowLANMulticast = allowLANMulticast};
            SendRequest(request);
        }

        public async Task KillSwitchSetIsPersistent(bool isPersistent)
        {
            await SendSyncRequestAsync<IVPNEmptyResponse>(new IVPNKillSwitchSetIsPersistentRequest() { IsPersistent = isPersistent });                            
        }

        public async Task<bool> KillSwitchGetIsPersistent()
        {            
            return (await SendSyncRequestAsync<IVPNKillSwitchGetIsPestistentResponse>(
                    new IVPNKillSwitchGetIsPestistentRequest())).IsPersistent;            
        }

        public async Task PauseConnection()
        {
            await SendSyncRequestAsync<IVPNEmptyResponse>(new IVPNPauseConnection());
        }

        public async Task ResumeConnection()
        {
            await SendSyncRequestAsync<IVPNEmptyResponse>(new IVPNResumeConnection());
        }

        public async Task<bool> SetAlternateDns(IPAddress dns)
        {
            return (await SendSyncRequestAsync<IVPNSetAlternateDnsResponse>(new IVPNSetAlternateDns {DNS = dns.ToString()})).IsSuccess;
        }

        private async Task<TResult> SendSyncRequestAsync<TResult>(IVPNRequest request) where TResult : IVPNResponse
        {
            await __SyncCallSemaphore.WaitAsync(__CancellationToken.Token);
            try
            {
                SendRequest(request);
                return await Task.Run(() => GetSyncResponse<TResult>());
            }
            finally
            {
                __SyncCallSemaphore.Release();
            }
        }        

        public void RemoveServiceCrashFile()
        {
            SendRequest(new IVPNRemoveServiceCrashFile ());
        }

        public void Exit()
        {
            Logging.Info("exiting...");
            __IsExiting = true;

            if (__Client != null)
            {
                __CancellationToken.Cancel();

                try
                {
                    try
                    {
                        __Client?.Close();
                    }
                    catch (Exception ex) 
                    {
                        Logging.Info ($"{ex}");
                    }

                    try 
                    {
                        __Thread?.Abort ();
                        __Thread = null;
                    }
                    catch (Exception ex) 
                    {
                        Logging.Info ($"{ex}");
                    }
                }
                catch (Exception ex)
                {
                    Logging.Info("Exit exception: " + ex.StackTrace);
                }
            }
        }

        public bool EventsEnabled
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return __EventsEnabled;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (value)
                    __HoldSignal.Set();
                else
                    __HoldSignal.Reset();

                __EventsEnabled = value;
            }
        }

        public bool ServiceConnected
        {
            get => __ServiceConnected;

            set
            {
                __ServiceConnected = value;
                ServiceStatusChange(__ServiceConnected);
            }
        }

        public VpnServersInfo VpnServerList { get; set; } = new VpnServersInfo();

        public int ServicePort { get; set; }

        public bool IsExiting => __IsExiting;
    }
}
