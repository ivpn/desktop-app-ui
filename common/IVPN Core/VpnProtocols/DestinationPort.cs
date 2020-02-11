using System;
using System.Runtime.Serialization;

namespace IVPN.VpnProtocols
{
    [DataContract]
    [Serializable]
    public class DestinationPort
    {
        public enum ProtocolEnum
        {
            UDP,
            TCP
        };

        public DestinationPort()
        { }

        public DestinationPort(int port, ProtocolEnum protocolEnum)
        {
            Port = port;
            Protocol = protocolEnum;
        }

        [DataMember]
        public ProtocolEnum Protocol { get; set; }

        [DataMember]
        public int Port { get; set; }

        public override string ToString() { return $"{Protocol} {Port}"; }

        public override int GetHashCode() { return 0; }

        public override bool Equals(object obj)
        {
            if (!(obj is DestinationPort))
                return base.Equals(obj);

            DestinationPort objPort = (DestinationPort)obj;
            if (objPort.Port == Port && objPort.Protocol == Protocol)
                return true;

            return false;
        }
    }
}
