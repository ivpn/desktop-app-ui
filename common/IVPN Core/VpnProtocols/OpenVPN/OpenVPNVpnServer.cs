using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IVPN.VpnProtocols.OpenVPN
{
    [Serializable]
    public class OpenVPNVpnServer : VpnServerInfoBase
    {
        public override List<string> GetHostsIpAddresses()
        {
            return IPAddresses;
        }

        public override bool IsContainHostIpAddress(string host)
        {
            return IPAddresses.Contains(host);  
        }

        [JsonProperty("ip_addresses")]
        public List<string> IPAddresses { get; set; }
    }
}
