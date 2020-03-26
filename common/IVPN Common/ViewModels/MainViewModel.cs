using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;

using IVPN.Models;
using System.Threading;
using IVPN.Interfaces;

using System.Collections.Generic;
using System.Net;
using System.Timers;
using IVPN.Exceptions;
using IVPN.Models.Configuration;
using IVPN.VpnProtocols;
using IVPN.WiFi;
using IVPNCommon.Interfaces;
using IVPN.RESTApi;
using IVPN.Models.Session;

namespace IVPN.ViewModels
{
    public class MainViewModel : ViewModelBase, IProgress<string>
    {
        private ServiceState __ConnectionState;
        private bool __IsConnectionCancelAvailable; // workaround bug of fast connect cancel 

        private ConnectionInfo __ConnectionInfo;
        private System.Timers.Timer __DurationUpdateTimer;

        private ServerLocation __SelectedServer;
        private ServerLocation __SelectedExitServer;
        private ServerLocation __FastestServer;

        private string __InternalIPAddress;
        private string __ExternalIPAddress;

        private string __ConnectionProgressString;
        private string __ConnectionError;

        private readonly AppState __AppState;

        private readonly IAppNotifications __Notifications;
        private readonly IApplicationServices __AppServices;
        private readonly IAppNavigationService __NavigationService;

        private readonly IService __Service;
        private readonly IWiFiWrapper __WiFiWrapper;

        /// <summary>
        /// The main goal: if a user enabled firewall BEFORE connection - it means he wants to have the firewall enabled after disconnection.
        /// If a user enabled Firewall before connection to VPN - option "to deactivate Firewall on disconnect" has no influence on the Firewall on disconnect.
        /// If user changed Firewall Off\On when VPN is connected - option works as expected (will deactivate firewall on disconnect).
        /// </summary>
        private bool __FirewallAutoEnabled;

        private bool __KillSwitchIsPersistent;
        private readonly ISynchronizeInvoke __SyncInvoke;

        private bool __IsMultiHop;
        // for WireGuard and for 'basic' subscription plans multihop is not in use 
        private bool __IsAllowedMultiHop;
        private bool __IsAllowedPrivateEmails;

        // false - when "Connecting" started. We need to know this to process.
        // It is necesary to process only first 'Disconnected' event.
        //
        // We can receive multiple 'Disconnected' events with failude description for one 'connect' try.
        // So, we processing only first event.
        private bool __IsDisconnectFailureProcessed = true;

        /// <summary>
        /// We need to update selected servers when servers list was updated.
        /// But we can do it only in 'Disconnected' state .
        /// Therefore, we trying to update them immediately after disconnection (if __IsNecessaryToUpdateSelectedServers == true)
        /// </summary>
        private bool __IsNecessaryToUpdateSelectedServers;

        /// <summary>
        /// Fastest server can be updated only in 'Disconnected' state.
        /// If we can not update it in current moment - save it to have updated when switching to 'Disconnected'
        /// </summary>
        private ServerLocation __LastFastestServerToUpdate;

        public WireguardKeysManager WireguardKeysManager { get; }

        public delegate void OnAccountSuspendedDelegate (SessionStatus account);
        public event OnAccountSuspendedDelegate OnAccountSuspended = delegate { };

        public MainViewModel(
            AppState appState,
            ISynchronizeInvoke syncInvoke,
            IAppNavigationService navigationService,
            IAppNotifications appNotifications,
            IApplicationServices appServices,
            IService service)
        {
            __AppState = appState;
            __SyncInvoke = syncInvoke;
            __NavigationService = navigationService;
            __Notifications = appNotifications;
            __AppServices = appServices;

            __Service = service;

            WireguardKeysManager = new WireguardKeysManager(
                isCanUpdateKey: () =>
                {
                    return Settings.IsUserLoggedIn()
                        && Settings.VpnProtocolType == VpnType.WireGuard
                        && (ConnectionState == ServiceState.Connected || ConnectionState == ServiceState.Connecting);
                },
                appSettings: Settings,
                appServices: __AppServices);

            SettingsCommand = new RelayCommand(OpenSettings);
            SelectServerCommand = new RelayCommand(SelectServerExecuted);
            SelectEntryServerCommand = new RelayCommand(SelectEntryServerExecuted);
            SelectExitServerCommand = new RelayCommand(SelectExitServerExecuted);

            ConnectCommand = new RelayCommand(Connect);
            DisconnectCommand = new RelayCommand(Disconnect);
            PauseCommand = new RelayCommand<double>(PauseStart);
            ResumeCommand = new RelayCommand(Resume);

            ConnectionState = __Service.State;

            IsMultiHop = Settings.IsMultiHop;

            // WiFi monitor (should be initialized before execution ServiceInitializedAsync)
            __WiFiWrapper = Platform.GetImplementation(Platform.PlatformImplementation.WiFi) as IWiFiWrapper;
            if (__WiFiWrapper != null)
                __WiFiWrapper.WiFiStateChanged += WiFiWrapperOnWiFiStateChanged;

            __Service.PropertyChanged += Service_PropertyChanged;
            __Service.Servers.OnFasterServerDetected += UpdateFastestServer;
            __Service.Servers.PropertyChanged += delegate (object sender, PropertyChangedEventArgs args)
            {
                if (args.PropertyName == nameof(__Service.Servers.ServersList))
                {
                    UpdateSelectedServers();

                    // If we still do not have SessionStatus - we can try to do new API request (can be used alternate API IPs from servers.json)
                    // Do SessionStatus update ONLY if it still not received.
                    if (__AppState.SessionStatusInfo == null)
                        __AppState.SessionManager.RequestStatusCheck();
                }
            };

            CheckCapabilities();
            __AppState.OnSessionStatusChanged += (SessionStatus sessionStatus) => { CheckCapabilities(); };

            __Service.Disconnected += Disconnected;
            __Service.ServiceInitialized += ServiceInitializedAsync;
            __Service.Connected += (ConnectionInfo connectionInfo) =>
            {
                // If current connection info is not null - it means there was no disconnection
                // It can be when vpn was reconnected (for example, because of WireGuard credentials was changed)
                var oldConnInfo = ConnectionInfo;
                if (oldConnInfo != null)
                    connectionInfo.SetConnectTime(oldConnInfo.ConnectTime);

                // save connection info
                ConnectionInfo = connectionInfo;
            };

            Settings.PropertyChanged += (object sender, PropertyChangedEventArgs e) => {
                if (e.PropertyName.Equals(nameof(Settings.VpnProtocolType)))
                {
                    CheckCapabilities();
                } else if (e.PropertyName.Equals(nameof(Settings.IsAntiTracker)))
                {
                    RaisePropertyWillChange(nameof(IsAntiTrackerEnabled));
                    RaisePropertyChanged(nameof(IsAntiTrackerEnabled));
                }
            };

            Settings.OnWireguardCredentialsChanged += Settings_OnWireguardCredentialsChanged;

            __KillSwitchIsPersistent = __Service.KillSwitchIsPersistent;

            Platform.PowerModeChanged += OnPowerChange;

            // SM-APP-1100 If session is not valid (user received Session not found error) client app have to assume that the user was forcibly logged out and present user with the “Log In” form.
            __AppState.SessionManager.OnSessionRequestError += (IVPNRestRequestApiException ex) => { ProcessApiErrorResponse(ex.ApiStatusCode); };
        }

        private void CheckCapabilities()
        {
            // MultiHop
            if (AppState.Capabilities.Contains(Constants.CAPABILITIES_MULTIHOP) == false // Multihop is not available for current pricing plan
                || Settings.VpnProtocolType == VpnType.WireGuard) // MiltiHop not supported by WireGuard
            {
                IsAllowedMultiHop = false;
            }
            else
                IsAllowedMultiHop = true;

            // PrivateEmails
            if (AppState.Capabilities.Contains(Constants.CAPABILITIES_PRIVATE_EMAILS) == false)
                IsAllowedPrivateEmails = false;
            else
                IsAllowedPrivateEmails = true;
        }

