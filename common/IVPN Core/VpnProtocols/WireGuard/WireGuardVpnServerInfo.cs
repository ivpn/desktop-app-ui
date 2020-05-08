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
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace IVPN.VpnProtocols.WireGuard
{
    [Serializable]
    public class WireGuardVpnServerInfo : VpnServerInfoBase
    {
        [Serializable]
        public class HostInfo
        {
            [JsonProperty("host")]
            public string Host { get; set; }
            [JsonProperty("public_key")]
            public string PublicKey { get; set; }
            [JsonProperty("local_ip")]
            public string LocalIP { get; set; }

            public override string ToString() { return $"host={Host}; PublicKey={PublicKey}; localIp={LocalIP}"; }
        }

        public override List<string> GetHostsIpAddresses()
        {
            return new List<string>(Hosts.Select(x => x.Host));
        }

        public override bool IsContainHostIpAddress(string host)
        {
            return Hosts.Any(x => x.Host.Equals(host));
        }

        [JsonProperty("hosts")]
        public List<HostInfo> Hosts { get; set; }

        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();

            ret.AppendLine($"Hosts({Hosts.Count})=[");
            foreach(var host in Hosts)
            {
                ret.AppendLine($"{host}");
            }
            ret.AppendLine("]");
            return ret.ToString();
        }
    }
}
