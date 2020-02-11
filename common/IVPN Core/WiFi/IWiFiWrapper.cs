using System.Collections.Generic;

namespace IVPN.WiFi
{
    public delegate void WiFiNetworkConnected(WifiState state);

    public delegate void OnNetworksScanCompleteDelegate();

    public interface IWiFiWrapper
    {
        event WiFiNetworkConnected WiFiStateChanged;
        
        WifiState CurrentState
        {
            get;
        }

        void Initialize();
        void Shutdown();

        event OnNetworksScanCompleteDelegate OnNetworksScanComplete;

        IEnumerable<WiFiNetworkInfo> GetWifiNetworks();
        void StartWiFiNetworksScan();
    }
    
    public class WifiState
    {
        public WifiState(WiFiNetwork network, bool connectedToInsecureNetwork)
        {
            Network = network;
            ConnectedToInsecureNetwork = connectedToInsecureNetwork;
        }

        public bool ConnectedToInsecureNetwork { get; }
        public WiFiNetwork Network { get; }
    }
}
