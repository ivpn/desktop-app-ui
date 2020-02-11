using Microsoft.Win32;
using System;
using System.Configuration;
using System.IO;
using IVPN.Interfaces;
using IVPN.Models.Configuration;
using System.Runtime.CompilerServices;
using IVPN.VpnProtocols;

namespace IVPN.Models
{
    public class SettingsProvider : ISettingsProvider
    {
        public const string AutoStartHiddenCmdArg = "/autostart_hidden";
        public const string AutoStartCmdArg = "/autostart";

        private const string RegistrySettingsSubKey = "Software\\IVPN Limited\\";

        private const string DateTimeSerializationFormatString = "yyyy-MM-ddTHH:mm:sszzz";

        public void Load(AppSettings settings)
        {
            try
            {
                settings.IsAutomaticServerSelection = Properties.Settings.Default.IsAutomaticServerSelection;
                settings.LastUsedServerId = Properties.Settings.Default.ServerId;
                settings.LastUsedExitServerId = Properties.Settings.Default.ExitServerId;

                settings.IsAutomaticServerSelectionWg = Properties.Settings.Default.IsAutomaticServerSelectionWg;
                settings.LastUsedWgServerId = Properties.Settings.Default.LastUsedWgServerId;
                settings.LastUsedWgExitServerId = Properties.Settings.Default.LastUsedWgExitServerId;

                // Fastest server
                settings.LastOvpnFastestServerId = Properties.Settings.Default.LastOvpnFastestServerId;
                settings.LastWgFastestServerId = Properties.Settings.Default.LastWgFastestServerId;

                settings.AlternateAPIHost = Properties.Settings.Default.AlternateAPIHost;

                string username = Properties.Settings.Default.Username;

                if (string.IsNullOrEmpty(username))
                {
                    settings.DeleteSession();
                    settings.SetWireGuardCredentials(null, null, true, null);
                }
                else
                {
                    // SESSION
                    settings.SetSession(username,
                        Properties.Settings.Default.SessionToken,
                        Properties.Settings.Default.VpnUser,
                        Properties.Settings.Default.VpnSafePass,
                        isPassEncrypded: true);
                }

                // Connection settings
                settings.IsAutoPortSelection = Properties.Settings.Default.IsAutoPortSelection;
                settings.PreferredPortIndex = Properties.Settings.Default.PreferredPortIndex;
                LoadProxySettings(settings);

                // General settings
                settings.RunOnLogin = GetRunOnLoginFromRegistry();
                settings.MinimizeToTray = Properties.Settings.Default.MinimizeToTray;
                settings.LaunchMinimized = Properties.Settings.Default.LaunchMinimized;
                settings.AutoConnectOnStart = Properties.Settings.Default.AutoConnectOnLaunch;
                settings.StopServerOnClientDisconnect = Properties.Settings.Default.StopServerOnClientDisconnect;
                settings.DoNotShowDialogOnAppClose = Properties.Settings.Default.DoNotShowDialogOnAppClose;

                // Advanced
                settings.ServiceUseObfsProxy = Properties.Settings.Default.ServiceUseObfsProxy;
                settings.ServiceConnectOnInsecureWifi = Properties.Settings.Default.ServiceConnectOnInsecureWifi;

                settings.FirewallDisableAutoOnOff = Properties.Settings.Default.FirewallDisableAutoOnOff;
                settings.FirewallDisableDeactivationOnExit = Properties.Settings.Default.FirewallDisableDeactivationOnExit;
                settings.FirewallLastStatus = Properties.Settings.Default.FirewallLastStatus;
                settings.FirewallAllowLAN = Properties.Settings.Default.FirewallAllowLAN;
                settings.FirewallAllowLANMulticast = Properties.Settings.Default.FirewallAllowMulticast;
                settings.IsIVPNFirewalIntoduced = Properties.Settings.Default.IsIVPNFirewalIntoduced;
                settings.IsIVPNAlwaysOnWarningDisplayed = Properties.Settings.Default.IsIVPNAlwaysOnWarningDisplayed;
                settings.LastWindowPosition = Properties.Settings.Default.LastWindowPosition;
                settings.LastNotificationWindowPosition = Properties.Settings.Default.LastNotificationWindowPosition;
                settings.IsMultiHop = Properties.Settings.Default.IsMultiHop;
                settings.FirewallType = LoadFirewallType();

                // Diagnostics
                settings.IsLoggingEnabled = Properties.Settings.Default.IsLoggingEnabled;

                // Additional OpenVPN parameters defined by user
                settings.OpenVPNExtraParameters = Properties.Settings.Default.OpenVPNExtraParameters;

                settings.DisableFirewallNotificationWindow = Properties.Settings.Default.DisableFirewallNotificationWindow;

                // Actions for Trusted\Untrusted networks
                settings.IsNetworkActionsEnabled = Properties.Settings.Default.IsNetworkActionsEnabled;
                settings.NetworkActions = NetworkActionsConfig.Deserialize(Properties.Settings.Default.NetworkActions);

                //Servers filter
                settings.ServersFilter = ServersFilterConfig.Deserialize(Properties.Settings.Default.ServersFilter);

                //DNS filter
                settings.IsAntiTrackerHardcore = Properties.Settings.Default.IsAntiTrackerHardcore;
                settings.IsAntiTracker = Properties.Settings.Default.IsAntiTracker;
                settings.IsCustomDns = Properties.Settings.Default.IsCustomDns;
                settings.CustomDns = Properties.Settings.Default.CustomDns;

                // VPN type
                settings.VpnProtocolType = (VpnType) Properties.Settings.Default.VpnProtocolType;

                // WireGuard
                DateTime wgKeyTimestamp;
                try
                {
                    wgKeyTimestamp = DateTime.ParseExact(Properties.Settings.Default.WireGuardKeysTimestamp,
                        DateTimeSerializationFormatString, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    wgKeyTimestamp = default(DateTime);
                }
                settings.SetWireGuardCredentials(Properties.Settings.Default.WireGuardClientPrivateKey, Properties.Settings.Default.WireGuardClientPublicKey, true, Properties.Settings.Default.WireGuardClientInternalIp, wgKeyTimestamp);
                settings.WireGuardKeysRegenerationIntervalHours = Properties.Settings.Default.WireGuardKeysRegenerationIntervalHours;
                settings.WireGuardPreferredPortIndex = Properties.Settings.Default.WireGuardPreferredPortIndex;

                settings.UpdateEnabledServerPorts();
            }
            catch (Exception ex)
            {
                Logging.Info("Settings loading error: " + (string.IsNullOrEmpty(ex.Message) ? "<no error information>" : ex.Message));
            }
        }

        public void Save(AppSettings settings)
        {
            try
            {
                Properties.Settings.Default.IsAutomaticServerSelection = settings.IsAutomaticServerSelection;
                Properties.Settings.Default.ServerId = settings.LastUsedServerId;
                Properties.Settings.Default.ExitServerId = settings.LastUsedExitServerId;

                Properties.Settings.Default.IsAutomaticServerSelectionWg = settings.IsAutomaticServerSelectionWg;
                Properties.Settings.Default.LastUsedWgServerId = settings.LastUsedWgServerId;
                Properties.Settings.Default.LastUsedWgExitServerId = settings.LastUsedWgExitServerId;
                // Fastest server
                Properties.Settings.Default.LastOvpnFastestServerId = settings.LastOvpnFastestServerId;
                Properties.Settings.Default.LastWgFastestServerId = settings.LastWgFastestServerId;
                
                Properties.Settings.Default.AlternateAPIHost = settings.AlternateAPIHost;

                Properties.Settings.Default.Username = settings.Username;

                Properties.Settings.Default.SessionToken = settings.SessionToken;
                Properties.Settings.Default.VpnUser = settings.VpnUser;
                Properties.Settings.Default.VpnSafePass = settings.VpnSafePass;

                Properties.Settings.Default.MinimizeToTray = settings.MinimizeToTray;
                Properties.Settings.Default.LaunchMinimized = settings.LaunchMinimized;
                Properties.Settings.Default.AutoConnectOnLaunch = settings.AutoConnectOnStart;
                Properties.Settings.Default.StopServerOnClientDisconnect = settings.StopServerOnClientDisconnect;
                Properties.Settings.Default.DoNotShowDialogOnAppClose = settings.DoNotShowDialogOnAppClose;

                Properties.Settings.Default.IsAutoPortSelection = settings.IsAutoPortSelection;
                Properties.Settings.Default.PreferredPortIndex = settings.PreferredPortIndex;

                Properties.Settings.Default.ProxyType = settings.ProxyType.ToString().ToLower();
                Properties.Settings.Default.ProxyAddress = settings.ProxyServer;
                Properties.Settings.Default.ProxyPort = settings.ProxyPort;

                Properties.Settings.Default.ProxyUsername = settings.ProxyUsername;
                Properties.Settings.Default.ProxyPassword = settings.ProxySafePassword;

                Properties.Settings.Default.ServiceUseObfsProxy = settings.ServiceUseObfsProxy;
                Properties.Settings.Default.ServiceConnectOnInsecureWifi = settings.ServiceConnectOnInsecureWifi;

                Properties.Settings.Default.FirewallDisableAutoOnOff = settings.FirewallDisableAutoOnOff;
                Properties.Settings.Default.FirewallDisableDeactivationOnExit =
                    settings.FirewallDisableDeactivationOnExit;
                Properties.Settings.Default.FirewallLastStatus = settings.FirewallLastStatus;
                Properties.Settings.Default.FirewallAllowLAN = settings.FirewallAllowLAN;
                Properties.Settings.Default.FirewallAllowMulticast = settings.FirewallAllowLANMulticast;
                Properties.Settings.Default.IsIVPNFirewalIntoduced = settings.IsIVPNFirewalIntoduced;
                Properties.Settings.Default.IsIVPNAlwaysOnWarningDisplayed = settings.IsIVPNAlwaysOnWarningDisplayed;

                Properties.Settings.Default.LastWindowPosition = settings.LastWindowPosition;
                Properties.Settings.Default.LastNotificationWindowPosition = settings.LastNotificationWindowPosition;

                Properties.Settings.Default.IsMultiHop = settings.IsMultiHop;

                Properties.Settings.Default.IsLoggingEnabled = settings.IsLoggingEnabled;
                Properties.Settings.Default.OpenVPNExtraParameters = settings.OpenVPNExtraParameters;

                Properties.Settings.Default.DisableFirewallNotificationWindow = settings.DisableFirewallNotificationWindow;

                // Actions for Trusted\Untrusted networks
                Properties.Settings.Default.IsNetworkActionsEnabled = settings.IsNetworkActionsEnabled;
                Properties.Settings.Default.NetworkActions = settings.NetworkActions.Serialize();

                //Servers filter
                Properties.Settings.Default.ServersFilter = settings.ServersFilter.Serialize();

                //DNS filter
                Properties.Settings.Default.IsAntiTrackerHardcore = settings.IsAntiTrackerHardcore;
                Properties.Settings.Default.IsAntiTracker = settings.IsAntiTracker;
                Properties.Settings.Default.IsCustomDns = settings.IsCustomDns;
                Properties.Settings.Default.CustomDns = settings.CustomDns;

                // VPN type
                Properties.Settings.Default.VpnProtocolType = (int)settings.VpnProtocolType;

                // WireGuard
                Properties.Settings.Default.WireGuardClientInternalIp = settings.WireGuardClientInternalIp;
                Properties.Settings.Default.WireGuardKeysTimestamp = settings.WireGuardKeysTimestamp.ToString(DateTimeSerializationFormatString);
                Properties.Settings.Default.WireGuardKeysRegenerationIntervalHours = settings.WireGuardKeysRegenerationIntervalHours;
                Properties.Settings.Default.WireGuardPreferredPortIndex = settings.WireGuardPreferredPortIndex;
                Properties.Settings.Default.WireGuardClientPrivateKey = settings.WireGuardClientPrivateKeySafe;
                Properties.Settings.Default.WireGuardClientPublicKey = settings.WireGuardClientPublicKey;

                // Save
                Properties.Settings.Default.Save();

                SaveRunOnStartup(settings);
                SaveFirewallType(settings);
            }
            catch (Exception ex)
            {
                Logging.Info("Settings saving error: " + (string.IsNullOrEmpty(ex.Message) ? "<no error information>" : ex.Message));
            }
        }

        public void Reset(AppSettings settings)
        {
            try
            {
                // completely remove configuration folder (for all versions of application)
                // This also will prevent settings 'upgrade' from previous version of application
                // (IVPN.Properties.Settings.Default.Upgrade())
                try
                {
                    string configFileName = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
                    string currentConfigFolder = Path.GetDirectoryName(configFileName);
                    string configDirForAllAssemblies = Directory.GetParent(currentConfigFolder).FullName;
                    Directory.Delete(configDirForAllAssemblies, true);
                }
                catch (Exception ex)
                {
                    Logging.Info("Failed to erase local configuration dirrectory: " + ex);
                }

                // reset settings to default values
                Properties.Settings.Default.Reset();

                // prevent upgrading settings values from settings of oldest assemblies (oldest application versions)
                Properties.Settings.Default.IsUpgraded = true;
                Properties.Settings.Default.Save();

                // Remove RunOn strurtup entry from registry
                SaveRunOnStartup(isRunOnStartup: false);

                Load(settings);
            }
            catch (Exception ex)
            {
                Logging.Info("Settings reset error: " + (string.IsNullOrEmpty(ex.Message) ? "<no error information>" : ex.Message));
            }
        }

        private void SaveRunOnStartup(AppSettings settings)
        {
            if (settings.RunOnLogin)
                SaveRunOnStartup(isRunOnStartup: true);
            else if (settings.FirewallType == IVPNFirewallType.Persistent && settings.DisableFirewallNotificationWindow == false)
            {
                // autostart next time if firewall is still enabled (Persistent).
                // It is necessary to show 'FloatingOverlayWindow' with information about blocked traffic
                SaveRunOnStartup(isRunOnStartup: true, isRunHidden: true);
            }
            else
                SaveRunOnStartup(isRunOnStartup: false);
        }

        private void SaveRunOnStartup(bool isRunOnStartup, bool isRunHidden = false)
        {
            try
            {
                RegistryKey rkRun = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (isRunOnStartup)
                {
                    if (isRunHidden)
                        rkRun.SetValue(App.PRODUCT_NAME, Environment.GetCommandLineArgs()[0] + " " + AutoStartHiddenCmdArg);
                    else
                        rkRun.SetValue(App.PRODUCT_NAME, Environment.GetCommandLineArgs()[0] + " " + AutoStartCmdArg);
                }
                else
                {
                    if (rkRun.GetValue(App.PRODUCT_NAME) != null)
                        rkRun.DeleteValue(App.PRODUCT_NAME);
                }
                rkRun.Close();
            }
            catch (Exception)
            {
                // Some people have disabled permissions to access "Run" section of CurrentVersion.
                // This is probably done to ensure that no other software add itself to the "run automatically" list.

                // We should not notify the user of this error and just ignore it
                // tbd: Log this error
            }
        }

        private void SaveFirewallType(AppSettings settings)
        {
            RegistryKey rkRun = Registry.CurrentUser.OpenSubKey(RegistrySettingsSubKey, true);
            if (rkRun == null)
                rkRun = Registry.CurrentUser.CreateSubKey(RegistrySettingsSubKey);

            var isPersistent = settings.FirewallType == IVPNFirewallType.Persistent;
            rkRun.SetValue("IsFirewallPersistent", isPersistent.ToString());
            rkRun.Close();
        }

        private void LoadProxySettings(AppSettings settings)
        {
            switch (Properties.Settings.Default.ProxyType)
            {
                case "auto":
                    settings.ProxyType = ProxyType.Auto;
                    break;
                case "http":
                    settings.ProxyType = ProxyType.Http;
                    break;
                case "socks":
                    settings.ProxyType = ProxyType.Socks;
                    break;
                case "none":
                default:
                    settings.ProxyType = ProxyType.None;
                    break;
            }

            settings.ProxyServer = Properties.Settings.Default.ProxyAddress;
            settings.ProxyPort = Properties.Settings.Default.ProxyPort;
            settings.ProxyUsername = Properties.Settings.Default.ProxyUsername;
            settings.ProxySafePassword = Properties.Settings.Default.ProxyPassword;

            //CryptoUtil.ToInsecureString(CryptoUtil.DecryptString(Properties.Settings.Default.ProxyPassword));
        }

        private bool GetRunOnLoginFromRegistry()
        {
            try
            {
                RegistryKey rkRun = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);
                string regVal = rkRun.GetValue(App.PRODUCT_NAME).ToString();
                rkRun.Close();

                if (regVal.Contains(AutoStartHiddenCmdArg))
                    return false;

                return regVal != null;
            }
            catch (Exception) { }

            return false;
        }

        private bool? RegistryGetBoolValue(string fieldName)
        {
            RegistryKey rkRun = Registry.CurrentUser.OpenSubKey(RegistrySettingsSubKey);
            if (rkRun == null)
                return null;

            using (rkRun)
            {
                object value = rkRun.GetValue("IsFirewallPersistent");
                if (value == null)
                    return null;

                if (rkRun.GetValueKind("IsFirewallPersistent") != RegistryValueKind.String)
                    return null;

                bool resultValue;
                if (!Boolean.TryParse((string)rkRun.GetValue(fieldName), out resultValue))
                    return null;

                return resultValue;
            }
        }

        private IVPNFirewallType LoadFirewallType()
        {
            bool? isPersistent = RegistryGetBoolValue("IsFirewallPersistent");

            if (isPersistent == true)
                return IVPNFirewallType.Persistent;

            return IVPNFirewallType.Manual;
        }

        public void OnSettingsChanged(AppSettings settings, [CallerMemberName] string propertyName = "")
        {
            // no implementation here for now
        }
    }
}