        // Returns TRUE - when status is processed
        private bool ProcessApiErrorResponse(ApiStatusCode statusCode)
        {
            if (statusCode == ApiStatusCode.SessionNotFound)
            {
                ConnectionState = __Service.State;

                if (!(ConnectionState==ServiceState.Uninitialized || ConnectionState == ServiceState.Disconnected || ConnectionState == ServiceState.Disconnecting))
                    return false;
                
                // Session is not valid - moving user to log-in page
                __NavigationService.NavigateToLogOutPage(NavigationAnimation.FadeToRight);
                return true;
            }

            return false;
        }

        private void OnPowerChange(Object sender, PowerModeChangedEventArgs e)
        {         
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    if (__ConnectionState == ServiceState.Connected)
                        __Service.Suspend();

                    break;

                case PowerModes.Resume:
                    if (__Service.IsSuspended)
                        __Service.Resume();

                    break;
            }
        }

        public void StartDurationUpdateTimer()
        {            
            if (__DurationUpdateTimer != null)
                return;

            var timer = new System.Timers.Timer();
            __DurationUpdateTimer = timer;

            timer.Interval = 1000; // 1 second
            timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                __SyncInvoke.BeginInvoke(new Action(() => { __ConnectionInfo?.UpdateDuration(); }), null);
            };
            timer.Start();            
        }        

        public void StopDurationUpdateTimer()
        {
            if (__DurationUpdateTimer == null)
                return;

            if (!__DurationUpdateTimer.Enabled)
                return;
            
            __DurationUpdateTimer.Stop();
            __DurationUpdateTimer = null;
        }
        
        async void ServiceInitializedAsync(object sender, EventArgs e)
        {
            __Service.KillSwitchAllowLANMulticast = Settings.FirewallAllowLANMulticast;
            __Service.KillSwitchAllowLAN = Settings.FirewallAllowLAN;

            if (Settings.FirewallLastStatus)
                IsKillSwitchEnabled = true;
            else
            {
                // This will update the Kill Switch status from the server
                RaisePropertyChanged(nameof(IsKillSwitchEnabled));
            }

            if (__Service.KillSwitchIsPersistent)
                Settings.FirewallType = IVPNFirewallType.Persistent;
            else
                Settings.FirewallType = IVPNFirewallType.Manual;

            // Remove references to unused(removed) servers or IPs from configuration (if necesary)
            // We can do this only once - on application start
            bool isSettingsChanged = Settings.ServersFilter.NormalizeData(__Service.Servers.ServersList.Select(x => x.VpnServer), Settings.VpnProtocolType);
            if (isSettingsChanged)
                Settings.Save();

            RaisePropertyChanged(nameof(Settings));

            if (__WiFiWrapper!=null)
                WiFiWrapperOnWiFiStateChanged(__WiFiWrapper.CurrentState);

            if (Settings.FirewallType == IVPNFirewallType.Persistent && !IsKillSwitchEnabled)
                IsKillSwitchEnabled = true;

            if (Settings.AutoConnectOnStart)
                await ConnectToLastServer();
        }
        
        void Service_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(__Service.KillSwitchIsEnabled))
            {
                RaisePropertyWillChange(() => IsKillSwitchEnabled);
                __IsKillSwitchEnabled = __Service.KillSwitchIsEnabled;
                RaisePropertyChanged(() => IsKillSwitchEnabled);
            }
            else if (e.PropertyName == nameof(__Service.KillSwitchIsPersistent))
            {
                RaisePropertyWillChange(() => KillSwitchIsPersistent);
                __KillSwitchIsPersistent = __Service.KillSwitchIsPersistent;
                RaisePropertyChanged(() => KillSwitchIsPersistent);

            }
            else if (e.PropertyName == nameof(__Service.State))
            {
                // Save that connecting process started 
                if (__Service.State == ServiceState.Connecting)
                {
                    __IsDisconnectFailureProcessed = false;
                    ConnectionState = ServiceState.Connecting;

                }

                // MainViewModel ConnectionState is assigned Disconnected only after
                // Disconnected() handler finishes executing, and not when Service becomes Disconnected.
                if (ConnectionState == ServiceState.Uninitialized ||
                    __Service.State != ServiceState.Disconnected)
                {
                    ConnectionState = __Service.State;

                    // if openVPN reconnected and we was in 'paused' state -> keep staying in 'paused' (update routing table)
                    if (ConnectionState == ServiceState.Connected 
                        && PauseStatus == PauseStatusEnum.Paused)
                    {
                        // Stay in Paused stage
                        __Service.PauseOn();
                    }
                }

                if (ConnectionState == ServiceState.ReconnectingOnService ||
                    ConnectionState == ServiceState.ReconnectingOnClient)
                    Report("Reconnecting...");

                if (ConnectionState == ServiceState.Connected
                    && Settings.IsAutoPortSelection)
                {
                    // Save port information if it was changed.
                    // perform save in background thread to avoid GUI freeze
                    if (!Settings.PreferredPort.Equals(__Service.ConnectionTarget.Port))
                    {
                        //TODO: I do not like this code ((. We should not care about VpnType here. Try to rework in future.
                        if (Settings.VpnProtocolType == VpnType.OpenVPN)
                            Settings.PreferredPort = __Service.ConnectionTarget.Port;
                        else if (Settings.VpnProtocolType == VpnType.WireGuard)
                            Settings.WireGuardPreferredPort = __Service.ConnectionTarget.Port;

                        Settings.Save();
                    }                    
                }

                if (ConnectionState == ServiceState.Connected ||
                    ConnectionState == ServiceState.Disconnected)
                {
                    // Perform account check only if something wrong with account (it is not active OR are going to expire OR it is trial)
                    if (__AppState?.SessionStatusInfo == null 
                        || __AppState.SessionStatusInfo.IsActive == false
                        || __AppState.SessionStatusInfo.IsOnFreeTrial 
                        || (__AppState.SessionStatusInfo.WillAutoRebill == false && (__AppState.SessionStatusInfo.ActiveUtil - DateTime.Now).TotalDays < 4)
                    )
                    {
                        __AppState.SessionManager.RequestStatusCheck();
                    }
                }
            }
        }

        void Settings_OnWireguardCredentialsChanged(object sender, EventArgs e)
        {
            // WireGuard credentials are changed
            // If we are already connected (WG) -> send to service new credantioal to update current connection 
            if (Settings.VpnProtocolType != VpnType.WireGuard 
                  || Settings.IsWireGuardCredentialsAvailable() == false)
                return;

            if (ConnectionState != ServiceState.Connected
                && ConnectionState != ServiceState.ReconnectingOnService
                && ConnectionState != ServiceState.ReconnectingOnClient)
                return;

            Connect();
        }

        public async Task ConnectToLastServer()
        {
            if (SelectedServer == null)
                return;

            if (ConnectionState != ServiceState.Disconnected)
                return;

            await ConnectToServer();
        }

        private void UpdateFastestServer(ServerLocation location)
        {
            if (ConnectionState != ServiceState.Disconnected && ConnectionState != ServiceState.Uninitialized)
            {
                __LastFastestServerToUpdate = location;
                return;
            }
            __LastFastestServerToUpdate = null;
                       
            FastestServer = location;
        }

        private void UpdateSelectedServers()
        {
            if (__Service.Servers == null || __Service.Servers.ServersList.Count == 0)
                return;

            if (ConnectionState != ServiceState.Disconnected && ConnectionState != ServiceState.Uninitialized)
            {
                __IsNecessaryToUpdateSelectedServers = true;
                return;
            }

            __IsNecessaryToUpdateSelectedServers = false;

            ServerLocation newEntryServer = GetNewServerById(SelectedServer, __AppState.Settings.GetLastUsedServerId(isExitServer: false), out var isDefaultSelectedServer);

            if (newEntryServer != null) 
                SetSelectedServer(newEntryServer);

            if (isDefaultSelectedServer)
            {
                // Was selected first default server. Probably, it is a first application start.
                // Save this info to try to find best server before connection
                IsAutomaticServerSelection = true;
            }

            ServerLocation newExitServer = GetNewServerById(SelectedExitServer, __AppState.Settings.GetLastUsedServerId(isExitServer: true), out isDefaultSelectedServer);
            
            if (newExitServer == null)
                return;

            if (newEntryServer!=null 
                && newExitServer.CountryCode == newEntryServer.CountryCode)
            {
                newExitServer = __Service.Servers.ServersList.FirstOrDefault(s => s.CountryCode != newEntryServer.CountryCode);
                if (newExitServer == null)
                    newExitServer = newEntryServer;
            }

            SetSelectedExitServer(newExitServer);

            // fastest server
            if (FastestServer == null)
            {   // load fastest server from settings (only if it not defined)
                string fsId = __AppState.Settings.GetLastFastestServerId();                
                if (string.IsNullOrEmpty(fsId) == false)
                {                    
                    ServerLocation fs = GetNewServerById(null, fsId, out isDefaultSelectedServer);
                    if (fs != null && isDefaultSelectedServer == false)
                    {
                        FastestServer = fs;
                    }
                }
            }
        }

        private ServerLocation GetNewServerById(ServerLocation currentServerLocation, string fallbackServerId, out bool isDefaultSelectedServer)
        {
            ServerLocation newServer = null;
            isDefaultSelectedServer = false;

            if (currentServerLocation != null)
                newServer = __Service.Servers.ServersList.FirstOrDefault(
                    s => s.VpnServer.GatewayId == currentServerLocation.VpnServer.GatewayId);

            if (newServer == null)
                newServer = __Service.Servers.ServersList.FirstOrDefault(s => s.VpnServer.GatewayId == fallbackServerId);

            if (newServer == null)
            {
                newServer = __Service.Servers.ServersList.First();
                isDefaultSelectedServer = true;
            }

            return newServer;
        }

        public void Connect()
        {
            // Run connect command asynchronously
            // Unsuccessfull connection is handled in Service_Disconnected event
            Task.Run(async () =>
            {
                try
                {
                    await ConnectToServer();
                }
                catch(Exception ex)
                {
                    Logging.Info("Connect failed: " + ex);
                    __SyncInvoke.Invoke(new Action(() => { throw ex; }), null);
                }
            });
        }

        private async Task ConnectToServer()
        {
            try
            {
                if (!Settings.GetVpnCredentials(out var username, out var password))
                {
                    Logging.Info("[ERROR] ConnectToServer(): Unable to connect. VPN user credentials not found.");

                    __NavigationService.NavigateToLogOutPage(NavigationAnimation.FadeToRight);
                    NotifyError("VPN credentials are empty", "You have been redirected to the login page to re-enter your credentials.");

                    return;
                }

                __IsRequiredReconnectOnResume = false;

                PauseStatus = PauseStatusEnum.Resumed;

                // we need to block GUI from changing server during we are preparing to connect (searching fastest server OR keys generation)
                ConnectionState = ServiceState.Connecting;

                // SESSION: Create new session (if still not created)
                // SM-APP-200 After upgrade: Create new session the next time user presses the “Connect” button manually.
                if (!Settings.IsSessionAvailable())
                {
                    try
                    {
                        var accResp = await __AppState.SessionManager.CreateNewSessionAsync(username, password, new CancellationTokenSource().Token, isForceDeleteAllSessions: false, Settings.WireGuardClientPublicKey);
                        // If wireguard public key successfuly registered - save wireguard credentials
                        if (accResp.WGIPAddress != null && accResp.WGIPAddress != default(IPAddress))
                        {
                            __AppState.Settings.SetWireGuardCredentials(Settings.WireGuardClientPrivateKeySafe, Settings.WireGuardClientPublicKey, true, accResp.WGIPAddress.ToString());
                        }
                        else
                            __AppState.Settings.ResetWireGuardCredentials();
                    }
                    catch (TimeoutException)
                    {
                        // Keep using old credentials
                    }
                    catch (IVPNRestRequestApiException ex)
                    {
                        Logging.Info("EXCEPTION during session creation on Connect (API request): " + ex);

                        // SM-APP- 220  When performing SM-APP-200, all errors, other than Authentication error, or session limit errors have to be ignored
                        // SM-APP- 221  When performing SM-APP-200, Authentication error have to result in presenting user with "Log In" screen
                        // SM-APP- 222  When performing SM-APP-200, session limit error should result in presenting "connection limit reached" dialog to the user.
                        if (ex.ApiStatusCode == ApiStatusCode.Unauthorized)
                        {
                            __NavigationService.NavigateToLogOutPage(NavigationAnimation.FadeToRight);
                            return;
                        }
                        else if (ex.ApiStatusCode == ApiStatusCode.SessionTooManySessions)
                        {
                            __NavigationService.NavigateToSessionLimitPage(NavigationAnimation.FadeToRight);
                            return;
                        }

                        Logging.Info("EXCEPTION during session creation on Connect (API request): " + ex);
                        // Session creation failed. But if no SessionLimit and not credentials error - keep using old credentials.
                    }
                    catch (Exception ex)
                    {
                        Logging.Info("EXCEPTION during session creation on Connect (API request): " + ex);
                        NotifyError(__AppServices.LocalizedString("Error_FailedToCreateSession", "IVPN: Failed to create new session"), $"{ex.Message}");
                        return;
                    }
                }

                // WIREGUARD: regenerate keys (if necessary)
                if (Settings.VpnProtocolType == VpnType.WireGuard)
                {
                    bool isUpgradeSuccess = false;
                    IVPNRestRequestApiException exp = null;
                    try
                    {
                        isUpgradeSuccess = await WireguardKeysManager.RegenerateKeysIfNecessary();
                    }
                    catch (IVPNRestRequestApiException ex)
                    {
                        if (ex.ResponseWithStatus.Status == ApiStatusCode.WgPublicKeyNotFound)
                        {
                            Logging.Info("[ERROR] Failed to regenerate WireGuard keys: " + ex);

                            // The old (current) WG key is no exists on backend (backend bug???)
                            // Trying to generate new key (active key is not in use for this request). 
                            try
                            {
                                await WireguardKeysManager.GenerateNewKeysAsync();
                                isUpgradeSuccess = true;
                            }
                            catch (Exception e)
                            {
                                Logging.Info("Failed to generate new key (attempt after regeneration failed) " + e);
                            }
                        }

                        // check if new call 'GenerateNewKeysAsync()' not fixed the issue
                        if (isUpgradeSuccess == false)
                        { 
                            if (ProcessApiErrorResponse(ex.ApiStatusCode))
                                return;

                            exp = ex;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Info("[ERROR] Failed to regenerate WireGuard keys: " + ex);
                    }
                    
                    if (isUpgradeSuccess == false || exp!=null)
                    {
                        // WG keys generation failed
#if BETA_WG_GENERATION_1MIN
#warning "!!!!!!!!!!!!!!!!!!! BETA_WG_GENERATION_1MIN !!!!!!!!!!!!!!!!!!!!!!!"
                        int minsPassed = (int)(DateTime.Now - WireguardKeysManager.KeysExpiryDate).TotalMinutes;
                        if (exp!=null || minsPassed > 3)
#elif BETA_WG_GENERATION_10MINS
#warning "!!!!!!!!!!!!!!!!!!! BETA_WG_GENERATION_10MINS !!!!!!!!!!!!!!!!!!!!!!!"
                        int minsPassed = (int)(DateTime.Now - WireguardKeysManager.KeysExpiryDate).TotalMinutes;
                        if (exp!=null || minsPassed > 3*10)
#elif BETA_WG_GENERATION_HOURS
#warning "!!!!!!!!!!!!!!!!!!! BETA_WG_GENERATION_HOURS !!!!!!!!!!!!!!!!!!!!!!!"
                        int hoursPassed = (int)(DateTime.Now - WireguardKeysManager.KeysExpiryDate).TotalHours;
                        if (exp!=null || hoursPassed > 3)
#else
                        int daysPassed =(int)(DateTime.Now - WireguardKeysManager.KeysExpiryDate).TotalDays;
                        if (exp!=null || daysPassed > 3)
#endif
                        {
                            ConnectionState = __Service.State;
                            
                            NotifyError(
                                __AppServices.LocalizedString("Error_WireGuardRegenerationFailedOnConnect", "Failed to automatically regenerate WireGuard keys"),
                                __AppServices.LocalizedString(
                                    "Error_WireGuardRegenerationFailedOnConnectDetailed",
                                    "Cannot connect using WireGuard protocol: regenerating WireGuard keys failed. This is likely because of no access to the IVPN API server."
                                    + Environment.NewLine + Environment.NewLine + "You can retry connection, regenerate keys manually from preferences, or select another protocol. Please contact support if this error persists."
                                    + ((exp != null)? Environment.NewLine + Environment.NewLine + $"({exp.Message})": "")
                                    )
                                );
                            return;
                        }
                    }
                }

                // For automatic server selection, if fastest server still not detected
                // (automatic server selection is disabled for Multi-Hop)
                if (!IsMultiHop 
                    && IsAutomaticServerSelection 
                    && IsFastestServerInUse == false)
                {
                    SetFastestServer(await __Service.Servers.GetFastestServerAsync(), false);
                }
                
                if (SelectedServer == null)
                    throw new Exception("Internal error: selected server not defined");
                if (SelectedServer.VpnServer == null)
                    throw new Exception("Internal error: selected server details not defined");
                if (Settings.VpnProtocolType == VpnType.WireGuard && !Settings.IsWireGuardCredentialsAvailable())
                {
                    NotifyError("WireGuard credentials not defined. Please, re-generate WireGuard keys");
                    return;
                }

                // Get custon DNS from configuration
                IPAddress manualDns;
                try
                {
                    manualDns = GetCustomDns();
                }
                catch (IVPNExceptionUserErrorMsg ex)
                {
                    RaisePropertyWillChange(nameof(IsAntiTrackerEnabled));
                    Settings.IsAntiTracker = false;
                    RaisePropertyChanged(nameof(IsAntiTrackerEnabled));

                    NotifyError(ex.Caption, ex.Message);
                    return;
                }

                // Get proxy options
                var proxyOptions = Settings.GetProxyOptions(SelectedServer.VpnServer.GetHostsIpAddresses()[0], GetPreferredPort(SelectedServer).Port);

                CancellationToken cancelToken = new CancellationToken();

                ConnectionProgressString = __AppServices.LocalizedString("Progress_Connecting");
                ConnectionError = null;
                IsConnectionCancelAvailable = true;

                // Enable firewall (if necessary)
                if (Settings.FirewallAutoOnOff)
                {
                    if (!IsKillSwitchEnabled)
                    {
                        IsKillSwitchEnabled = true;
                        __FirewallAutoEnabled = true;
                    }
                }

                // CONNECT

                var connectionTarget = new ConnectionTarget(
                    SelectedServer,
                    GetPreferredPort(SelectedServer),
                    GetPortsToReconnect(),
                    manualDns,
                    GetAdjustedUsername(username), password,
                    proxyOptions,
                    Settings.WireGuardClientInternalIp,
                    Settings.GetWireGuardClientPrivateKey());

                ConnectionResult result = await __Service.Connect(this, cancelToken, connectionTarget);

                if (result.Success)
                {
                    UpdateTrayIconOnConnect(result);
                    StartDurationUpdateTimer();
                }
            }
            finally
            {
                // ensure that connection state represents correct value (we changed it manually on the begining of this method)
                ConnectionState = __Service.State;
            }
        }

        private string GetAdjustedUsername(string username)
        {
            // MultiHop configuration is based just by adding "@exit_server_id" to the end of username
            // And forwarding this info on server
            if (IsMultiHop)
                return $"{username}@{SelectedExitServer.MultihopId}";

            return username;
        }

        private void ConnectionInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ConnectionInfo.DurationString) && ConnectionState == ServiceState.Connected)
            {
                __Notifications.UpdateConnectedTimeDuration((SelectedServer==null)? "" : SelectedServer.Name, ConnectionInfo.DurationString);
            }
        }

        async void Disconnected (bool failure, IVPNServer.DisconnectionReason reason, string reasonDescription)
        {
            if (__Service.State == ServiceState.ReconnectingOnClient)
            {
                Logging.Info("Disconnected event received. Reconnecting required. Reconnect...");
                Connect();
                return;
            }
            
            await DoDisconnectedAsync (failure, reason, reasonDescription);
        }

        async Task DoDisconnectedAsync(bool failure, IVPNServer.DisconnectionReason reason, string reasonDescription)
        {
            Logging.Info("Disconnected event received");

            if (ConnectionInfo != null) 
            {
                ConnectionInfo.PropertyChanged += ConnectionInfo_PropertyChanged;
                __Notifications.ShowDisconnectedNotification ();
            }

            StopDurationUpdateTimer();
            ConnectionInfo = null;

            if (Settings.FirewallType == IVPNFirewallType.Manual 
                && Settings.FirewallAutoOnOff 
                && __FirewallAutoEnabled)
                    IsKillSwitchEnabled = false;

            await ProcessDisconnectedReason (failure, reason, reasonDescription);
        }

        async Task ProcessDisconnectedReason(bool failure, IVPNServer.DisconnectionReason reason, string reasonDescription)
        {
            // We can receive multiple 'Disconnected' events with fail description for one 'connect' try.
            // So, we processing only first event.
            if (__IsDisconnectFailureProcessed) 
                return;
            __IsDisconnectFailureProcessed = true;

            try
            {
                SessionStatus sessionStatus = null;                
                if (failure)
                {
                    // When account is suspended - openvpn fails with authentication error.
                    // To check is the account is suspended or it is really wrong credentails - we are sending Acount status request
                    if (reason == IVPNServer.DisconnectionReason.AuthenticationError) 
                    {
                        if (IsKillSwitchEnabled == false) // when KillSwitch enabled - no sense to request status. Request will be blocked.
                        {
                            try
                            {
                                sessionStatus = await __AppState.SessionManager.CheckStatusNowAsync(5000);
                            }
                            catch (IVPNRestRequestApiException ex)
                            {
                                if (ex.ApiStatusCode == ApiStatusCode.SessionNotFound)
                                    return; // this error already processed by 'SessionManager.OnSessionRequestError' event handler. Do nothing. 

                                if (ex.ApiStatusCode == ApiStatusCode.Unauthorized)
                                {
                                    ConnectionState = ServiceState.Disconnected;

                                    // Wrong username or password - moving user to log-in page
                                    __NavigationService.NavigateToLogOutPage(NavigationAnimation.FadeToRight);
                                    ConnectionError = __AppServices.LocalizedString("Error_AuthenticationGoToLogInPage");
                                    return;
                                }
                            }
                            catch
                            {
                                sessionStatus = null;
                                // ignore all
                            }                            
                        }

                        /*
                         * It is required by the feature “Skip and continue”: When a user can skip LogIn procedure (skip login\pass verification) 
                         * 
                        // If user credentials still not checked by IVPN API server, 
                        // it means, there was no possibility to access to it
                        //  - LogOut user and show LogIn page
						if  (__AppState.SessionStatusInfo == null // account info should not be null here. Otherwise - logout.
						     || sessionStatus==null)
                        {
                            ConnectionState = ServiceState.Disconnected;

                            // Wrong username or password - moving user to log-in page
                            __NavigationService.NavigateToLogOutPage(NavigationAnimation.FadeToRight);
                            ConnectionError = __AppServices.LocalizedString("Error_AuthenticationGoToLogInPage");
                            return;
                        }*/

                        ConnectionState = __Service.State;
                        if (sessionStatus != null) 
                        {
                            if (!sessionStatus.IsActive) 
                            {
                                // raise event "account suspended"
                                OnAccountSuspended (sessionStatus);
                                reasonDescription = null;
                            }
                        } 
                        else 
                        {
                            // Unable to get latest account status info
                            // So, we can make conclusion based only on account 'ExpirationDate' of last known account status (saved in AppState)
                            if (__AppState.SessionStatusInfo != null
                                && __AppState.SessionStatusInfo.ActiveUtil != default
                                && __AppState.SessionStatusInfo.ActiveUtil <= DateTime.Now)
                            {
                                ConnectionError = __AppServices.LocalizedString ("Error_ConnectionError_AccountExpiredOrWrongPassword");
                                reasonDescription = null;
                            }
                        }
                    } 

                    ConnectionError = reasonDescription;
                }

                // SM-APP-1000 Client app have to check, whether session_token is valid after each disconnection event using /session/status call.
                // SM-APP-1010 Client app have to check, whether session_token is valid after each connection error as well
                if (sessionStatus == null)
                {
                    if (IsKillSwitchEnabled == false) // when KillSwitch enabled - no sense to request status. Request will be blocked.
                        __AppState.SessionManager.RequestStatusCheck();
                }
            }
            finally
            {
                ConnectionState = __Service.State;
            }
        }

        private void UpdateTrayIconOnConnect(ConnectionResult result)
        {
            string baloonText =
                $"You are now connected to {SelectedServer.Name}. Your internal IP address is: {result.ConnectionInfo.ClientIPAddress}";
            
            __Notifications.ShowConnectedTrayBaloon(baloonText);
        }

        private DestinationPort GetPreferredPort(ServerLocation server)
        {
            if (Settings.VpnProtocolType == VpnType.OpenVPN)
            {
                var port = Settings.PreferredPort;

                // When HTTP proxy is selected, only TCP protocols are preferred
                if (port.Protocol == DestinationPort.ProtocolEnum.UDP &&
                    Settings.ProxyType == ProxyType.Http)
                {
                    var tcpPort = Settings.PreferredPortsList.FirstOrDefault(p => p.Protocol == DestinationPort.ProtocolEnum.TCP);
                    if (tcpPort != null)
                        port = tcpPort.DestinationPort;
                }
                return port;
            }

            if (Settings.VpnProtocolType == VpnType.WireGuard)            
                return Settings.WireGuardPreferredPort;

            throw new Exception("Unexpected VPN type");
        }

        private List<DestinationPort> GetPortsToReconnect()
        {
            if (Settings.VpnProtocolType == VpnType.OpenVPN)
            {
                List<DestinationPort> ret = new List<DestinationPort>();

                if (Settings.IsAutoPortSelection == false)
                    return ret;

                foreach (ServerPort port in Settings.PreferredPortsList)
                {
                    // When HTTP proxy is selected, only TCP protocols are preferred
                    if (Settings.ProxyType == ProxyType.Http
                        && port.Protocol == DestinationPort.ProtocolEnum.UDP)
                        continue;

                    ret.Add(port.DestinationPort);
                }
                return ret;
            }
            else if (Settings.VpnProtocolType == VpnType.WireGuard)
            {
                // TODO: is 'IsAutoPortSelection' common for all VPN types?
                //if (Settings.IsAutoPortSelection == false)
                    return new List<DestinationPort>(); 

                //return Settings.WireGuardPreferredPortsList.Select(x => x.DestinationPort).ToList(); 
            }

            throw new Exception("Unexpected VPN type");
        }

        public void Report(string progressStatus)
        {
            ConnectionProgressString = progressStatus;
            IsConnectionCancelAvailable = true;
        }

        private void CancelConnect()
        {
            IsConnectionCancelAvailable = false;
            if (__Service.State != ServiceState.Disconnected) 
                __Service.Disconnect ();
        }

        private void Disconnect()
        {
            try
            {
                DoDisconnect();
            }
            catch (Exception ex)
            {
                Logging.Info("[ERROR] Disconnection error: " + ex.Message);
                NotifyError("Disconnection error", ex.Message);
            }
        }

        private void DoDisconnect()
        {
            if (PauseStatus == PauseStatusEnum.Paused)
                ResumeKillSwitch();
            PauseStatus = PauseStatusEnum.Resumed;

            if (__Service.State != ServiceState.Disconnected)
            {
                ConnectionProgressString = __AppServices.LocalizedString("Progress_Disconnecting");
                __Service.Disconnect();
            }
        }

        private void OpenSettings()
        {
            __NavigationService.ShowSettingsWindow();
            RaisePropertyChanged(nameof(Settings));

            ApplySettings();
        }

        public void ApplySettings()
        {
            // apply network action for current connected network
            ApplyActionForCurrentNetwork();

            //Change DNS (if necessary)
            Task.Run(async () => await SetCustomDnsIfNecessary());
        }

        private void SelectServerExecuted()
        {
            __NavigationService.NavigateToServerSelection();
        }

        private void SelectEntryServerExecuted()
        {
            __NavigationService.NavigateToEntryServerSelection();
        }

        private void SelectExitServerExecuted()
        {
            __NavigationService.NavigateToExitServerSelection();
        }

        public ICommand SettingsCommand { get; }

        public ICommand ConnectCommand { get; }

        public ICommand DisconnectCommand { get; }

        public RelayCommand<double> PauseCommand { get; }

        public ICommand ResumeCommand { get; }
        
        public ServiceState ConnectionState
        {
            get => __ConnectionState;

            private set
            {
                if (__ConnectionState == value)
                    return;
                
                RaisePropertyWillChange();
                __ConnectionState = value;
                RaisePropertyChanged();

                if (__ConnectionState == ServiceState.Disconnected)
                {
                    // UpdateSelectedServersIfNecessary
                    if (__IsNecessaryToUpdateSelectedServers)
                        UpdateSelectedServers();

                    // UpdateFastestServerIfNecessary
                    if (__LastFastestServerToUpdate != null)
                        UpdateFastestServer(__LastFastestServerToUpdate);
                }
            }
        }

