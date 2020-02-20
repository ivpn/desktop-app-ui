using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IVPN.VpnProtocols
{
    [Serializable]
    public abstract class VpnServerInfoBase
    {
        [JsonProperty("gateway")]
        public string GatewayId { get; set; }
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }

        public abstract List<string> GetHostsIpAddresses();
        public abstract bool IsContainHostIpAddress(string host);

        public override string ToString() { return FullName; }

        public string FullName => $"{City}, {CountryCode}"; 
    }
}
