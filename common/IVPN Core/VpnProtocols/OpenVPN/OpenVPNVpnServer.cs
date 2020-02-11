using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace IVPN.VpnProtocols.OpenVPN
{
    [DataContract]
    [Serializable]
    public class OpenVPNVpnServer : VpnServerInfoBase
    {
        public override List<string> GetHostsIpAddresses()
        {
            return IpAddresses;
        }

        public override bool IsContainHostIpAddress(string host)
        {
            return IpAddresses.Contains(host);
        }

        [DataMember]
        public List<string> IpAddresses { get; set; }
    }
}
