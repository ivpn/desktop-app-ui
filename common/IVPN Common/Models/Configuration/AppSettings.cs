using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using IVPN.Exceptions;
using IVPN.Interfaces;
using IVPN.Lib;
using IVPN.ViewModels;
using IVPN.VpnProtocols;

namespace IVPN.Models.Configuration
{
    public class AppSettings : ViewModelBase
    {
        private readonly ISettingsProvider __SettingsProvider;
        
        #region Events
        /// <summary> Occurs when on settings saved. </summary>
        public event EventHandler OnSettingsSaved = delegate { };

        #endregion //Events

        public static AppSettings Instance()
        {
            if (__SingletonInstance == null)
                throw new Exception("Settings instance not initialized");

            return __SingletonInstance;
        }

        private static AppSettings __SingletonInstance;
        public static AppSettings InitInstance(ISettingsProvider settingsProvider)
        {
            if (__SingletonInstance != null)
                return __SingletonInstance;

            if (settingsProvider == null)
                throw new IVPNInternalException("Setings object not initialized. To initialize, a 'provider' should be used");

            __SingletonInstance = new AppSettings(settingsProvider);

            return __SingletonInstance;
        }

        private AppSettings(ISettingsProvider settingsProvider)
        {
            __SettingsProvider = settingsProvider;

            PreferredPortsList = new List<ServerPort>
            {
                new ServerPort(new DestinationPort(2049, DestinationPort.ProtocolEnum.UDP)),
                new ServerPort(new DestinationPort(2050, DestinationPort.ProtocolEnum.UDP)),
                new ServerPort(new DestinationPort(53, DestinationPort.ProtocolEnum.UDP)),
                new ServerPort(new DestinationPort(1194, DestinationPort.ProtocolEnum.UDP)),
                new ServerPort(new DestinationPort(443, DestinationPort.ProtocolEnum.TCP)),
                new ServerPort(new DestinationPort(1443, DestinationPort.ProtocolEnum.TCP)),
                new ServerPort(new DestinationPort(80, DestinationPort.ProtocolEnum.TCP))
            };

            WireGuardPreferredPortsList = new List<ServerPort>
            {
                // 53, 2049, 2050, 30587,41893,58237, 48574
                new ServerPort(new DestinationPort(2049, DestinationPort.ProtocolEnum.UDP)),
                new ServerPort(new DestinationPort(2050, DestinationPort.ProtocolEnum.UDP)),
                new ServerPort(new DestinationPort(53, DestinationPort.ProtocolEnum.UDP)),
                new ServerPort(new DestinationPort(1194, DestinationPort.ProtocolEnum.UDP)),
                new ServerPort(new DestinationPort(30587, DestinationPort.ProtocolEnum.UDP)),
                new ServerPort(new DestinationPort(41893, DestinationPort.ProtocolEnum.UDP)),
                new ServerPort(new DestinationPort(48574, DestinationPort.ProtocolEnum.UDP)),
                new ServerPort(new DestinationPort(58237, DestinationPort.ProtocolEnum.UDP))
            };

            __SettingsProvider.Load(this);
        }

        public void Save()
        {
            __SettingsProvider.Save(this);

            // notify event "settings saved"
            OnSettingsSaved(this, null);
        }

        /// <summary>
        /// Reset alls settings instead of VpnProtocolType and Username
        /// </summary>
        public void Reset()
        {
            UnfreezeSettings(isRestoreFreezedState: false);

            VpnType vpnType = VpnProtocolType;
            
            bool isFirstIntroductionDone = IsFirstIntroductionDone;
            bool macIsShowIconInSystemDock = MacIsShowIconInSystemDock;
            bool isLoggingOn = IsLoggingEnabled;

            // Reset values to defaults
            __SettingsProvider.Reset(this);
            
            // Some values should not be changed after reset:
            VpnProtocolType = vpnType;
            IsFirstIntroductionDone = isFirstIntroductionDone;
            MacIsShowIconInSystemDock = macIsShowIconInSystemDock;
            IsLoggingEnabled = isLoggingOn;

            Save();
        }

        public bool GetOldStyleCredentials(
            out string AccountID,
            out string Session,
            out string OvpnUser,
            out string OvpnPass,
            out string WgPublicKey,
            out string WgPrivateKey,
            out string WgLocalIP,
            out Int64 WgKeyGenerated)
        {
            return __SettingsProvider.GetOldStyleCredentials(out AccountID,
                out Session,
                out OvpnUser,
                out OvpnPass,
                out WgPublicKey,
                out WgPrivateKey,
                out WgLocalIP,
                out WgKeyGenerated);
        }

