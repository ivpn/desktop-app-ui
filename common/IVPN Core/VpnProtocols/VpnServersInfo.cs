using System;
using System.Collections.Generic;
using System.Linq;
using IVPN.VpnProtocols.OpenVPN;
using IVPN.VpnProtocols.WireGuard;
using Newtonsoft.Json;

namespace IVPN.VpnProtocols
{
    [Serializable]
    public class VpnServersInfo
    {
        [JsonProperty("wireguard")]
        public List<WireGuardVpnServerInfo> WireGuardServers { get; set; } = new List<WireGuardVpnServerInfo>();
        [JsonProperty("openvpn")]
        public List<OpenVPNVpnServer> OpenVPNServers { get; set; } = new List<OpenVPNVpnServer>();

        [JsonProperty("config")]
        public RESTApi.RestRequestGetServers.ServersInfoResponse.ConfigInfoResponse Config { get; set; }

        public bool IsAnyServers()
        {
            return (WireGuardServers != null && WireGuardServers.Any()) || (OpenVPNServers != null && OpenVPNServers.Any());
        }
    }
}
