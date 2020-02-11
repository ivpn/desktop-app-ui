namespace IVPN.Models.Configuration
{
    public class ProxyOptions
    {
        public string Type { get; }
        public string Server { get; }
        public int Port { get; }
        public string Username { get; }
        public string UnsafePassword { get; }
        
        public ProxyOptions(string proxyType, string proxyServer,
            int proxyPort, string proxyUsername, string proxyUnsafePassword)
        {
            Type = proxyType;
            Server = proxyServer;
            Port = proxyPort;
            Username = proxyUsername;
            UnsafePassword = proxyUnsafePassword;
        }
    }
}
