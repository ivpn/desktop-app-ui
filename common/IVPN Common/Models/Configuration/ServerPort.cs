using IVPN.VpnProtocols;

namespace IVPN.Models.Configuration
{
    public class ServerPort: ModelBase
    {
        private DestinationPort __DestinationPort;
        private bool __IsEnabled;

        public ServerPort(DestinationPort destinationPort)
        {
            __DestinationPort = destinationPort;
            __IsEnabled = true;
        }

        public DestinationPort DestinationPort
        {
            get => __DestinationPort;
            set => __DestinationPort = value;
        }

        public DestinationPort.ProtocolEnum Protocol => __DestinationPort.Protocol;

        public override string ToString()
        {
            return __DestinationPort.ToString();
        }

        public int Port => __DestinationPort.Port;

        public bool IsEnabled
        {
            get => __IsEnabled;

            set
            {
                __IsEnabled = value;
                DoPropertyChanged();
            }
        }
    }
}
