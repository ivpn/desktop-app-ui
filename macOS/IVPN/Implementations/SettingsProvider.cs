using System;
using System.Reflection;
using System.Security.Cryptography;

using System.Collections.Generic;
using Foundation;

using IVPN.Models;
using IVPN.Interfaces;
using IVPN.Lib;
using System.IO;
using System.Xml.Serialization;
using IVPN.Models.Configuration;
using IVPN.VpnProtocols;

namespace IVPN
{
    public class SettingsProvider : ISettingsProvider
    {
        private static bool __DefaultsRegistered = false;
        private static Dictionary<string, object> __Defaults;

        public SettingsProvider()
        {
            __Defaults = new Dictionary<string, object>();

            __Defaults["VpnProtocolType"] = (int)VpnType.OpenVPN;

            __Defaults["AlternateAPIHost"] = "";

            __Defaults["IsFirstIntroductionDone"] = false;
            __Defaults["AutoConnectOnStart"] = false;
            __Defaults["LaunchMinimized"] = false;

            __Defaults["DoNotShowDialogOnAppClose"] = false;
                        
            __Defaults["WireGuardPreferredPortIndex"] = 0;
            __Defaults["WireGuardKeysRegenerationIntervalHours"] = 24 * 7; // 7 days

            __Defaults["ProxyType"] = (int)ProxyType.None;
            __Defaults["ProxyServer"] = "";
            __Defaults["ProxyPort"] = 8080;
            __Defaults["ProxyUsername"] = "";
            __Defaults["ProxySafePassword"] = "";
            __Defaults["ServiceUseObfsProxy"] = false;
            __Defaults["ServiceConnectOnInsecureWifi"] = false;

            __Defaults["IsAutomaticServerSelection"] = true;
            __Defaults["IsAutomaticServerSelectionWg"] = true;

            __Defaults["LastUsedServerId"] = "";
            __Defaults["LastUsedExitServerId"] = "";
            __Defaults["LastUsedWgServerId"] = "";
            __Defaults["LastUsedWgExitServerId"] = "";

            __Defaults["LastOvpnFastestServerId"] = "";
            __Defaults["LastWgFastestServerId"] = "";

            __Defaults["IsMultiHop"] = false;

            __Defaults["PreferredPortIndex"] = 0;
            __Defaults["IsAutoPortSelection"] = true;

            __Defaults["FirewallAutoOnOff"] = true;
            __Defaults["FirewallDeactivationOnExit"] = true;
            __Defaults["FirewallLastStatus"] = false;
            __Defaults["FirewallAllowLAN"] = false;
            __Defaults["FirewallAllowLANMulticast"] = true;
            __Defaults["IsIVPNFirewalIntoduced"] = false;
            __Defaults["FirewallType"] = (int)IVPNFirewallType.Manual;
            __Defaults["IsIVPNAlwaysOnWarningDisplayed"] = false;

            __Defaults["IsLoggingEnabled"] = true;

            __Defaults["LastWindowPosition"] = "";
            __Defaults["LastNotificationWindowPosition"] = "";

            __Defaults["OpenVPNExtraParameters"] = "";
            __Defaults["DisableFirewallNotificationWindow"] = false;
            __Defaults["RunOnLogin"] = false;
            __Defaults["MacIsShowIconInSystemDock"] = false;
            __Defaults["StopServerOnClientDisconnect"] = false;

            __Defaults["IsNetworkActionsEnabled"] = true;

            __Defaults["ServersFilter"] = new ServersFilterConfig();
            __Defaults["NetworkActions"] = new NetworkActionsConfig();

            __Defaults["IsAntiTrackerHardcore"] = false;
            __Defaults["IsAntiTracker"] = false;
            __Defaults["IsCustomDns"] = false;
            __Defaults["CustomDns"] = "";

        }

        private void RegisterDefaults(NSDictionary defaults)
        {
            NSUserDefaults.StandardUserDefaults.RegisterDefaults(defaults);
        }

