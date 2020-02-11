using System;
using System.Collections.Generic;
using NativeWifi;
using System.ComponentModel;
using System.Text;
using IVPN.WiFi;

namespace IVPN
{
    public class WindowsWiFiWrapper : IWiFiWrapper
    {
        public event WiFiNetworkConnected WiFiStateChanged = delegate { }; 

        public static WindowsWiFiWrapper Create()
        {
            var obj = Platform.GetImplementation(Platform.PlatformImplementation.WiFi) as IWiFiWrapper;

            if (obj == null)
            {
                obj = new WindowsWiFiWrapper();
                Logging.Info(obj.ToString());
                obj.Initialize();

                Platform.RegisterImplementation(Platform.PlatformImplementation.WiFi, obj);
            }

            return (WindowsWiFiWrapper) obj;
        }

        private readonly WlanClient __Client;
        private readonly object __WlanClientLocker = new object();
        private bool __IsInitialized;

        private WlanClient.WlanInterface[] GetInterfacesThreadSafe()
        {
            lock (__WlanClientLocker)
            {
                return __Client.Interfaces;
            }
        }

        public WifiState CurrentState
        {
            get
            {
                try
                {
                    WlanClient.WlanInterface[] interfaces = GetInterfacesThreadSafe();
                    if (interfaces != null)
                    {
                        foreach (WlanClient.WlanInterface iface in interfaces)
                        {
                            if (iface.InterfaceState == Wlan.WlanInterfaceState.Connected)
                            {
                                return new WifiState
                                (
                                    new WiFiNetwork(iface.CurrentConnection.profileName),
                                    IsInsecureCipherAlgorithm(iface.CurrentConnection.wlanSecurityAttributes
                                        .dot11CipherAlgorithm)
                                );
                            }
                        }
                    }
                }
                catch (Win32Exception ex)
                {
                    Logging.Info($"Error obtaining current WiFi state: {ex}");
                }
                return null;
            }
        }

        private WindowsWiFiWrapper() { __Client = new WlanClient(); }

        public void Initialize()
        {
            if (__IsInitialized)
                return;

            __IsInitialized = true;

            try
            { 
                WlanClient.WlanInterface[] interfaces = GetInterfacesThreadSafe();
                if (interfaces != null)
                { 
                    foreach (WlanClient.WlanInterface iface in interfaces)
                    {
                        Logging.Info("RegisterForNotifications: iface: " + iface.InterfaceName);

                        if (iface.InterfaceState == Wlan.WlanInterfaceState.Connected)
                            Logging.Info(string.Format("RegisterForNotifications: {0} connected, {1}", iface.InterfaceName, IsInsecureCipherAlgorithm(iface.CurrentConnection.wlanSecurityAttributes.dot11CipherAlgorithm) ? "not secure" : "secure"));

                        iface.WlanConnectionNotification += iface_WlanConnectionNotification;
                        iface.WlanNotification += iface_WlanNotification;
                    }
                }
            }
            catch (Win32Exception ex)
            {
                Logging.Info($"Internal Win32 error when accessing WiFi interface: {ex}");
            }
        }

        public void Shutdown() {}
        
        bool IsInsecureCipherAlgorithm(Wlan.Dot11CipherAlgorithm algo) { return algo == Wlan.Dot11CipherAlgorithm.None || algo == Wlan.Dot11CipherAlgorithm.WEP40; }

        private void iface_WlanNotification(Wlan.WlanNotificationData notifydata)
        {
            if (notifydata.notificationSource == Wlan.WlanNotificationSource.ACM
                && (Wlan.WlanNotificationCodeAcm) notifydata.notificationCode == Wlan.WlanNotificationCodeAcm.ScanComplete)
            {
                OnNetworksScanComplete();
            }
        }

        private void iface_WlanConnectionNotification(Wlan.WlanNotificationData notifyData, Wlan.WlanConnectionNotificationData connNotifyData)
        {
            WlanClient.WlanInterface iface = GetInterfaceByGuid(notifyData.interfaceGuid);
            if (iface == null)
            {
                Logging.Info("IVPN Network Status: no interface for GUID: " + notifyData.interfaceGuid);
                return;
            }

            try
            {
                WiFiStateChanged(CurrentState);

                Logging.Info(string.Format("IVPN Network Status Changed: interface {0} ({1}, cipher: {2}, state: {3})",
                    iface.InterfaceName,
                    IsInsecureCipherAlgorithm(iface.CurrentConnection.wlanSecurityAttributes.dot11CipherAlgorithm) ? "not secure" : "secure",
                    iface.CurrentConnection.wlanSecurityAttributes.dot11CipherAlgorithm,
                    iface.InterfaceState));
            }
            catch (Win32Exception ex)
            {
                Logging.Info(string.Format("IVPN Network Status exception while notifying: {0}", ex));
            }
        }

        WlanClient.WlanInterface GetInterfaceByGuid(Guid interfaceGuid)
        {
            WlanClient.WlanInterface[] interfaces = GetInterfacesThreadSafe();
            if (interfaces != null)
            { 
                foreach (WlanClient.WlanInterface iface in interfaces)
                {
                    if (iface.InterfaceGuid == interfaceGuid) 
                        return iface;
                }
            }
            return null;
        }

        public event OnNetworksScanCompleteDelegate OnNetworksScanComplete = delegate { };

        public IEnumerable<WiFiNetworkInfo> GetWifiNetworks()
        {
            List<WiFiNetworkInfo> ret = new List<WiFiNetworkInfo>();

            WlanClient.WlanInterface[] interfaces = GetInterfacesThreadSafe();
            if (interfaces != null)
            { 
                foreach (WlanClient.WlanInterface wlanIface in interfaces)
                {
                    if (wlanIface.InterfaceState == Wlan.WlanInterfaceState.Disconnected || wlanIface.InterfaceState == Wlan.WlanInterfaceState.NotReady)
                        continue;

                    try
                    {
                        foreach (Wlan.WlanBssEntry bss in wlanIface.GetNetworkBssList())
                        {
                            Wlan.Dot11Ssid ssid = bss.dot11Ssid;
                            string networkname = Encoding.UTF8.GetString(ssid.SSID, 0, (int) ssid.SSIDLength);
                            if (!string.IsNullOrEmpty(networkname))
                                ret.Add(new WiFiNetworkInfo(new WiFiNetwork(networkname)));
                        }
                    }
                    catch
                    {
                        // Ignore everything
                    }
                }
            }

            return ret;
        }

        public void StartWiFiNetworksScan()
        {
            try
            {
                WlanClient.WlanInterface[] interfaces = GetInterfacesThreadSafe();
                if (interfaces != null)
                { 
                    foreach (WlanClient.WlanInterface wlanIface in interfaces)
                    {
                        if (wlanIface.InterfaceState == Wlan.WlanInterfaceState.Disconnected || wlanIface.InterfaceState == Wlan.WlanInterfaceState.NotReady)
                            continue;

                        wlanIface.Scan();
                    }
                }
            }
            catch
            {
                // Ignore everything
            }
        }
    }
}
