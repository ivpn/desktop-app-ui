using IVPN.Models;

namespace IVPN.ViewModels
{
    public class ServerPingResult
    {        
        public ServerPingResult(ServerLocation server, bool isServerReachable, int pingTimeMs = 0)
        {
            Server = server;
            IsServerReachable = isServerReachable;
            PingTimeMs = pingTimeMs;            
        }

        public ServerLocation Server { get; }

        public bool IsServerReachable { get; }
        public int PingTimeMs { get; }

    }
}
