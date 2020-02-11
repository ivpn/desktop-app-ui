using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace IVPN.VpnProtocols
{
    [DataContract]
    [Serializable]
    public abstract class VpnServerInfoBase
    {
        [DataMember]
        public string GatewayId { get; set; }
        [DataMember]
        public string CountryCode { get; set; }
        [DataMember]
        public string Country { get; set; }
        [DataMember]
        public string City { get; set; }

        public abstract List<string> GetHostsIpAddresses();
        public abstract bool IsContainHostIpAddress(string host);

        public override string ToString() { return FullName; }

        public string FullName => $"{City}, {CountryCode}"; 
    }
}
