using System;
using System.Runtime.Serialization;

namespace IVPN.VpnProtocols.WireGuard
{
    [DataContract]
    [Serializable]
    public class WireGuardConnectionParameters
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
