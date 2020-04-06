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
using IVPN.RESTApi;

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
        private const int SYNC_RESPONSE_TIMEOUT_MS = 1000 * 60 * 6;

        public delegate void ServerListChangedHandler(VpnServersInfo vpnServers);
        public delegate void ServersPingsUpdatedHandler(Dictionary<string, int> pingResults);
        public delegate void ConnectedHandler(ulong timeSecFrom1970, string clientIP, string serverIP, VpnType VpnType);
        public delegate void ConnectionStateHandler(string state, string stateAdditionalInfo);
        public delegate void DisconnectedHandler(bool failure, DisconnectionReason reason, string reasonDescription);
        public delegate void SecurityPolicyHandler(string type, string message);
        public delegate void ServiceStatusChangeHandler(bool connected);
        public delegate void ServiceExitingHandler();
        public delegate void PreferencesHandler(Dictionary<string, string> preferences);
        public delegate void ConnectToLastServerHandler();
        public delegate void DiagnosticsSubmissionStatusHandler(bool success, string error);
        public delegate void DiagnosticsGeneratedHandler(Responses.IVPNDiagnosticsGeneratedResponse diagInfoResponse);
        public delegate void ExceptionHappenedHandler(Exception exception);
        public delegate void SessionInfoChangedHandler(SessionInfo s);
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
        public event SessionInfoChangedHandler SessionInfoChanged = delegate { };

        private TcpClient __Client;
        private StreamReader __StreamReader;
        private StreamWriter __StreamWriter;
        private Thread __Thread;
        private bool __IsExiting;

        private bool __EventsEnabled;
        private readonly EventWaitHandle __HoldSignal = new AutoResetEvent(false);
        private bool __ServiceConnected;

        private readonly SemaphoreSlim __SyncCallSemaphore = new SemaphoreSlim(1);
        private readonly BlockingCollection<Responses.IVPNResponse> __BlockingCollection = new BlockingCollection<Responses.IVPNResponse>();
        private CancellationTokenSource __CancellationToken;

        public void Initialize(int port, UInt64 secret)
        {
            __IsExiting = false;
            if (__Thread != null)
                throw new InvalidOperationException("client is already running");

            ServicePort = port;
            Secret = secret;

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
                SendRequest(new Requests.Hello { Version = Platform.Version, Secret = Secret, GetServersList = true, GetStatus = true });


                while (HandleRequest())
                {

                }

                Logging.Info("Handle request loop finished");
            }
            catch (Exception ex)
            {
                Logging.Info($"ERROR: {ex} Stack:" + ex.StackTrace);
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
                        string connectionInfo = File.ReadAllText(portfile);
                        var p = connectionInfo.Split(new char[] {':'});

                        ServicePort = Convert.ToUInt16(p[0]);
                        Secret = Convert.ToUInt64(p[1], 16);
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

            string line = "";

            try
            {
                line = __StreamReader.ReadLine();
                if (string.IsNullOrEmpty(line))
                    return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to receive command ", ex);
            }

            Responses.IVPNResponse response;

            try
            {
                response = JsonConvert.DeserializeObject<Responses.IVPNResponse>(line);
                Logging.Info($"received {response.Command}: {response}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse command '{line}'", ex);
            }

            try
            { 
                switch (response.Command)
                {
                    case "HelloResp":
                        var resp = JsonConvert.DeserializeObject<Responses.IVPNHelloResponse>(line);

                        NotifySessionInfo(
                                    resp.Session.AccountID,
                                    resp.Session.Session,
                                    resp.Session.WgPublicKey,
                                    resp.Session.WgLocalIP,
                                    resp.Session.WgKeyGenerated,
                                    resp.Session.WgKeysRegenInerval);
                                               
                        Logging.Info("got hello, server version is " + resp.Version
                            + (string.IsNullOrEmpty(resp.Session.Session)?"Not logged in":"Logged in"));

                        break;

                    case "ServerListResp":
                        var serversResp = JsonConvert.DeserializeObject<Responses.IVPNServerListResponse>(line);

                        Logging.Info($"Got servers info [{serversResp.VpnServers.OpenVPNServers.Count} openVPN; {serversResp.VpnServers.WireGuardServers.Count} WireGuard]");
                        VpnServersInfo retServers = serversResp.VpnServers;

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

                    case "PingServersResp":
                        var pingResp = JsonConvert.DeserializeObject<Responses.IVPNPingServersResponse>(line);
                        if (pingResp.PingResults != null)
                        {
                            Dictionary<string, int> results = pingResp.PingResults.ToDictionary(x => x.Host, x => x.Ping);

                            Logging.Info($"Got ping response for {results.Count} servers");
                            ServersPingsUpdated(results);
                        }
                        break;

                    case "VpnStateResp":
                        var stateResp = JsonConvert.DeserializeObject<Responses.IVPNVpnStateResponse>(line);
                        ConnectionState(stateResp.State, stateResp.StateAdditionalInfo);
                        break;

                    case "ConnectedResp":
                        var connectedRes = JsonConvert.DeserializeObject<Responses.IVPNConnectedResponse>(line);  
                        Connected(connectedRes.TimeSecFrom1970, connectedRes.ClientIP, connectedRes.ServerIP, connectedRes.VpnType);
                        break;

                    case "DisconnectedResp":
                        var discRes = JsonConvert.DeserializeObject<Responses.IVPNDisconnectedResponse>(line);
                        Disconnected(discRes.Failure, discRes.Reason, discRes.ReasonDescription);
                        break;

                    case "DiagnosticsGeneratedResp":
                        var diagResp = JsonConvert.DeserializeObject<Responses.IVPNDiagnosticsGeneratedResponse>(line);  
                        DiagnosticsGenerated(diagResp);
                        break;

                    case "ServiceExitingResp":
                        __IsExiting = true;
                        ServiceExiting();
                        break;

                    // :: __BlockingCollection ::
                    case "KillSwitchStatusResp":
                        var kssResp = JsonConvert.DeserializeObject<Responses.IVPNKillSwitchStatusResponse>(line);
                        __BlockingCollection.Add(kssResp);
                        break;
                    case "KillSwitchGetIsPestistentResp":
                        var kspResp = JsonConvert.DeserializeObject<Responses.IVPNKillSwitchGetIsPestistentResponse>(line);
                        __BlockingCollection.Add(kspResp);
                        break;

                    case "SessionNewResp":
                        {
                            var snResp = JsonConvert.DeserializeObject<Responses.SessionNewResponse>(line);
                            if (snResp.APIStatus == (int)ApiStatusCode.Success)
                            {
                                NotifySessionInfo(
                                    snResp.Session.AccountID,
                                    snResp.Session.Session,
                                    snResp.Session.WgPublicKey,
                                    snResp.Session.WgLocalIP,
                                    snResp.Session.WgKeyGenerated,
                                    snResp.Session.WgKeysRegenInerval);
                            }
                            __BlockingCollection.Add(snResp);
                        }
                        break;

                    case "SessionStatusResp":
                        var ssResp = JsonConvert.DeserializeObject<Responses.SessionStatusResponse>(line);
                        __BlockingCollection.Add(ssResp);
                        break;

                    case "SetAlternateDNSResp":
                        var adnsResp = JsonConvert.DeserializeObject<Responses.IVPNSetAlternateDnsResponse>(line);
                        __BlockingCollection.Add(adnsResp);
                        break;
                    case "EmptyResp":
                        var erResp = JsonConvert.DeserializeObject<Responses.IVPNEmptyResponse>(line);
                        __BlockingCollection.Add(erResp);
                        break;
                    case "ErrorResp":
                        var errResp = JsonConvert.DeserializeObject<Responses.IVPNErrorResponse>(line);
                        __BlockingCollection.Add(errResp);
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to process '{response.Command}'", ex);
            }
            return true;
        }

        private void NotifySessionInfo(string accountID,
            string session,
            string wgPublicKey,
            string wgLocalIP,
            Int64 wgKeyGenerated,
            Int64 wgKeyRotateInterval)
        {
            IPAddress.TryParse(wgLocalIP, out IPAddress wgLocalIPAddr);
            var s = new SessionInfo(
                accountID,
                session,
                wgPublicKey,
                wgLocalIPAddr,
                IVPN_Helpers.DataConverters.DateTimeConverter.FromUnixTime(wgKeyGenerated),
                new TimeSpan(0, 0, (int)wgKeyRotateInterval));
            SessionInfoChanged(s);
        }

        private T GetSyncResponse<T>() where T : Responses.IVPNResponse
        {
            var result = __BlockingCollection.TryTake(out var response, SYNC_RESPONSE_TIMEOUT_MS, __CancellationToken.Token);
            if (!result)
                throw new TimeoutException("SyncResponse took more time than expected.");

            if (response is Responses.IVPNErrorResponse errorResponse)
                throw new IVPNClientProxyException(errorResponse.ErrorMessage);

            T ret = null;
            try
            {
                ret = (T)response;
            }
            catch (Exception ex)
            {
                Logging.Info($"Error casting - expected:{typeof(T).FullName}, received:{response.GetType()}:" + ex);
            }
            return ret;
        }

        private void CheckConnected()
        {
            if (!ServiceConnected)
                throw new IVPNClientProxyNotConnectedException("Proxy is not connected to service");
        }

        private void SendRequest(   Requests.Request request)
        {
            Logging.Info("Sending: " + request);

            // Hello request can be sent without being in "connected
            if (!(request is Requests.Hello))
                CheckConnected();

            lock (__StreamWriter)
            {
                __StreamWriter.WriteLine(JsonConvert.SerializeObject(request));
                __StreamWriter.Flush();
            }            
        }

        public void SetPreference(string key, string value)
        {
            SendRequest(new Requests.SetPreference { Key = key, Value = value });
        }

        public void ConnectOpenVPN(OpenVPNVpnServer vpnServer, string multihopExitSrvId, DestinationPort port, IPAddress manualDns, string proxyType = "none", string proxyAddress = null, int proxyPort = 0, string proxyUsername = null, string proxyPassword = null)
        {
            Logging.Info($"[OpenVPN] Connect: {vpnServer}:{port} (proxy: {proxyType}: {proxyAddress})");

            SendRequest(new Requests.Connect
            {
                VpnType = VpnType.OpenVPN,
                CurrentDNS = manualDns.ToString(),
                OpenVpnParameters = new OpenVPNConnectionParameters()
                { 
                    EntryVpnServer = vpnServer,
                    MultihopExitSrvID = multihopExitSrvId,
                    Port = port,
                    ProxyType = proxyType,
                    ProxyAddress = proxyAddress,
                    ProxyPort = proxyPort,
                    ProxyUsername = proxyUsername,
                    ProxyPassword = proxyPassword
                }
            });
        }

        public void ConnectWireGuard(WireGuardVpnServerInfo vpnServer, DestinationPort port, IPAddress manualDns)
        {
            Logging.Info($"[WireGuard] Connect: {vpnServer})");

            SendRequest(new Requests.Connect
            {
                VpnType = VpnType.WireGuard,
                CurrentDNS = manualDns.ToString(),
                WireGuardParameters = new WireGuardConnectionParameters
                {
                    EntryVpnServer = vpnServer,
                    Port = port
                }
            });
        }

        public async Task SetCredentials(
            string AccountID,
            string Session,
            string OvpnUser,
            string OvpnPass,
            string WgPublicKey,
            string WgPrivateKey,
            string WgLocalIP,
            Int64 WgKeyGenerated)
        {
            var request = new Requests.SetCredentials {
                 AccountID= AccountID,
                 Session= Session,
                 OvpnUser= OvpnUser,
                 OvpnPass= OvpnPass,
                 WgPublicKey= WgPublicKey,
                 WgPrivateKey= WgPrivateKey,
                 WgLocalIP= WgLocalIP,
                 WgKeyGenerated= WgKeyGenerated
            };
            await SendSyncRequestAsync<Responses.IVPNEmptyResponse>(request);
        }

        public void Disconnect()
        {
            Logging.Info("Disconnecting...");

            SendRequest(new Requests.Disconnect());
        }

        public void PingServers(int pingTimeOutMs, int pingRetriesCount)
        {
            if (ServiceConnected)
                SendRequest(new Requests.PingServers { TimeOutMs = pingTimeOutMs, RetryCount = pingRetriesCount });
        }

        public void GenerateDiagnosticLogs(VpnType vpnProtocolType)
        {
            SendRequest(new Requests.GenerateDiagnostics {VpnProtocolType = vpnProtocolType});
        }

        public async Task<bool> KillSwitchGetIsEnabled()
        {
            return (await SendSyncRequestAsync<Responses.IVPNKillSwitchStatusResponse>(new Requests.KillSwitchGetStatus())).IsEnabled;
        }

        public async Task KillSwitchSetEnabled(bool isEnabled)
        {            
            var request = new Requests.KillSwitchSetEnabled() {IsEnabled = isEnabled};
            await SendSyncRequestAsync<Responses.IVPNEmptyResponse>(request);
        }

        public void KillSwitchSetAllowLAN(bool allowLAN)
        {
            var request = new Requests.KillSwitchSetAllowLAN {AllowLAN = allowLAN};
            SendRequest(request);
        }

        public void KillSwitchSetAllowLANMulticast(bool allowLANMulticast)
        {
            var request = new Requests.KillSwitchSetAllowLANMulticast {AllowLANMulticast = allowLANMulticast};
            SendRequest(request);
        }

        public async Task KillSwitchSetIsPersistent(bool isPersistent)
        {
            await SendSyncRequestAsync<Responses.IVPNEmptyResponse>(new Requests.KillSwitchSetIsPersistent() { IsPersistent = isPersistent });                            
        }

        public async Task<bool> KillSwitchGetIsPersistent()
        {
            return (await SendSyncRequestAsync<Responses.IVPNKillSwitchGetIsPestistentResponse>(
                    new Requests.KillSwitchGetIsPestistent())).IsPersistent;            
        }

        public async Task PauseConnection()
        {
            await SendSyncRequestAsync<Responses.IVPNEmptyResponse>(new Requests.PauseConnection());
        }

        public async Task ResumeConnection()
        {
            await SendSyncRequestAsync<Responses.IVPNEmptyResponse>(new Requests.ResumeConnection());
        }

        public async Task<bool> SetAlternateDns(IPAddress dns)
        {
            return (await SendSyncRequestAsync<Responses.IVPNSetAlternateDnsResponse>(new Requests.SetAlternateDns {DNS = dns.ToString()})).IsSuccess;
        }

        public async Task<Responses.SessionNewResponse> LogIn(string accountId, bool forceLogin)
        {
            return await SendSyncRequestAsync<Responses.SessionNewResponse>(new Requests.SessionNew { AccountID = accountId, ForceLogin = forceLogin });
        }

        public async Task<Responses.SessionStatusResponse> SessionStatus()
        {
            return await SendSyncRequestAsync<Responses.SessionStatusResponse>(new Requests.SessionStatus{});
        }

        public async Task LogOut()
        {
            try
            {
                await SendSyncRequestAsync<Responses.IVPNEmptyResponse>(new Requests.SessionDelete());
            }
            finally
            {
                SessionInfoChanged(null); // notify session removed
            }
        }

        public async Task WireGuardGeneratedKeys(bool onlyUpdateIfNecessary)
        {
            await SendSyncRequestAsync<Responses.IVPNEmptyResponse>(new Requests.WireGuardGenerateNewKeys { OnlyUpdateIfNecessary = onlyUpdateIfNecessary });
        }

        public async Task WireGuardKeysSetRotationInterval(Int64 interval)
        {
            await SendSyncRequestAsync<Responses.IVPNEmptyResponse>(new Requests.WireGuardSetKeysRotationInterval { Interval = interval });
        }

        private async Task<TResult> SendSyncRequestAsync<TResult>(Requests.Request request) where TResult : Responses.IVPNResponse
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

        public int ServicePort { get; private set; }
        public UInt64 Secret { get; private set; }

        public bool IsExiting => __IsExiting;
    }
}
