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
using System.Collections.Generic;
using System.Net;
using IVPN.Models;
using IVPN.Models.Configuration;
using IVPN.VpnProtocols;

namespace IVPN
{
    public class ConnectionTarget
    {
        private ConnectionTarget(ServerLocation server, string openVpnMultihopExitSrvId, DestinationPort port, List<DestinationPort> portsToReconnect, IPAddress currentManualDns)
        {
            Server = server;
            OpenVpnMultihopExitSrvId = openVpnMultihopExitSrvId;
            Port = port;
            CurrentManualDns = currentManualDns;

            if (portsToReconnect != null && portsToReconnect.Count > 0)
            {
                if (!portsToReconnect.Contains(port))
                    throw new ArgumentOutOfRangeException(nameof(portsToReconnect), "Preffered ports list does not caontain Port which is defined as first");

                PortsToReconnect.AddRange(portsToReconnect);
            }
        }

        public ConnectionTarget(ServerLocation server, string openVpnMultihopExitSrvId, DestinationPort port, List<DestinationPort> portsToReconnect, IPAddress currentManualDns, ProxyOptions proxyOptions)
            : this(server, openVpnMultihopExitSrvId, port, portsToReconnect, currentManualDns)
        {
            // TODO: necessary to think how to divide implementation for OpenVPN and Wireguard 
            OpenVpnProxyOptions = proxyOptions;            
        }

        public ServerLocation Server { get; }

        public DestinationPort Port { get; private set; }

        public List<DestinationPort> PortsToReconnect { get; } = new List<DestinationPort>();

        public IPAddress CurrentManualDns { get; set; }

        public DestinationPort ChangeToNextPort()
        {
            if (PortsToReconnect.Count <= 0)
                return Port;

            int idx = PortsToReconnect.IndexOf(Port);

            int newIdx = idx + 1;
            if (newIdx >= PortsToReconnect.Count)
                newIdx = 0;

            Port = PortsToReconnect[newIdx];
            return Port;
        }

        #region OpenVpn specific configuration parameters
        public ProxyOptions OpenVpnProxyOptions { get; }
        public string       OpenVpnMultihopExitSrvId { get; }
        #endregion //OpenVpn specific configuration parameters

        #region WireGuard specific configuration parameters
        #endregion //WireGuard specific configuration parameters
    }
}

