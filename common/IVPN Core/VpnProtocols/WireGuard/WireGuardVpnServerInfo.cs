using System;
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
