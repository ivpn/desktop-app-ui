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

using System;
using System.Collections.Generic;
using CoreWlan;
using Foundation;
using IVPN.WiFi;
using MacLib;

namespace IVPN {
    
    public class MacWiFiWrapper : IWiFiWrapper 
    {
        public event OnNetworksScanCompleteDelegate OnNetworksScanComplete;
        public event WiFiNetworkConnected WiFiStateChanged = delegate {};

        public static MacWiFiWrapper Create() 
        {
            IWiFiWrapper obj = Platform.GetImplementation(Platform.PlatformImplementation.WiFi) as IWiFiWrapper;

            if (obj == null) 
            {
                obj = new MacWiFiWrapper();
                obj.Initialize();
                Logging.Info(obj.ToString());
                Platform.RegisterImplementation(Platform.PlatformImplementation.WiFi, obj);
            }

            return (MacWiFiWrapper) obj;
        }

        private readonly object __InsecureLock = new object();
        private CWInterface __Interface;
        private NSObject __Observer;

        private WifiState __CurrentState;
        public WifiState CurrentState 
        {
            get 
            {
                lock (__InsecureLock) 
                {
                    return __CurrentState;
                }
            }
        }

        private MacWiFiWrapper() 
        {
            try 
            {
                __Interface = CWInterface.MainInterface;
            } 
            catch(Exception ex) 
            {
                Logging.Info("error creating WiFi wrapper: " + ex.Message);
                Logging.Info(ex.StackTrace);
            }
        }

        public void Initialize() 
        {
            Logging.Info("installing CWLinkDidChangeNotification observer");

            UpdateWiFiState();

            __Observer = NSNotificationCenter.DefaultCenter.AddObserver( MacHelpers.ToNSString ("com.apple.coreWLAN.notification.link") /* CWLinkDidChangeNotification */, (NSNotification obj) =>  {
                Logging.Info("CWLinkDidChangeNotification");

                UpdateWiFiState();
            });
        }

        private void UpdateWiFiState()
        {
            lock(__InsecureLock) 
            {
                try
                {
                    var network = new WiFiNetwork(__Interface.Ssid);
                    bool isInsecureNetwork = (int)__Interface.InterfaceState == 4 /* kCWInterfaceStateRunning */ &&
                                                            (int)__Interface.SecurityMode == 0   /* kCWSecurityModeOpen */;

                    __CurrentState = new WifiState(network, isInsecureNetwork);

                    WiFiStateChanged(__CurrentState);
                }
                catch (Exception ex)
                {
                    Logging.Info($"Inrernal ERROR parsing info about connected WiFi network '{ex.Message}' : {ex}");
                }
            }
        }

        public void Shutdown() 
        {
            Logging.Info("removing CWLinkDidChangeNotification observer");
            NSNotificationCenter.DefaultCenter.RemoveObserver(__Observer);
        }

        public IEnumerable<WiFiNetworkInfo> GetWifiNetworks()
        {
            List<WiFiNetworkInfo> ret = new List<WiFiNetworkInfo>();
            if (__Interface == null)
                return ret;
             CWNetwork[] networks = __Interface.CachedScanResults;
            if (networks != null)
            {
                foreach (CWNetwork net in networks)
                {
                    try
                    {
                        var network = new WiFiNetwork(net.Ssid);
                        ret.Add(new WiFiNetworkInfo(network));
                    }
                    catch (Exception ex)
                    {
                        Logging.Info($"Inrernal ERROR parsing info about WiFi networks '{ex.Message}' : {ex}");
                    }
                }
            }             
            return ret;
        }

        public void StartWiFiNetworksScan()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    NSError error;
                    __Interface.ScanForNetworksWithSsid(null, false, out error);

                    OnNetworksScanComplete();
                }
                catch
                {
                    // ignore everything
                }
            });
        }

        private byte[] GetBssidDataFromBssid(string bssid)
        {
            byte[] ret = new byte[6];

            if (string.IsNullOrEmpty(bssid))
                return ret;
            
            String[] parts = bssid.Split(':');
            for (int i = 0; i < 6; i++)
            {
                byte hex = byte.Parse(parts[i], System.Globalization.NumberStyles.AllowHexSpecifier);
                ret[i] = hex;
            }
            return ret;
        }
    }
}