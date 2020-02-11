namespace IVPN.WiFi
{
    public class WiFiNetworkInfo
    {
        public WiFiNetworkInfo(WiFiNetwork network)
        {
            Network = network;
        }

        public WiFiNetwork Network { get; }
        
        // here can be additional information. e.g. signal quality... etc.
    }
}