        private NSDictionary NSDictionaryFromDictionary(Dictionary<string, object> dictionary)
        {
            NSMutableDictionary newDictionary = new NSMutableDictionary();

            foreach (var key in dictionary.Keys)
            {
                object value = dictionary[key];

                if (value is string)
                    newDictionary.Add(new NSString(key), new NSString((string)value));
                else if (value is int)
                    newDictionary.Add(new NSString(key), NSNumber.FromInt32((int)value));
                else if (value is bool)
                    newDictionary.Add(new NSString(key), NSNumber.FromBoolean((bool)value));
                /*else if (value is DateTime)
                    newDictionary.Add (new NSString (key), MacLib.MacHelpers.DateTimeToNSDate((DateTime)value));*/
                else if (value is List<string>)
                {
                    List<string> list = value as List<string>;
                    if (list == null)
                        list = new List<string>();

                    StringWriter sw = new StringWriter();
                    XmlSerializer s = new XmlSerializer(list.GetType());
                    s.Serialize(sw, list);

                    newDictionary.Add(new NSString(key), new NSString((string)list.ToString()));
                }
                else if (value is ServersFilterConfig)
                    newDictionary.Add(new NSString(key), new NSString(((ServersFilterConfig)value).Serialize()));
                else if (value is NetworkActionsConfig)
                    newDictionary.Add(new NSString(key), new NSString(((NetworkActionsConfig)value).Serialize()));
                else 
                    throw new InvalidOperationException("Unsupported property type in Settings Defaults");
            }

            return NSDictionary.FromDictionary(newDictionary);
        }

        private void RegisterDefaults()
        {
            if (!__DefaultsRegistered)
            {
                RegisterDefaults(NSDictionaryFromDictionary(__Defaults));
                __DefaultsRegistered = true;
            }
        }

        private PropertyInfo GetPropertyInfo(AppSettings settings, string propertyName)
        {
            var propertyInfo = settings.GetType().GetProperty(propertyName);

            if (propertyInfo == null)
                throw new InvalidOperationException($"Trying to save non-existing property from AppSettings ('{propertyName}')");

            return propertyInfo;
        }

        private void SaveProperty(AppSettings settings, string propertyName)
        {

            var propertyInfo = GetPropertyInfo(settings, propertyName);

            var value = propertyInfo.GetValue(settings);

            var propertyType = propertyInfo.PropertyType;

            if (propertyType == typeof(string))
                NSUserDefaults.StandardUserDefaults.SetString((string)value, propertyName);
            else if (propertyType == typeof(int))
                NSUserDefaults.StandardUserDefaults.SetInt((int)value, propertyName);
            else if (propertyType == typeof(bool))
                NSUserDefaults.StandardUserDefaults.SetBool((bool)value, propertyName);
            else if (propertyType.BaseType == typeof(Enum))
                NSUserDefaults.StandardUserDefaults.SetInt(Convert.ToInt32((Enum)value), propertyName);
            /*else if (propertyType == typeof (DateTime))
                NSUserDefaults.StandardUserDefaults.SetValueForKey (MacLib.MacHelpers.DateTimeToNSDate ((DateTime)value), MacLib.MacHelpers.ToNSString (propertyName));
            */
            else if (propertyType == Type.GetType("System.Collections.Generic.List`1[System.String]"))
            {
                List<string> list = value as List<string>;
                if (list == null)
                    list = new List<string>();

                StringWriter sw = new StringWriter();
                XmlSerializer s = new XmlSerializer(list.GetType());
                s.Serialize(sw, list);

                string serialized = sw.ToString();

                NSUserDefaults.StandardUserDefaults.SetString((string)serialized, propertyName);
            }
            else if (propertyType == typeof(ServersFilterConfig))
                NSUserDefaults.StandardUserDefaults.SetString(((ServersFilterConfig)value).Serialize(), propertyName);
            else if (propertyType == typeof(NetworkActionsConfig))
                NSUserDefaults.StandardUserDefaults.SetString(((NetworkActionsConfig)value).Serialize(), propertyName);
            else
                throw new InvalidOperationException("Trying to save property from AppSettings with unsupported type");
        }