#region Selected servers
        public bool IsAutomaticServerSelection
        {
            get => Settings.GetIsAutomaticServerSelection();
            set
            {
                RaisePropertyWillChange();
                if (Settings.GetIsAutomaticServerSelection() != value)
                {
                    Settings.SetIsAutomaticServerSelection(value);
                    // perform save in background thread to avoid GUI freeze
                    Task.Run(() => Settings.Save());
                }
                RaisePropertyChanged();

                if (value)
                {
                    if (__ConnectionState != ServiceState.Disconnected)
                    {
                        // When connected - pings update will not pe started.
                        // We are using an old data
                        FastestServer = __Service.Servers.GetFastestServer();
                    }
                    else
                        ReInitializeFastestSever();
                }
            }
        }

        public void ReInitializeFastestSever()
        {
            if (!IsAutomaticServerSelection)
                return;

            FastestServer = null;

            // find fastest server: start pings update
            if (ConnectionState != ServiceState.Disconnected
                || __Service.Servers.StartPingUpdate() == false)
            {
                // If VPN is connected - no sense to start pings (Service will ignore this request)
                // in this case, select fastest server using old data
                //
                // Pings update was not started (probably, it just finished few seconds ago)
                // So, we are using an old data
                FastestServer = __Service.Servers.GetFastestServer();
            }
        }

        public bool IsFastestServerInUse
        {
            get
            {
                try
                {
                    if (FastestServer == null || SelectedServer == null)
                        return false;
                    return SelectedServer.VpnServer.GatewayId == FastestServer.VpnServer.GatewayId;
                }
                catch
                {
                    return false;
                }
            }
        }

        public ServerLocation SelectedServer
        {
            get => __SelectedServer;
            set
            {
                // When server was selected from UI - disable IsAutomaticServerSelection
                // (do not disable it only when IsMultiHop, because this functionality disabled for Multi-Hop, so it keeps state when switch back to SingleHop)
                if (!IsMultiHop)
                    IsAutomaticServerSelection = false;

                SetSelectedServer(value);
            }
        }

        public ServerLocation FastestServer
        {
            get => __FastestServer;
            set => SetFastestServer(value);
        }

        public ServerLocation SelectedExitServer
        {
            get => __SelectedExitServer;
            set => SetSelectedExitServer(value);
        }

        private void SetSelectedServer(ServerLocation value, bool isCanReconnect = true)
        {
            if (value == null)
                return;

            if (!CheckServerLocationType(value))
                return;

            RaisePropertyWillChange(nameof(SelectedServer));
            RaisePropertyWillChange(nameof(IsFastestServerInUse));

            __SelectedServer = value;

            RaisePropertyChanged(nameof(IsFastestServerInUse));
            RaisePropertyChanged(nameof(SelectedServer));

            string lastSrvId = __AppState.Settings.GetLastUsedServerId(isExitServer: false);
            if (lastSrvId != __SelectedServer.VpnServer.GatewayId)
            {
                Settings.SetLastUsedServerId(__SelectedServer.VpnServer.GatewayId, isExitServer: false);

                // perform save in background thread to avoid GUI freeze
                Task.Run(() => Settings.Save());

                if (isCanReconnect)
                {
                    // reconnect, if necessary
                    ReconnectIfConnectedAfterServerChange();
                }
            }
        }

        private void SetSelectedExitServer(ServerLocation value)
        {
            if (value == null)
                return;

            if (!CheckServerLocationType(value))
                return;

            RaisePropertyWillChange(nameof(SelectedExitServer));
            __SelectedExitServer = value;
            RaisePropertyChanged(nameof(SelectedExitServer));

            string lastSrvId = __AppState.Settings.GetLastUsedServerId(isExitServer: true);
            if (lastSrvId != __SelectedExitServer.VpnServer.GatewayId)
            {
                Settings.SetLastUsedServerId(__SelectedExitServer.VpnServer.GatewayId, isExitServer: true);

                // perform save in background thread to avoid GUI freeze
                Task.Run(() => Settings.Save());
                
                // reconnect, if necessary
                ReconnectIfConnectedAfterServerChange();
            }
        }

        private void SetFastestServer(ServerLocation value, bool isCanReconnect = true)
        {
            if (!CheckServerLocationType(value))
                return;

            if (IsAutomaticServerSelection && value != null)
            {
                // Automatic server selection is disabled for Multi-Hop
                if (!IsMultiHop)
                    SetSelectedServer(value, isCanReconnect);
            }

            // save settings
            if (value!=null && value.VpnServer != null)            
                Settings.SetLastFastestServerId(value.VpnServer.GatewayId);

            RaisePropertyWillChange(nameof(FastestServer));
            RaisePropertyWillChange(nameof(IsFastestServerInUse));
            __FastestServer = value;
            RaisePropertyChanged(nameof(IsFastestServerInUse));
            RaisePropertyChanged(nameof(FastestServer));
        }

        private bool CheckServerLocationType(ServerLocation value)
        {
            if (value == null)
                return true;

            if (__AppState.Settings.VpnProtocolType == VpnType.OpenVPN
                && !(value.VpnServer is VpnProtocols.OpenVPN.OpenVPNVpnServer))
            {
                Logging.Info("INTERNAL ERROR: unable to set a selected server. Server VPN type differs from the current protocol");
                return false;
            }
            if (__AppState.Settings.VpnProtocolType == VpnType.WireGuard
                && !(value.VpnServer is VpnProtocols.WireGuard.WireGuardVpnServerInfo))
            {
                Logging.Info("INTERNAL ERROR: unable to set a selected server. Server VPN type differs from the current protocol");
                return false;
            }
            return true;
        }
        private void ReconnectIfConnectedAfterServerChange()
        {
            if (__ConnectionState == ServiceState.Connected
                || __ConnectionState == ServiceState.Connecting
                || __ConnectionState == ServiceState.ReconnectingOnService
                || __ConnectionState == ServiceState.ReconnectingOnClient
            )
            {
                if (PauseStatus == PauseStatusEnum.Resumed || PauseStatus == PauseStatusEnum.Resuming)
                {
                    Connect();
                }
                else
                {
                    // if server was changed during Pause - necessary to fully reconnect on Resume
                    __IsRequiredReconnectOnResume = true;
                }
            }
        }
