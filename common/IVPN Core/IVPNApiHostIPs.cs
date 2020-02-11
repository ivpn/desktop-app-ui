using System.Collections.Generic;
using System.Net;

namespace IVPN
{
    /// <summary>
    /// DNS-BE-100 Access to our API server have to be available using multiple IP addresses
    /// DNS-APP-100 Every call to IVPN API have to be done using DNS name first
    /// DNS-APP-200 If that call fails with connection error / timeout / TLS certificate error, connection with IP addresses have to be initiated
    /// DNS-APP-300 If the call fails with error result from the API, no retry have to be done
    /// DNS-APP-400 Calls to API server have to be retries using every IP address provided in the configuration section of /servers.json file.
    /// DNS-APP-500 IVPN client app should store the IP address of the last successful API request and use it in any subsequent calls for the first try
    /// </summary>
    public class IVPNApiHostIPs
    {
        // Thread-safe locker
        private readonly object __Locker = new object();

        // Alternate hostIPs (in use if DNS access is blocked)
        private readonly List<IPAddress> __AlternateHostIPs = new List<IPAddress>();
        private IPAddress __CurrentAlternateHostIP;

        public delegate void AlternateHostChangedDelegate(IPAddress ip);
        public event AlternateHostChangedDelegate AlternateHostChanged = delegate { };

        public delegate void AlternateHostsListUpdatedDelegate();
        public event AlternateHostsListUpdatedDelegate AlternateHostsListUpdated = delegate { };

        public void SetAlternateHostIPs(List<IPAddress> ips)
        {
            lock (__Locker)
            {
                __AlternateHostIPs.Clear();
                if (ips != null)
                {
                    __AlternateHostIPs.AddRange(ips);
                    AlternateHostsListUpdated();
                }

                if (!__AlternateHostIPs.Contains(__CurrentAlternateHostIP))
                    SetCurAlternateHost (null);
            }
        }

        public void SetAlternateHostIPs(List<string> ipList)
        {
            List<IPAddress> ips = new List<IPAddress>();
            if (ipList != null)
            {
                foreach (string ipstr in ipList)
                {
                    if (IPAddress.TryParse(ipstr, out var addr))
                        ips.Add(addr);
                }
            }

            SetAlternateHostIPs(ips);
        }

        public void SetCurAlternateHost(IPAddress ip)
        {
            lock (__Locker)
            {
                if (ip == null || __AlternateHostIPs.Count == 0 || __AlternateHostIPs.Contains(ip))
                {
                    if (!Equals(__CurrentAlternateHostIP, ip))
                    {
                        __CurrentAlternateHostIP = ip;
                        AlternateHostChanged(__CurrentAlternateHostIP);
                    }
                }
            }
        }

        public void GetHostsInfo(out IPAddress currentHostIP, out List<IPAddress> hostsIPs)
        {
            lock (__Locker)
            {
                currentHostIP = __CurrentAlternateHostIP;
                hostsIPs = new List<IPAddress>(__AlternateHostIPs);
            }
        }
    }
}