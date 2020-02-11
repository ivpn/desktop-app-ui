using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace IVPN.VpnProtocols.WireGuard
{
    [DataContract]
    [Serializable]
    public class WireGuardVpnServerInfo : VpnServerInfoBase
    {
        [DataContract]
        [Serializable]
        public class HostInfo
        {
            [DataMember]
            public string Host { get; set; }
            [DataMember]
            public string PublicKey { get; set; }
            [DataMember]
            public string LocalIp { get; set; }

            public override string ToString() { return $"host={Host}; PublicKey={PublicKey}; localIp={LocalIp}"; }
        }

        public override List<string> GetHostsIpAddresses()
        {
            return new List<string>(Hosts.Select(x => x.Host));
        }

        public override bool IsContainHostIpAddress(string host)
        {
            return Hosts.Any(x => x.Host.Equals(host));
        }

        [DataMember]
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
