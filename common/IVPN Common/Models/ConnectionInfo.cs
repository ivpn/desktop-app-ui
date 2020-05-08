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

namespace IVPN.Models
{
    public class ConnectionInfo : ModelBase
    {
        private string __DurationString;
        private bool __IsDurationStopped;
        public ConnectionInfo(ServerLocation server, ServerLocation exitServer, DateTime connectTime, string clientIPAddress, string serverIPAddress, VpnProtocols.VpnType vpnType)
        {
            Server = server;
            ExitServer = exitServer;
            ConnectTime = connectTime;
            ClientIPAddress = clientIPAddress;
            ServerIPAddress = serverIPAddress;
            VpnType = vpnType;
        }

        public void UpdateDuration()
        {
            if (__IsDurationStopped)
            {
                DurationString = "";
                return;
            }

            TimeSpan ts = DateTime.UtcNow.Subtract(ConnectTime);
            if (ts.Days == 0)
                DurationString = $"{ts.Hours + (ts.Days * 24):D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
            else if (ts.Days == 1)
                DurationString = $"{ts.Days} day {ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
            else
                DurationString = $"{ts.Days} days {ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        public void SetConnectTime(DateTime connectTime)
        {
            ConnectTime = connectTime;
        }

        /// <summary>
        /// Useful when pausing connection
        /// </summary>
        public void DurationStop()
        {
            __IsDurationStopped = true;
            UpdateDuration();
        }

        /// <summary>
        /// Useful when resuming connection
        /// </summary>
        public void DurationStart()
        {
            __IsDurationStopped = false;

            DoPropertyWillChange(nameof(ConnectTime));
            ConnectTime = DateTime.UtcNow;
            DoPropertyChanged(nameof(ConnectTime));

            UpdateDuration();
        }

        public string DurationString
        {
            get => __DurationString;

            private set
            {
                DoPropertyWillChange();
                __DurationString = value;
                DoPropertyChanged();
            }
        }

        public DateTime ConnectTime { get; private set; }

        public string ClientIPAddress { get; }

        public string ServerIPAddress { get; }

        public ServerLocation Server { get; }

        public ServerLocation ExitServer { get; }

        public VpnProtocols.VpnType VpnType { get; }

        public string VpnProtocolInfo => VpnType.ToString();
    }
}
