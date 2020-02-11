using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using IVPN.VpnProtocols.OpenVPN;
using IVPN.VpnProtocols.WireGuard;

namespace IVPN.VpnProtocols
{
    [DataContract]
    [Serializable]
    public class VpnServersInfo
    {
        [DataMember]
        public List<WireGuardVpnServerInfo> WireGuardServers { get; set; } = new List<WireGuardVpnServerInfo>();
        [DataMember]
        public List<OpenVPNVpnServer> OpenVPNServers { get; set; } = new List<OpenVPNVpnServer>();

        [DataMember]
        public RESTApi.RestRequestGetServers.ServersInfoResponse.ConfigInfoResponse Config { get; set; }

        public bool IsAnyServers()
        {
            return (WireGuardServers != null && WireGuardServers.Any()) || (OpenVPNServers != null && OpenVPNServers.Any());
        }
    }
}