        #region Temporary settings
        private AppSettings __FreezedSettings;

        public void FreezeSettings()
        {
            Save();
            __FreezedSettings = new AppSettings(__SettingsProvider);
        }

        public void UnfreezeSettings(bool isRestoreFreezedState)
        {
            var settings = __FreezedSettings;
            __FreezedSettings = null;

            if (isRestoreFreezedState == false || settings == null)
                return;
                       
            __SettingsProvider.Save(settings);
            __SettingsProvider.Load(this);
        }
        #endregion //Temporary settings

        public string AlternateAPIHost { get; set; }

        /// <summary>
        /// Indicating whether the first run introduction finished.
        /// On first application start (after first install) this value should be false.
        /// </summary>
        public bool IsFirstIntroductionDone { get; set; }

        private bool __RunOnLogin;
        public bool RunOnLogin 
        { 
            get => __RunOnLogin; 
            set
            {
                if (__RunOnLogin == value)
                    return;

                RaisePropertyWillChange();
                __RunOnLogin = value;
                try
                {
                    __SettingsProvider.OnSettingsChanged(this);
                }
                catch
                {
                    // failed to save. Revert previous value
                    __RunOnLogin = !__RunOnLogin;
                    throw;
                }
                RaisePropertyChanged();
            } 
        }

        public bool MinimizeToTray { get; set; }

        public bool AutoConnectOnStart { get; set; }

        public bool LaunchMinimized { get; set; }

        /// <summary>
        /// Do not show question-dialog on application close in 'connected' state
        /// </summary>
        public bool DoNotShowDialogOnAppClose { get; set; }

        /// <summary>
        /// TRUE - server (Windows service or macOS Agent) will stop on client disconnect (when application cloased)
        /// </summary>
        public bool StopServerOnClientDisconnect { get; set; }

        public bool ServiceConnectOnInsecureWifi { get; set; }

        #region Fastest server
        /// <summary> OpenVPN fastest server. </summary>
        public string LastOvpnFastestServerId { get; set; }

        /// <summary> WireGuard fastest server </summary>
        public string LastWgFastestServerId { get; set; }

        public string GetLastFastestServerId()
        {
            return (VpnProtocolType == VpnType.WireGuard) ? LastWgFastestServerId : LastOvpnFastestServerId;
        }

        public void SetLastFastestServerId(string serverId)
        {
            if (VpnProtocolType == VpnType.WireGuard)
                LastWgFastestServerId = serverId;
            else
                LastOvpnFastestServerId = serverId;
        }
        #endregion // Fastest server

        #region Last used servers
        public bool GetIsAutomaticServerSelection()
        {
            return (VpnProtocolType == VpnType.WireGuard) ? IsAutomaticServerSelectionWg : IsAutomaticServerSelection;
        }

        public void SetIsAutomaticServerSelection(bool isAutomaticServerSelection)
        {
            if (VpnProtocolType == VpnType.WireGuard)
                IsAutomaticServerSelectionWg = isAutomaticServerSelection;
            else 
                IsAutomaticServerSelection = isAutomaticServerSelection;
        }

        public string GetLastUsedServerId(bool isExitServer)
        {
            if (isExitServer)
                return (VpnProtocolType == VpnType.WireGuard) ? LastUsedWgExitServerId : LastUsedExitServerId; 
            return (VpnProtocolType == VpnType.WireGuard) ? LastUsedWgServerId : LastUsedServerId;
        }

        public void SetLastUsedServerId(string serverId, bool isExitServer)
        {
            if (isExitServer)
            {
                if (VpnProtocolType == VpnType.WireGuard)
                    LastUsedWgExitServerId = serverId;
                else
                    LastUsedExitServerId = serverId;
            }
            else
            {
                if (VpnProtocolType == VpnType.WireGuard)
                    LastUsedWgServerId = serverId;
                else
                    LastUsedServerId = serverId;
            }
        }

        /// <summary> Is FastestServer selection enabled (for OpenVPN) </summary>
        public bool IsAutomaticServerSelection { get; set; }
        /// <summary> Last selected server (OpenVPN) </summary>
        public string LastUsedServerId { get; set; }
        /// <summary> Last selected exit-server (OpenVPN) </summary>
        public string LastUsedExitServerId { get; set; }

