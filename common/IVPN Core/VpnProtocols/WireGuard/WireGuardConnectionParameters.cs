using System;
using System.Runtime.Serialization;

namespace IVPN.VpnProtocols.WireGuard
{
    [DataContract]
    [Serializable]
    public class WireGuardLocalCredentials
    {
        [DataMember]
        internal string InternalClientIP;

        [DataMember]
        internal string LocalPrivateKey;
    }

    [DataContract]
    [Serializable]
    public class WireGuardConnectionParameters : WireGuardLocalCredentials
    {
        [DataMember]
        internal WireGuardVpnServerInfo EntryVpnServer;

        [DataMember]
        internal DestinationPort Port;

        public override string ToString()
        {
            return $"Port={Port} EntryServer=[{EntryVpnServer}]";
        }
    }
}