#endregion // Selected servers

        public string ConnectionError
        {
            get => __ConnectionError;
            private set
            {
                if (string.Equals (__ConnectionError, value))
                    return;
                RaisePropertyWillChange(() => ConnectionError);
                __ConnectionError = value;
                RaisePropertyChanged(() => ConnectionError);
            }
        }

        public ICommand SelectServerCommand { get; }

        public ICommand SelectEntryServerCommand { get; }

        public ICommand SelectExitServerCommand { get; }

        public string ExternalIPAddress
        {
            get => __ExternalIPAddress;

            private set
            {
                RaisePropertyWillChange(() => ExternalIPAddress);
                __ExternalIPAddress = value;
                RaisePropertyChanged(() => ExternalIPAddress);
            }
        }

        public string InternalIPAddress
        {
            get => __InternalIPAddress;
            private set
            {
                RaisePropertyWillChange(() => InternalIPAddress);
                __InternalIPAddress = value;
                RaisePropertyChanged(() => InternalIPAddress);
            }
        }

        public string ConnectionProgressString
        {
            get => __ConnectionProgressString;
            private set
            {
                RaisePropertyWillChange(() => ConnectionProgressString);
                __ConnectionProgressString = value;
                RaisePropertyChanged(() => ConnectionProgressString);
            }
        }

        public bool IsConnectionCancelAvailable
        {
            get => __IsConnectionCancelAvailable;
            set
            {
                RaisePropertyWillChange(() => IsConnectionCancelAvailable);
                __IsConnectionCancelAvailable = value;
                RaisePropertyChanged(() => IsConnectionCancelAvailable);
            }
        }

        public ConnectionInfo ConnectionInfo
        {
            get => __ConnectionInfo;
            private set
            {
                RaisePropertyWillChange();

                if (__ConnectionInfo!=null)
                    __ConnectionInfo.PropertyChanged -= ConnectionInfo_PropertyChanged;

                __ConnectionInfo = value;

                if (__ConnectionInfo!=null)
                    __ConnectionInfo.PropertyChanged += ConnectionInfo_PropertyChanged;

                RaisePropertyChanged();
            }
        }

        private bool __IsKillSwitchEnabled;

        public bool IsKillSwitchEnabled
        {
            get => __IsKillSwitchEnabled;

            set
            {
                if (__IsKillSwitchEnabled == value)
                    return;

                RaisePropertyWillChange();

                if (value == false && Settings.FirewallType == IVPNFirewallType.Persistent)
                    throw new InvalidOperationException("Firewall cannot be switched off when in persistent state");

                __Service.KillSwitchIsEnabled = value;
                __IsKillSwitchEnabled = value;

                if (ConnectionState != ServiceState.Connected)
                    __FirewallAutoEnabled = false;

                RaisePropertyChanged();
            }
        }
        
        public void DisableFirewallOnExitIfRequired()
        {
            if (Settings.FirewallType == IVPNFirewallType.Persistent)
                return;

            if (!Settings.FirewallDisableDeactivationOnExit)
            {
                bool killSwitchStatusToSave = IsKillSwitchEnabled;

                if (PauseStatus == PauseStatusEnum.Paused)
                {
                    killSwitchStatusToSave = __KillSwitchStateForResume;
                    PauseStatus = PauseStatusEnum.Resumed;
                }

                if (__FirewallAutoEnabled)
                    killSwitchStatusToSave = false;

                if (Settings.FirewallLastStatus != killSwitchStatusToSave)
                {
                    Settings.FirewallLastStatus = killSwitchStatusToSave;
                    Settings.Save();
                }

                IsKillSwitchEnabled = false;
            }
        }

        public void ForceDisconnectAndDisableFirewall()
        {
            // Disconnect VPN (if connected) and disable Firewall
            try 
            {
                if (KillSwitchIsPersistent != false
                    || Settings.FirewallType != IVPNFirewallType.Manual
                    || Settings.FirewallLastStatus != false
                    || IsKillSwitchEnabled != false)
                {
                    KillSwitchIsPersistent = false;
                    Settings.FirewallType = IVPNFirewallType.Manual;
                    Settings.FirewallLastStatus = false;

                    IsKillSwitchEnabled = false;

                    Settings.Save();
                }

                if (__Service.State != ServiceState.Disconnected)
                    Disconnect ();
            } 
            catch (Exception ex) 
            { 
                Logging.Info ($"{ex}"); 
            }
        }

        public AppSettings Settings => __AppState.Settings;

        public bool KillSwitchAllowLAN
        {
            set => __Service.KillSwitchAllowLAN = value;
        }

        public bool KillSwitchAllowLANMulticast
        {
            set => __Service.KillSwitchAllowLANMulticast = value;
        }

        public bool KillSwitchIsPersistent
        {
            get => __KillSwitchIsPersistent;

            set
            {
                RaisePropertyWillChange();

                __Service.KillSwitchIsPersistent = value;
                __KillSwitchIsPersistent = value;

                RaisePropertyChanged();

                if (value)
                    IsKillSwitchEnabled = true;
            }
        }

        public bool IsMultiHop
        {
            get => __IsMultiHop;

            set
            {
                RaisePropertyWillChange();

                __IsMultiHop = value;

                // for MultiHop - ensure that Entry- and Exit- servers are selected correctly
                if (__IsMultiHop && SelectedServer != null && SelectedExitServer != null)
                {
                    if (SelectedServer.CountryCode == SelectedExitServer.CountryCode)
                    {
                        var newServer = __Service.Servers.ServersList.FirstOrDefault(s => s.CountryCode != SelectedServer.CountryCode);
                        if (newServer != null)
                            SelectedExitServer = newServer;
                    }
                }

                // when switching to SingleHop - select fastest server (if necessary)
                if (__IsMultiHop == false && IsAutomaticServerSelection)
                {
                    if (FastestServer != null)
                        SelectedServer = FastestServer;
                    else
                        __Service.Servers.StartPingUpdate();
                }

                if (Settings.IsMultiHop != __IsMultiHop)
                {
                    Settings.IsMultiHop = __IsMultiHop;
                    Settings.Save();
                }

                RaisePropertyChanged();
            }
        }

        public bool IsAllowedMultiHop
        {
            get => __IsAllowedMultiHop;
            private set
            {
                RaisePropertyWillChange();
                __IsAllowedMultiHop = value;
                if (!__IsAllowedMultiHop && IsMultiHop)
                    IsMultiHop = false;
                RaisePropertyChanged();
            }
        }

        public bool IsAllowedPrivateEmails
        {
            get => __IsAllowedPrivateEmails;
            private set
            {
                if (__IsAllowedPrivateEmails == value)
                    return;

                RaisePropertyWillChange();
                __IsAllowedPrivateEmails = value;
                RaisePropertyChanged();
            }
        }

        // In use by macOS project
        public IAppNavigationService NavigationService => __NavigationService;

        public IApplicationServices AppServices => __AppServices;
        
        public AppState AppState => __AppState;

