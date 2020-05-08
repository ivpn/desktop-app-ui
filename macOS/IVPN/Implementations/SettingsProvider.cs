//
//  IVPN Client Desktop
//  https://github.com/ivpn/desktop-app-ui
//
//  Created by Stelnykovych Alexandr.
//  Copyright (c) 2020 Privatus Limited.
//
//  This file is part of the IVPN Client Desktop.
//
//  The IVPN Client Desktop is free software: you can redistribute it and/or
//  modify it under the terms of the GNU General Public License as published by the Free
//  Software Foundation, either version 3 of the License, or (at your option) any later version.
//
//  The IVPN Client Desktop is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
//  details.
//
//  You should have received a copy of the GNU General Public License
//  along with the IVPN Client Desktop. If not, see <https://www.gnu.org/licenses/>.
//

﻿using System;
using System.Reflection;

using System.Collections.Generic;
using Foundation;

using IVPN.Models;
using IVPN.Interfaces;
using System.IO;
using System.Xml.Serialization;
using IVPN.Models.Configuration;
using IVPN.VpnProtocols;
using IVPN.Lib;

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
                                
                Load(settings);
            }
            catch (Exception ex)
            {
                Logging.Info("[ERROR] Failed to reset settings to defaults: " + ex);
                throw;
            }
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
            AccountID = "";
            Session = "";
            OvpnUser = "";
            OvpnPass = "";
            WgPublicKey = "";
            WgPrivateKey = "";
            WgLocalIP = "";
            WgKeyGenerated = 0;

            // READ OLD-STYLE CREDENTIALS (compatibility with older client versions) 
            // USERNAME
            AccountID = NSUserDefaults.StandardUserDefaults.StringForKey("Username");
            if (string.IsNullOrEmpty(AccountID))
                return false; // unknown user - do not load the rest
            // SESSION
            Session = NSUserDefaults.StandardUserDefaults.StringForKey("SessionToken");
            OvpnUser = NSUserDefaults.StandardUserDefaults.StringForKey("VpnUser");
            OvpnPass = CryptoUtil.DecryptString(KeyChain.GetSecuredValueFromKeychain(AccountID, "VpnSafePass"));
            // WIREGUARD
            WgPublicKey = NSUserDefaults.StandardUserDefaults.StringForKey("WireGuardClientPublicKey");
            WgLocalIP = NSUserDefaults.StandardUserDefaults.StringForKey("WireGuardClientInternalIp");
            WgPrivateKey = CryptoUtil.DecryptString(KeyChain.GetSecuredValueFromKeychain(AccountID, "WireGuardClientPrivateKeySafe"));
            string keysTimestampString = KeyChain.GetSecuredValueFromKeychain(AccountID, "WireGuardKeysTimestamp");
            if (string.IsNullOrEmpty(keysTimestampString) || !DateTime.TryParse(keysTimestampString, out DateTime keysTimestamp))
                keysTimestamp = default;
            WgKeyGenerated = IVPN_Helpers.DataConverters.DateTimeConverter.ToUnixTime(keysTimestamp);

            // REMOVE ALL OLD-STYLE CREDENTIALS
            bool isHasUsername = !string.IsNullOrEmpty(AccountID);
            // Remove user password
            if (isHasUsername)
                KeyChain.RemoveCredentialFromKeychain(AccountID);
            // Remove wireguard info
            NSUserDefaults.StandardUserDefaults.RemoveObject("WireGuardClientPublicKey");
            NSUserDefaults.StandardUserDefaults.RemoveObject("WireGuardClientInternalIp");
            if (isHasUsername)
            {
                KeyChain.RemoveSecuredValueFromKeychain(AccountID, "WireGuardClientPrivateKeySafe");
                KeyChain.RemoveSecuredValueFromKeychain(AccountID, "WireGuardKeysTimestamp");
            }
            // Remove session info
            NSUserDefaults.StandardUserDefaults.RemoveObject("SessionToken");
            NSUserDefaults.StandardUserDefaults.RemoveObject("VpnUser");
            if (isHasUsername)
                KeyChain.RemoveSecuredValueFromKeychain(AccountID, "VpnSafePass");

            NSUserDefaults.StandardUserDefaults.RemoveObject("Username");

            if (string.IsNullOrEmpty(Session))
                return false;
            return true;
        }
    }
}