        private void LoadProperty(AppSettings settings, string propertyName)
        {
            var propertyInfo = GetPropertyInfo(settings, propertyName);

            var propertyType = propertyInfo.PropertyType;

            object value;
            if (propertyType == typeof(string))
                value = (string)NSUserDefaults.StandardUserDefaults.StringForKey(propertyName);
            else if (propertyType == typeof(int))
                value = (int)NSUserDefaults.StandardUserDefaults.IntForKey(propertyName);
            else if (propertyType == typeof(bool))
                value = (bool)NSUserDefaults.StandardUserDefaults.BoolForKey(propertyName);
            else if (propertyType.BaseType == typeof(Enum))
                value = Enum.ToObject(propertyType, (int)NSUserDefaults.StandardUserDefaults.IntForKey(propertyName));
            /*else if (propertyType == typeof (DateTime)) 
            {
                NSDate date = (NSDate) NSUserDefaults.StandardUserDefaults.ValueForKey (MacLib.MacHelpers.ToNSString (propertyName));
                value = MacLib.MacHelpers.NSDateToDateTime (date);
            }*/
            else if (propertyType == Type.GetType("System.Collections.Generic.List`1[System.String]"))
            {
                try
                {
                    string serialized = (string)NSUserDefaults.StandardUserDefaults.StringForKey(propertyName);
                    XmlSerializer xs = new XmlSerializer(typeof(List<string>));
                    List<string> newList = (List<string>)xs.Deserialize(new StringReader(serialized));
                    value = newList;
                }
                catch { value = new List<string>(); }
            }
            else if (propertyType == typeof(ServersFilterConfig))
                value = ServersFilterConfig.Deserialize((string)NSUserDefaults.StandardUserDefaults.StringForKey(propertyName));
            else if (propertyType == typeof(NetworkActionsConfig))
                value = NetworkActionsConfig.Deserialize((string)NSUserDefaults.StandardUserDefaults.StringForKey(propertyName));
            else
                throw new InvalidOperationException("Trying to load property from AppSettings with unsupported type");

            propertyInfo.SetValue(settings, value);

        }
    
        private string LoadPasswordFromKeyChain(string username)
        {
            try
            {
                if (!string.IsNullOrEmpty(username))
                {
                    var password = KeyChain.GetCredentialFromFromKeychain(username);
                    if (!string.IsNullOrEmpty(password))
                        return CryptoUtil.EncryptString(password);
                }
            }
            catch (CryptographicException ex)
            {
                Logging.Info(string.Format("Error loading password from KeyChain: {0}", ex));
            }

            return "";
        }

        #region Acount credentials
        private void SaveCredentials(AppSettings settings)
        {
            if (settings.IsTempCredentialsSaved())
                return;

            string username = settings.Username;
            // USERNAME
            NSUserDefaults.StandardUserDefaults.SetString(username, nameof(AppSettings.Username));

            if (string.IsNullOrEmpty(username))
                return;

            // SESSION
            NSUserDefaults.StandardUserDefaults.SetString(settings.SessionToken, nameof(AppSettings.SessionToken));
            NSUserDefaults.StandardUserDefaults.SetString(settings.VpnUser, nameof(AppSettings.VpnUser));
            KeyChain.SaveSecuredValueToKeychain(username, nameof(AppSettings.VpnSafePass), settings.VpnSafePass);

            // WIREGUARD
            string clientPublicKey = settings.WireGuardClientPublicKey ?? "";
            string privateKeySafe = settings.WireGuardClientPrivateKeySafe ?? "";
            string clientInternalIp = settings.WireGuardClientInternalIp ?? "";

            NSUserDefaults.StandardUserDefaults.SetString(clientPublicKey, nameof(AppSettings.WireGuardClientPublicKey));
            NSUserDefaults.StandardUserDefaults.SetString(clientInternalIp, nameof(AppSettings.WireGuardClientInternalIp));
            KeyChain.SaveSecuredValueToKeychain(username, nameof(AppSettings.WireGuardClientPrivateKeySafe), privateKeySafe);
            KeyChain.SaveSecuredValueToKeychain(username, nameof(AppSettings.WireGuardKeysTimestamp), settings.WireGuardKeysTimestamp.ToString());
        }

        private void LoadCredentials(AppSettings settings)
        {
            // USERNAME
            string username = NSUserDefaults.StandardUserDefaults.StringForKey(nameof(AppSettings.Username));

            if (string.IsNullOrEmpty(username))
            {
                settings.DeleteSession();
                settings.SetWireGuardCredentials( null, null, true, null);
                return; // unknown user - do not load the rest
            }

            // SESSION
            string session = NSUserDefaults.StandardUserDefaults.StringForKey(nameof(AppSettings.SessionToken));
            string vpnUser = NSUserDefaults.StandardUserDefaults.StringForKey(nameof(AppSettings.VpnUser));
            string vpnPass = KeyChain.GetSecuredValueFromKeychain(username, nameof(AppSettings.VpnSafePass));

            settings.SetSession(username, session, vpnUser, vpnPass, isPassEncrypded: true);

            // WIREGUARD
            string publicKey = NSUserDefaults.StandardUserDefaults.StringForKey(nameof(AppSettings.WireGuardClientPublicKey));
            string internalIp = NSUserDefaults.StandardUserDefaults.StringForKey(nameof(AppSettings.WireGuardClientInternalIp));
            string privateKey = KeyChain.GetSecuredValueFromKeychain(settings.Username, nameof(AppSettings.WireGuardClientPrivateKeySafe));
            string keysTimestampString = KeyChain.GetSecuredValueFromKeychain(settings.Username, nameof(AppSettings.WireGuardKeysTimestamp));
            if (string.IsNullOrEmpty(keysTimestampString) || !DateTime.TryParse(keysTimestampString, out DateTime keysTimestamp))
                keysTimestamp = DateTime.Now;
            
            settings.SetWireGuardCredentials(privateKey, publicKey, true, internalIp, keysTimestamp);
        }

