using System.Net;

namespace NetworkHelpers
{
    public class RoutingTableEntry
    {
        public IPAddress destination { get; set; }
        public IPAddress subnetMask { get; set; }
        public IPAddress nextHop { get; set; }
        public uint interfaceIndex { get; set; }
        public uint type { get; set; }
        public uint proto { get; set; }
        public uint age { get; set; }
        public uint metric { get; set; }
    }
}