        /// <summary> Is FastestServer selection enabled (for WireGuard) </summary>
        public bool IsAutomaticServerSelectionWg { get; set; }
        /// <summary> Last selected server (WireGuard) </summary>
        public string LastUsedWgServerId { get; set; }
        /// <summary> Last selected exit-server (WireGuard) </summary>
        public string LastUsedWgExitServerId { get; set; }

        #endregion //Last user servers

        public bool IsMultiHop { get; set; }

        public bool FirewallDisableAutoOnOff { get; set; }

        public bool FirewallDisableDeactivationOnExit { get; set; }

        /// <summary>
        /// Activate Firewall on connect to VPN 
        /// and  deactivate on disconnect
        /// </summary>
        public bool FirewallAutoOnOff
        {
            get => !FirewallDisableAutoOnOff;
            set => FirewallDisableAutoOnOff = !value;
        }

        /// <summary>
        /// Deactivate IVPN firewall on IVPN client exit
        /// </summary>
        public bool FirewallDeactivationOnExit
        {
            get => !FirewallDisableDeactivationOnExit;
            set => FirewallDisableDeactivationOnExit = !value;
        }

        public bool FirewallLastStatus { get; set; }

        private bool __FirewallAllowLAN;
        public bool FirewallAllowLAN
        {
            get => __FirewallAllowLAN;
            set
            {
                RaisePropertyWillChange();
                __FirewallAllowLAN = value;
                RaisePropertyChanged();
            }
        }

        public bool FirewallAllowLANMulticast { get; set; }

        public bool IsIVPNFirewalIntoduced { get; set; }

        public IVPNFirewallType FirewallType { get; set; }

        public bool IsIVPNAlwaysOnWarningDisplayed { get; set; }

        public string LastWindowPosition { get; set; }

        public string LastNotificationWindowPosition { get; set; }

        public bool IsLoggingEnabled { get; set; }

        public bool MacIsLoginItemRemoved { get; set; }

        // platform: macOS-only
        public bool MacIsShowIconInSystemDock { get; set; }

        /// <summary>
        /// true - firewall notification window will not be shown
        /// (Notification window - “Firewall is enabled” visible on display (topmost) in case when Firewall is enabled and VPN connection is OFF)
        /// </summary>
        public bool DisableFirewallNotificationWindow { get; set; }

        #region ServersFilter
        public ServersFilterConfig ServersFilter
        {
            set
            {
                RaisePropertyWillChange();
                __ServersFilter = value;
                RaisePropertyChanged();
            }
            get => __ServersFilter ?? (__ServersFilter = new ServersFilterConfig());
        }
        private ServersFilterConfig __ServersFilter;
        #endregion // ServersFilter

        #region Trusted\untrusted WiFi actions
        public bool IsNetworkActionsEnabled { get; set; }
        [IgnoreDataMember]
        public NetworkActionsConfig NetworkActions
        {
            set => __NetworkActions = value;
            get => __NetworkActions ?? (__NetworkActions = new NetworkActionsConfig());
        }
        private NetworkActionsConfig __NetworkActions;
        #endregion Trusted\untrusted WiFi actons

        #region VPN protocols
        private VpnType __VpnType;
        public VpnType VpnProtocolType
        {
            get => __VpnType;
            set
            {
                if (__VpnType == value)
                    return;

                RaisePropertyWillChange();
                __VpnType = value;
                RaisePropertyChanged();
            }
        }

        private DestinationPort GetPort(List<ServerPort> ports, int idx)
        {
            if (idx >= ports.Count)
            {
                if (ports.Count > 0)
                    return ports[0].DestinationPort;

                return null;
            }
            return ports[idx].DestinationPort;
        }

        private void SetPort(DestinationPort value, List<ServerPort> ports, out int idx)
        {
            for (int i = 0; i < ports.Count; i++)
            {
                ServerPort port = ports[i];
                if (port.DestinationPort.Equals(value))
                {
                    idx = i;
                    return;
                }
            }
            throw new ArgumentOutOfRangeException($"Unable to set prefered port. Port is not exists in __PreferredPortsList");
        }
        #region OpenVPN configuration
        private ProxyType __ProxyType;

        public ProxyType ProxyType
        {
            get => __ProxyType;
            set
            {
                __ProxyType = value;
                UpdateEnabledServerPorts();
            }
        }

        public string ProxyServer { get; set; }

        public int ProxyPort { get; set; }
        [IgnoreDataMember]
        public string ProxyUsername { get; set; }
        [IgnoreDataMember]
        public string ProxySafePassword { get; set; }
        [IgnoreDataMember]
        public string ProxyUnsafePassword
        {
            get => CryptoUtil.DecryptString(ProxySafePassword);
            set => ProxySafePassword = CryptoUtil.EncryptString(value);
        }