#region 'Pause' functionality

        public enum PauseStatusEnum
        {
            Pausing,
            Resuming,
            Paused,
            Resumed
        };

        private PauseStatusEnum __PauseStatus = PauseStatusEnum.Resumed;

        public PauseStatusEnum PauseStatus
        {
            get => __PauseStatus;
            set
            {
                RaisePropertyWillChange();
                __PauseStatus = value;
                RaisePropertyChanged();
            }
        }

        private System.Timers.Timer __PauseTimer;
        private DateTime __PauseTill = default(DateTime);
        private bool __KillSwitchStateForResume;

        // if server was changed during Pause - necessary to fully reconnect on Resume
        private bool __IsRequiredReconnectOnResume; 

        private async void PauseStart(double seconds)
        {
            try
            {
                if (PauseStatus != PauseStatusEnum.Resumed || ConnectionState != ServiceState.Connected)
                    return;

                PauseStatus = PauseStatusEnum.Pausing;
                await __Service.PauseOn();
                PauseStatus = PauseStatusEnum.Paused;
            }
            catch (Exception ex)
            {
                PauseStatus = PauseStatusEnum.Resumed;
                Logging.Info($"{ex}");
                NotifyError("Failed to pause the connection", ex.Message);
            }

            if (PauseStatus == PauseStatusEnum.Paused)
                ConnectionInfo.DurationStop();

            // save kill-switch state (will be used for resume)
            __KillSwitchStateForResume = IsKillSwitchEnabled;

            // disable kill-switch (if not in firewall-persistant mode)
            if (Settings.FirewallType != IVPNFirewallType.Persistent)
                IsKillSwitchEnabled = false;

            // stop timer if it already started
            System.Timers.Timer prevTimer = __PauseTimer;
            __PauseTimer = null;
            if (prevTimer != null)
            {
                prevTimer.Stop();
                prevTimer.Dispose();
            }

            __PauseTill = DateTime.Now.AddSeconds(seconds);
            __PauseTimer = new System.Timers.Timer(1000);
            __PauseTimer.Elapsed += CheckIfResumeRequired;
            __PauseTimer.Start();
            CalculateTimeToResumeLeft();
        }

        private async void Resume()
        {
            if (PauseStatus == PauseStatusEnum.Paused)
            {
                PauseStatus = PauseStatusEnum.Resuming;
                await __Service.PauseOff();
                PauseStatus = PauseStatusEnum.Resumed;

                ConnectionInfo.DurationStart();

                ResumeKillSwitch();

                // if server was changed during Pause - necessary to fully reconnect
                if (__IsRequiredReconnectOnResume)
                    Connect();
            }
        }

        private void ResumeKillSwitch()
        {
            if (__KillSwitchStateForResume == false && Settings.FirewallType == IVPNFirewallType.Persistent)
                return;


            bool firewallAutoEnabled = __FirewallAutoEnabled;

            // restore kill-switch state
            IsKillSwitchEnabled = __KillSwitchStateForResume;

            // keep '__FirewallAutoEnabled' without changes
            __FirewallAutoEnabled = firewallAutoEnabled;
        }

        private void CheckIfResumeRequired(object sender, ElapsedEventArgs e)
        {
            CalculateTimeToResumeLeft();

            if (PauseStatus != PauseStatusEnum.Paused)
                return;
            
            if (DateTime.Now >= __PauseTill)
                Resume();
        }

        private void CalculateTimeToResumeLeft()
        {
            DateTime now = DateTime.Now;
            if (PauseStatus != PauseStatusEnum.Paused || __PauseTill == default(DateTime) || now > __PauseTill)
                TimeToResumeLeft = "0:00:00";
            else
            {
                TimeSpan timeLeft = __PauseTill - now;
                TimeToResumeLeft = $"{timeLeft.Days * 24 + timeLeft.Hours}:{(timeLeft):mm\\:ss}";
            }
        }

        private string __TimeToResumeLeft;
        public string TimeToResumeLeft
        {
            get => __TimeToResumeLeft;
            set
            {
                RaisePropertyWillChange();
                __TimeToResumeLeft = value;
                RaisePropertyChanged();
            }
        }

        public void SetPauseTime(double seconds)
        {
            __PauseTill = DateTime.Now.AddSeconds(seconds);
            CalculateTimeToResumeLeft();
        }

        public void AddPauseTime(double seconds)
        {
            __PauseTill = __PauseTill.AddSeconds(seconds);
            CalculateTimeToResumeLeft();
        }

