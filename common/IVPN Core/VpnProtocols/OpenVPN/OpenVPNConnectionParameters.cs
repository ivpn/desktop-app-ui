using System;
using System.Runtime.Serialization;

namespace IVPN.VpnProtocols.OpenVPN
{
    [DataContract]
    [Serializable]
    public class OpenVPNConnectionParameters
    {
        [DataMember]
        internal OpenVPNVpnServer EntryVpnServer;

        [DataMember]
        internal string Username;

        [DataMember]
        internal string Password;

        [DataMember]
        internal DestinationPort Port;

        [DataMember]
        internal string ProxyType;

        [DataMember]
        internal string ProxyAddress;

        [DataMember]
        internal int ProxyPort;

        [DataMember]
        internal string ProxyUsername;

        [DataMember]
        internal string ProxyPassword;
    }
}
