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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using IVPN.VpnProtocols;

namespace IVPN.Models
{
    public class ServerLocation : INotifyPropertyChanged
    {
        private readonly string __Name;
        private int __PingTime;
        private double __PingTimeRelative;

        private bool __IsSelected;
        
        public ServerLocation(VpnServerInfoBase vpnServer)
            : this(vpnServer.City,
                vpnServer.CountryCode,
                vpnServer)
        {}

        public ServerLocation(string name, string countryCode, VpnServerInfoBase vpnServer)
        {
            MultihopId = vpnServer.GetMultihopId();
            __Name = name;
            CountryCode = countryCode;
            VpnServer = vpnServer;
        }
        
        private string FormatName(string city, string countryCode)
        {
            return $"{city}, {countryCode}";
        }

        public string Name => FormatName(__Name, CountryCode);

        public string CountryCode { get; }

        public string MultihopId { get; }

        public VpnServerInfoBase VpnServer { get; }

        public bool IsSelected
        {
            get => __IsSelected;
            set
            {
                if (__IsSelected == value)
                    return;
                __IsSelected = value;
                DoPropertyChanged();
            }
        }

        public int PingTime
        {
            get => __PingTime;
            set
            {
                LastPingUpdateTime = DateTime.Now;

                if (__PingTime == value)
                    return;

                __PingTime = value;
                DoPropertyChanged();
            }
        }

        public DateTime LastPingUpdateTime { get; private set; }

        /// <summary>
        /// Relative ping time [0.0 - 1.0]: 
        ///     0 - small ping (green on GUI)
        ///     1 - big ping (red on GUI)
        /// Determine ping "quality" in comparison to ping-result from other servers. 
        /// </summary>
        public double PingTimeRelative
        {
            get => __PingTimeRelative;
            set
            {
                if (System.Math.Abs(__PingTimeRelative - value) < 0.001)
                    return;
                __PingTimeRelative = value;
                DoPropertyChanged();
            }
        }

        private bool __IsCountryDisallowed;
        public bool IsCountryDisallowed 
        {
            get => __IsCountryDisallowed;
            set
            {                
                __IsCountryDisallowed = value;
                DoPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void DoPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