#endregion //'Pause' functionality

#region Networks management

        private WifiState __WiFiState;
        public WifiState WiFiState
        {
            get => __WiFiState;
            set
            {
                RaisePropertyWillChange();
                RaisePropertyWillChange(nameof(WiFiActionType));
                __WiFiState = value;
                RaisePropertyChanged(nameof(WiFiActionType));
                RaisePropertyChanged();
            }
        }

        public NetworkActionsConfig.WiFiActionTypeEnum WiFiActionType
        {
            get
            {
                if (WiFiState == null)
                    return NetworkActionsConfig.WiFiActionTypeEnum.None;

                return Settings.NetworkActions.GetActionForNetwork(WiFiState.Network);
            }
            set
            {
                RaisePropertyWillChange();
                SetActionForCurrentWiFi(value);
                RaisePropertyChanged();
            }
        }

        private void WiFiWrapperOnWiFiStateChanged(WifiState state)
        {
            WiFiState = state;
            ApplyActionForCurrentNetwork();
        }

        private WiFiNetwork __PreviouslyProcessedNetwork;
        private NetworkActionsConfig.WiFiActionTypeEnum __PreviouslyProcessedNetworkAction;

        /// <summary>
        /// Wifi state changed.
        /// Perform rules for connected network
        /// </summary>
        private void ApplyActionForCurrentNetwork()
        {
            if (ConnectionState == ServiceState.Uninitialized)
                return;
            
            // emulating user iteraction from GUI (Connect button press... etc.)
            __SyncInvoke.BeginInvoke( new Action (async () =>
            {
                // notify to update UI
                RaisePropertyChanged(nameof(WiFiActionType));

                WifiState state = WiFiState;
                if (state == null || state.Network == null || string.IsNullOrEmpty(state.Network.SSID))
                {
                    __PreviouslyProcessedNetwork = state?.Network;
                    return;
                }

                // get action for new WiFi from settings
                NetworkActionsConfig.WiFiActionTypeEnum actionType = Settings.NetworkActions.GetActionForNetwork(state.Network);

                // processing insecure network (if necessary)
                if (state.ConnectedToInsecureNetwork
                    && Settings.ServiceConnectOnInsecureWifi
                    && __ConnectionState == ServiceState.Disconnected
                    && (
                        Settings.IsNetworkActionsEnabled == false
                        ||
                            (
                            // if IsNetworkActionsEnabled == true we are processing insecure network configuration only when: Default-noAction
                            actionType == NetworkActionsConfig.WiFiActionTypeEnum.Default 
                            && Settings.NetworkActions.DefaultActionType == NetworkActionsConfig.WiFiActionTypeEnum.None
                            )
                        )
                    )
                {
                    // If we already processed this network with such action - do nothing
                    if (__PreviouslyProcessedNetwork != null
                        && __PreviouslyProcessedNetwork.Equals(state.Network)
                        && __PreviouslyProcessedNetworkAction == NetworkActionsConfig.WiFiActionTypeEnum.None)
                        return;
                    __PreviouslyProcessedNetwork = state.Network;
                    __PreviouslyProcessedNetworkAction = NetworkActionsConfig.WiFiActionTypeEnum.None;

                    // When connected to insecure network
                    __Notifications.TrayIconNotifyInsecureWiFi($"{state.Network.SSID} is an insecure WiFi network. To protect your privacy, a VPN connection is automatically being established.");
                    await ConnectToLastServer();
                    return;
                }

                //When PAUSED (and, of course, we already connected in PAUSED stage):
                //-> On connected to Untrusted network:
                //      ---> if 'Connect to VPN' ENABLED  -> RESUME
                //      ---> if 'Connect to VPN' DISABLED -> stay PAUSED
                //-> On connected to Trusted network :
                //      ---> if 'Disconnect VPN' ENABLED  -> RESUME
                //      ---> if 'Disconnect VPN' DISABLED -> stay PAUSED

                if (Settings.IsNetworkActionsEnabled)
                {
                    // get action for new WiFi from default settings (if current network configuration is set to Default)
                    if (actionType == NetworkActionsConfig.WiFiActionTypeEnum.Default)
                        actionType = Settings.NetworkActions.DefaultActionType;

                    // If we already processed this network with such action - do nothing
                    if (__PreviouslyProcessedNetwork != null
                        && __PreviouslyProcessedNetwork.Equals(state.Network)
                        && __PreviouslyProcessedNetworkAction == actionType)
                        return;

                    __PreviouslyProcessedNetwork = state.Network;
                    __PreviouslyProcessedNetworkAction = actionType;

                    if (actionType == NetworkActionsConfig.WiFiActionTypeEnum.Untrusted)
                    {
                        // If it Untusted network
                        if (Settings.NetworkActions.UnTrustedConnectToVPN)
                        {
                            // PAUSE - OFF
                            Resume();

                            // if need to connect VPN
                            // Check if we should enable Firewall before connecting
                            if (Settings.NetworkActions.UnTrustedEnableKillSwitch)
                                IsKillSwitchEnabled = true;

                            await ConnectToLastServer();
                        }
                        else if (Settings.NetworkActions.UnTrustedEnableKillSwitch)
                        {
                            if (PauseStatus == PauseStatusEnum.Paused)
                                __KillSwitchStateForResume = true;
                            else
                                IsKillSwitchEnabled = true;
                        }
                    }
                    else if (actionType == NetworkActionsConfig.WiFiActionTypeEnum.Trusted)
                    {
                        // If it Trusted network
                        if (Settings.NetworkActions.TrustedDisconnectFromVPN)
                        {
                            // Disconnect
                            Disconnect();
                        }

                        if (Settings.NetworkActions.TrustedDisableKillSwitch
                            && Settings.FirewallType != IVPNFirewallType.Persistent)
                        {
                            if (PauseStatus == PauseStatusEnum.Paused)
                                __KillSwitchStateForResume = false;
                            else
                                IsKillSwitchEnabled = false;
                        }
                    }
                }
            }), null);
        }

        public void SetActionForCurrentWiFi(NetworkActionsConfig.WiFiActionTypeEnum action)
        {
            if (WiFiState==null)
                return;

            Settings.NetworkActions.SetActionForNetwork(WiFiState.Network, action);

            // perform save in background thread to avoid GUI freeze
            Task.Run(() => Settings.Save());

            ApplyActionForCurrentNetwork();
        }
