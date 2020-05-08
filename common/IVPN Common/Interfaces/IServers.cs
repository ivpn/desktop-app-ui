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

﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using IVPN.Models;
using IVPN.VpnProtocols;

namespace IVPNCommon.Interfaces
{
    public delegate void OnPingUpdatedDelegate();
    public delegate void OnPingUpdateRequestDelegate(int pingTimeOutMs, int pingRetriesCount);
    public delegate void OnFasterServerDetectedDelegate(ServerLocation location);

    public enum DnsTypeEnum
    {
        AntiTracker,
        AntiTrackerHardcore
    }

    public interface IServers: INotifyPropertyChanged
    {
        /// <summary> Occurs when servers ping times are updated. </summary>
        event OnPingUpdatedDelegate OnPingsUpdated; // in use by macOS implementation
        /// <summary> Occurs when faster server detected </summary>
        event OnFasterServerDetectedDelegate OnFasterServerDetected;

        // Notify that new ping-times are required (must be started pings-update immediately)
        event OnPingUpdateRequestDelegate OnPingUpdateRequired;

        /// <summary> Servers for current protocol </summary>
        List<ServerLocation> ServersList { get; }

        /// <summary> Set new servers list </summary>
        void UpdateServers(VpnServersInfo vpnServers);

        /// <summary> Set servers pings </summary>
        void UpdateServersPings(Dictionary<string, int> serversPings);

        /// <summary>
        /// Try to find fastest server.
        /// If servers ping is running - wait for completed. If Pings not defined - start ping process and wait to finish
        /// </summary>
        Task<ServerLocation> GetFastestServerAsync();

        /// <summary>
        /// Try to find fastest server.
        /// Use current data of pings-time. (Does not wait servers to be pinged ... etc.)
        /// </summary>
        ServerLocation GetFastestServer();

        /// <summary>
        /// Start task to ping all servers in background
        /// </summary>
        /// <returns>
        /// true : ping update requested (SUCCESS).
        /// false:
        ///     1) when ping update not started due to it was performed few seconds ago (in this case, you can use 'GetFastestServer()')
        ///     2) nothing to ping
        /// </returns>
        bool StartPingUpdate();

        ServerLocation GetServerByIP(IPAddress addr, VpnType vpnType);
        ServerLocation GetServerByOpenVpnMultihopID(string multihopId);

        IPAddress GetDnsIp(DnsTypeEnum dnsType, bool isMultihop);
        bool IsAntitracker(IPAddress dnsIP, out bool isHardcore, out bool isMultihop);
    }
}