        /// <summary>
        /// When TRUE - client will try to find correct port (from PreferredPortsList) when connection was failed.
        /// It will try to connect to each port one-by-one
        /// </summary>
        public bool IsAutoPortSelection { get; set; }

        [IgnoreDataMember]
        public List<ServerPort> PreferredPortsList { get; }
        [IgnoreDataMember]
        public int PreferredPortIndex { get; set; }

        public DestinationPort PreferredPort
        {
            get => GetPort(PreferredPortsList, PreferredPortIndex);
            set
            {
                SetPort(value, PreferredPortsList, out int idx);
                PreferredPortIndex = idx;
            }
        }

        public bool ServiceUseObfsProxy
        {
            get; set;
        }

        public ProxyOptions GetProxyOptions(string hostname, int port)
        {
            if (ProxyType == ProxyType.None)
                return null;

            if (ProxyType == ProxyType.Auto)
                return ResolveAutoProxyOptions(hostname, port);

            return new ProxyOptions(ProxyType.ToString().ToLower(), ProxyServer, ProxyPort, ProxyUsername, ProxyUnsafePassword);
        }

        private ProxyOptions ResolveAutoProxyOptions(string hostname, int port)
        {
            if (WebRequest.DefaultWebProxy != null)
            {
                Uri proxyUri = WebRequest.DefaultWebProxy.GetProxy(new Uri("https://" +
                    hostname + ":" + port + "/"));

                ProxyType proxyType = ProxyType.None;
                if (proxyUri != null && proxyUri.DnsSafeHost != null)
                {
                    switch (proxyUri.Scheme)
                    {
                        case "http": proxyType = ProxyType.Http; break;
                        case "socks": proxyType = ProxyType.Socks; break;
                    }

                    if (proxyType != ProxyType.None)
                    {
                        string proxyAddress = proxyUri.DnsSafeHost;
                        int proxyPort = proxyUri.Port;

                        return new ProxyOptions(proxyType.ToString(), proxyAddress, proxyPort, "", "");
                    }
                }
            }

            return null;
        }

        public void UpdateEnabledServerPorts()
        {
            foreach (var port in PreferredPortsList)
            {
                bool udpEnabled = true;

                if (__ProxyType == ProxyType.Http)
                    udpEnabled = false;

                if (port.Protocol == DestinationPort.ProtocolEnum.UDP)
                    port.IsEnabled = udpEnabled;
            }
        }

        #endregion //OpenVPN configuration

        #region WireGuard configuration
        // Ports
        [IgnoreDataMember]
        public List<ServerPort> WireGuardPreferredPortsList { get; }

        // Selected port index
        [IgnoreDataMember]
        public int WireGuardPreferredPortIndex { get; set; }

        public DestinationPort WireGuardPreferredPort
        {
            get => GetPort(WireGuardPreferredPortsList, WireGuardPreferredPortIndex);
            set
            {
                SetPort(value, WireGuardPreferredPortsList, out int idx);
                WireGuardPreferredPortIndex = idx;
            }
        }
        #endregion //WireGuard configuration
        #endregion //VPN protocols

        #region DNS
        public bool IsAntiTrackerHardcore
        {
            get => __IsAntiTrackerHardcore;
            set
            {
                if (__IsAntiTrackerHardcore == value)
                    return;

                RaisePropertyWillChange();
                __IsAntiTrackerHardcore = value;
                RaisePropertyChanged();
            }
        }
        private bool __IsAntiTrackerHardcore;

        /// <summary>
        /// True - AntiTracker functionality ON
        /// </summary>
        public bool IsAntiTracker
        {
            get => __IsAntiTracker;
            set
            {
                if (__IsAntiTracker == value)
                    return;

                RaisePropertyWillChange();
                __IsAntiTracker = value;
                RaisePropertyChanged();
            }
        }
        private bool __IsAntiTracker;

        /// <summary>
        /// True - when user custom DNS enabled
        /// </summary>
        public bool IsCustomDns
        {
            get => __IsCustomDns;
            set
            {
                if (__IsCustomDns == value)
                    return;

                RaisePropertyWillChange();
                __IsCustomDns = value;
                RaisePropertyChanged();
            }
        }
        private bool __IsCustomDns;

        /// <summary> User custom DNS </summary>
        public string CustomDns { get; set; }
        #endregion //DNS
    }
}