        private void ResetCredentials(string username)
        {
            bool isHasUsername = !string.IsNullOrEmpty(username);

            // Remove user password
            if (isHasUsername)
                KeyChain.RemoveCredentialFromKeychain(username);

            // Remove wireguard info
            NSUserDefaults.StandardUserDefaults.RemoveObject(nameof(AppSettings.WireGuardClientPublicKey));
            NSUserDefaults.StandardUserDefaults.RemoveObject(nameof(AppSettings.WireGuardClientInternalIp));
            if (isHasUsername)
            {
                KeyChain.RemoveSecuredValueFromKeychain(username, nameof(AppSettings.WireGuardClientPrivateKeySafe));
                KeyChain.RemoveSecuredValueFromKeychain(username, nameof(AppSettings.WireGuardKeysTimestamp));
            }

            // Remove session info
            NSUserDefaults.StandardUserDefaults.RemoveObject(nameof(AppSettings.SessionToken));
            NSUserDefaults.StandardUserDefaults.RemoveObject(nameof(AppSettings.VpnUser));
            if (isHasUsername)
                KeyChain.RemoveSecuredValueFromKeychain(username, nameof(AppSettings.VpnSafePass));
        }
        #endregion //Credentials

        private static void SaveLoginItem(bool RunOnLogin)
        {
            if (RunOnLogin)
                LoginItems.AddLoginItem();
            else
                LoginItems.RemoveLoginItem();
        }

        /// <summary>
        /// Some settings should be saved immediately.
        /// For example, "RunOnLogin" for macOS implementation
        /// </summary>
        public void OnSettingsChanged(AppSettings settings, string propertyName = "")
        {
            if (__IsLoading || settings == null)
                return;

            if (string.Equals(propertyName, nameof(AppSettings.RunOnLogin)))
            {
                SaveLoginItem(settings.RunOnLogin);
            }
        }

        public void Save(AppSettings settings)
        {
            if (__IsLoading)
                return;
            // "RunOnLogin" was already saved ( see OnSettingsChanged() )
            // SaveLoginItem(settings.RunOnLogin);

            try
            {
                // Application should autostart when Firewall=Persistant.
                // In this case will be possible to show "FirewallNotificatioWindow" after login
                if (settings.RunOnLogin == false && settings.FirewallType == IVPNFirewallType.Persistent)
                    SaveLoginItem(true);
            }
            catch(Exception ex)
            {
                Logging.Info($"ERROR saving 'RunOnLogin' settings: {ex}");
            }

            foreach (var propertyName in __Defaults.Keys)
            {
                SaveProperty(settings, propertyName.ToString());
            }

            SaveCredentials(settings);
        }

        private bool __IsLoading;
        public void Load(AppSettings settings)
        {
            Exception firstExp = null;

            __IsLoading = true;
            try
            {
                RegisterDefaults();
                
                foreach (var propertyName in __Defaults.Keys)
                {
                    try
                    {
                        LoadProperty(settings, propertyName.ToString());
                    }
                    catch (Exception e)
                    {
                        // Save first exception but keep loading other properties
                        if (firstExp == null)
                            firstExp = new Exception($"Failed to load property '{propertyName}'", e);
                    }
                }

                try
                {
                    settings.RunOnLogin = LoginItems.IsLoginItemInstalled();
                }
                catch (Exception ex)
                {
                    settings.RunOnLogin = false;
                    Logging.Info($"ERROR loading 'RunOnLogin' settings: {ex}");
                }

                LoadCredentials(settings);

                Save(settings);

                if (firstExp != null)
                    throw firstExp;
            }
            finally
            {
                __IsLoading = false;
            }
        }

        public void Reset(AppSettings settings)
        {
            try
            {
                foreach (var propertyName in __Defaults.Keys)
                {
                    NSUserDefaults.StandardUserDefaults.RemoveObject(propertyName);
                }

                ResetCredentials(settings.Username);
                                
                Load(settings);
            }
            catch (Exception ex)
            {
                Logging.Info("[ERROR] Failed to reset settings to defaults: " + ex);
                throw;
            }
        }
    }
}