#endregion

#region DNS

        private bool __IsIsAntiTrackerChangingStatus;
        public bool IsIsAntiTrackerChangingStatus
        {
            get => __IsIsAntiTrackerChangingStatus;
            set
            {
                RaisePropertyWillChange();
                __IsIsAntiTrackerChangingStatus = value;
                RaisePropertyChanged();
            }
        }

        public bool IsAntiTrackerEnabled
        {
            get => Settings.IsAntiTracker;

            set
            {
                if (Settings.IsAntiTracker == value || IsIsAntiTrackerChangingStatus)
                    return;

                RaisePropertyWillChange();
                IsIsAntiTrackerChangingStatus = true;
                Settings.IsAntiTracker = value;
                RaisePropertyChanged();

                Task.Run(async () =>
                {
                    try
                    {
                        await SetCustomDnsIfNecessary();
                        // Save only after 'SetCustomDnsIfNecessary()' because this method can change 'IsAntiTracker' property
                        Settings.Save();
                    }
                    finally
                    {
                        IsIsAntiTrackerChangingStatus = false;
                    }
                });
            }
        }

        private IPAddress GetCustomDns()
        {
            IPAddress dns = IPAddress.None;

            if (Settings.IsAntiTracker)
            { 
                if (Settings.IsAntiTrackerHardcore)
                    dns = __Service.Servers.GetDnsIp(DnsTypeEnum.AntiTrackerHardcore, IsMultiHop);
                else
                    dns = __Service.Servers.GetDnsIp(DnsTypeEnum.AntiTracker, IsMultiHop);

                if (dns == null)
                    throw new IVPNExceptionUserErrorMsg(
                        __AppServices.LocalizedString("Error_FailedToChangeDNS", "Failed to change DNS"),
                        __AppServices.LocalizedString("Error_InvalidAntiTrackerDNS", "Unable to obtain AntiTracker DNS"));
            }
            else if (Settings.IsCustomDns && string.IsNullOrEmpty(Settings.CustomDns) == false)
            {
                if (IPAddress.TryParse(Settings.CustomDns, out dns) == false)
                {
                    throw new IVPNExceptionUserErrorMsg(
                        __AppServices.LocalizedString("Error_InvalidCustomDNS", "Invalid Custom DNS IP Address"),
                        string.Format(__AppServices.LocalizedString("Error_InvalidCustomDNSDetailed_PARAMETRIZED", "The IP Address ({0}) is invalid. Please, check and update settings."), Settings.CustomDns));
                }
            }

            return dns;
        }

        private async Task<bool> SetCustomDnsIfNecessary()
        {
            // It has no sens to change DNS in disconnected state. 
            // Anyway, it will be changed on connection
            if (ConnectionState == ServiceState.Disconnected
                || ConnectionState == ServiceState.Disconnecting
                || ConnectionState == ServiceState.CancellingConnection
                || ConnectionState == ServiceState.Uninitialized)
                return true;

            void FailedChangeDnsNotify(string caption, string message) 
            {
                RaisePropertyWillChange(nameof(IsAntiTrackerEnabled));
                Settings.IsAntiTracker = false;
                RaisePropertyChanged(nameof(IsAntiTrackerEnabled));

                NotifyError(caption, message);
            };

            IPAddress dns = IPAddress.None;
            bool isDnsParseOk = false;
            try
            {
                dns = GetCustomDns();
                isDnsParseOk = true;
            }
            catch (IVPNExceptionUserErrorMsg ex)
            {
                FailedChangeDnsNotify(ex.Caption, ex.Message);
            }
            catch 
            {
                FailedChangeDnsNotify(  __AppServices.LocalizedString("Error_FailedToChangeDNS", "Failed to change DNS"),
                                        __AppServices.LocalizedString("Error_FailedToChangeDNSDetailed", "DNS was not changed."));
            }
            
            bool isSuccess = await __Service.SetDns(dns);

            if (!isSuccess)
            {
                if (isDnsParseOk) // do not show notification if DNS was not parsed (we already showed an error)
                {
                    FailedChangeDnsNotify(
                        __AppServices.LocalizedString("Error_FailedToChangeDNS", "Failed to change DNS"),
                        __AppServices.LocalizedString("Error_FailedToChangeDNSDetailed", "DNS was not changed."));
                }
            }

            return isDnsParseOk && isSuccess;
        }
#endregion //DNS
    }
}
