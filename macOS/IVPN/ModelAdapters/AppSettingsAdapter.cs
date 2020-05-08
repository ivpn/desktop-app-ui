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

using Foundation;

using IVPN.Models;
using IVPN.Models.Configuration;

namespace IVPN
{
    public class AppSettingsAdapter: ObservableObject
    {
        private NSMutableArray __PreferedPortValues;
        private NSMutableArray __WireGuardPreferedPortValues;

        public AppSettingsAdapter(AppSettings settings): base(settings)
        {
            __PreferedPortValues = new NSMutableArray();
            foreach (var port in settings.PreferredPortsList)
                __PreferedPortValues.Add(new NSString(port.ToString()));

            __WireGuardPreferedPortValues = new NSMutableArray();
            foreach (var port in settings.WireGuardPreferredPortsList)
                __WireGuardPreferedPortValues.Add(new NSString(port.ToString()));
        }

        [Export("PreferedPortValues")]
        public NSArray PreferedPortValues
        {
            get => __PreferedPortValues;
        }

        [Export("WireGuardPreferedPortValues")]
        public NSArray WireGuardPreferedPortValues
        {
            get => __WireGuardPreferedPortValues;
        }

        [Export("ProxyTypeId")]
        public int ProxyTypeId
        {
            get => (int)Settings.ProxyType;

            set {
                WillChangeValue("IsProxyEditable");
                WillChangeValue("ProxyTypeId");
                Settings.ProxyType = (ProxyType)value;
                DidChangeValue("ProxyTypeId");
                DidChangeValue("IsProxyEditable");
            }
        }

        [Export("IsProxyEditable")]
        public bool IsProxyEditable
        {
            get => Settings.ProxyType == ProxyType.Http || Settings.ProxyType == ProxyType.Socks;
        }

        [Export("FirewallTypeId")]
        public int FirewallTypeId
        {
            get => (int)Settings.FirewallType;
            set 
            {
                WillChangeValue("IsManualFirewall");
                WillChangeValue("FirewallTypeId");
                Settings.FirewallType = (IVPNFirewallType)value;
                DidChangeValue("FirewallTypeId");
                DidChangeValue("IsManualFirewall");

            }
        }

        [Export("IsManualFirewall")]
        public bool IsManualFirewall
        {
            get => Settings.FirewallType == IVPNFirewallType.Manual;
        }

        public AppSettings Settings
        {
            get => (AppSettings)ObservedObject;
        }
    }
}

