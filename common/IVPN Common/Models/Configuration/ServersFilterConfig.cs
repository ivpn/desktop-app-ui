using System;
using System.Collections.Generic;
using System.Linq;
using IVPN.VpnProtocols;
using Newtonsoft.Json;

namespace IVPN.Models.Configuration
{
    /// <summary>
    /// Information about servers which can be used:
    /// - servers which can be used as FastestServer
    /// - servers (IP-s) which must be excluded from connection
    /// </summary>
    public class ServersFilterConfig
    {
        public event EventHandler OnFilterChangedFastestServer = delegate {};

        /// <summary>
        /// The fastest servers in use.
        /// List of serverID which can be user to determine fastest server
        /// </summary>
        [JsonProperty]
        private Dictionary<VpnType, HashSet<string>> __FastestServersInUse = new Dictionary<VpnType, HashSet<string>>();

        /// <summary>
        /// IPs which must be excluded from connection to server
        /// (serverID -> List of IPs)
        /// </summary>
        //[JsonProperty]
        //private Dictionary<string, HashSet<string>> __ExcludedServerIp = new Dictionary<string, HashSet<string>>();

        /// <summary>
        /// Remove references to unused(removed) servers or IPs from configuration (if necesary)
        /// </summary>
        public bool NormalizeData(IEnumerable<VpnServerInfoBase> allServers, VpnType vpnType)
        {
            if (allServers == null || !allServers.Any())
                return false;

            if (!__FastestServersInUse.Any() 
                //&& !__ExcludedServerIp.Any()
                )
                return false;

            try
            {
                bool isChanged = false;

                // list of all posible server-IDs
                HashSet<string> allServersId = new HashSet<string>(allServers.Select(s => s.GatewayId.Trim().ToLower()));

                // if fastest servers defined
                if (__FastestServersInUse.ContainsKey(vpnType)
                    && __FastestServersInUse[vpnType].Any()
                    )
                {
                    List<string> serversToRemove = new List<string>();
                    foreach (string svrId in __FastestServersInUse[vpnType])
                    {
                        if (!allServersId.Contains(svrId))
                            serversToRemove.Add(svrId);
                    }

                    isChanged = serversToRemove.Any() || isChanged;
                    foreach (string svr in serversToRemove)
                        __FastestServersInUse[vpnType].Remove(svr);
                }

                /*
                // if servers to exclude defined
                if (__ExcludedServerIp.Any())
                {
                    // list of all possible server IP-s
                    HashSet<string> allIPs = new HashSet<string>(allPossibleServers.SelectMany(s => s.GetHostsIpAddresses().Select(ip => ip.Trim().ToLower())));

                    List<string> serversToRemove = new List<string>();
                    foreach (KeyValuePair<string, HashSet<string>> svr in __ExcludedServerIp)
                    {
                        if (!allServersId.Contains(svr.Key))
                            serversToRemove.Add(svr.Key);
                        else
                        {
                            List<string> ipToRemove = new List<string>();
                            foreach (string ip in svr.Value)
                            {
                                if (!allIPs.Contains(ip))
                                    ipToRemove.Add(ip);
                            }

                            isChanged = ipToRemove.Any() || isChanged;
                            foreach (string ip in ipToRemove)
                                svr.Value.Remove(ip);
                        }
                    }

                    isChanged = serversToRemove.Any() || isChanged;
                    foreach (string svr in serversToRemove)
                        __ExcludedServerIp.Remove(svr);
                }*/

                return isChanged;
            }
            catch (Exception ex)
            {
                __FastestServersInUse.Clear();
                //__ExcludedServerIp.Clear();
                Logging.Info($"EXCEPTION. Failed to normalize ServersFilterConfig: {ex}");
            }
            return true;
        }

        #region FastestServers
        public void AddFastestServer(string serverId, VpnType vpnType, IEnumerable<string> allServers)
        {
            if (string.IsNullOrEmpty(serverId)) 
                return;

            if (__FastestServersInUse.ContainsKey(vpnType))
            {
                __FastestServersInUse[vpnType].Add(serverId.Trim().ToLower());

                // When customer changes a list of servers, it is not allowed to add additional servers / location to the list.Any new server deployed should be unchecked.
                // (this can be implemented by storing the list of servers for which the “fastest connection” is enabled.)
                // Use case example:
                //  Customer removed “Vienna, AT” from the list of fastest servers
                //      We deployed new location in “New Delhi, India”
                //      This new server should be also not included in the “Fastest servers”
                // So, if all servers selected - we can just remove all. IsFastestServerInUse - will remove true for each call in this case too.
                if (__FastestServersInUse[vpnType].SetEquals(new HashSet<string>(allServers.Select(x => x.Trim().ToLower()))))
                    __FastestServersInUse.Remove(vpnType);
            }
            else
                __FastestServersInUse.Add(vpnType, new HashSet<string> { serverId.Trim().ToLower() });

            OnFilterChangedFastestServer(this, new EventArgs());
        }

        public void RemoveFastestServer(string serverId, VpnType vpnType, IEnumerable<string> allServers)
        {
            if (string.IsNullOrEmpty(serverId)) 
                return;

            // When customer changes a list of servers, it is not allowed to add additional servers / location to the list.Any new server deployed should be unchecked.
            // (this can be implemented by storing the list of servers for which the “fastest connection” is enabled.)
            // Use case example:
            //  Customer removed “Vienna, AT” from the list of fastest servers
            //      We deployed new location in “New Delhi, India”
            //      This new server should be also not included in the “Fastest servers”
            if (__FastestServersInUse.ContainsKey(vpnType) && !__FastestServersInUse[vpnType].Any())
                __FastestServersInUse.Remove(vpnType);
            if (!__FastestServersInUse.ContainsKey(vpnType))
                __FastestServersInUse.Add(vpnType, new HashSet<string>(allServers.Select(x => x.Trim().ToLower())));

            __FastestServersInUse[vpnType].Remove(serverId.Trim().ToLower());

            OnFilterChangedFastestServer(this, new EventArgs());
        }

        public bool IsFastestServerInUse(string serverId, VpnType vpnType)
        {
            if (string.IsNullOrEmpty(serverId)) 
                return false;

            // if configuration not defined - allow all servers
            if (!__FastestServersInUse.ContainsKey(vpnType) 
                || !__FastestServersInUse[vpnType].Any())
                return true;

            return __FastestServersInUse[vpnType].Contains(serverId.Trim().ToLower());
        }
        #endregion //FastestServers

        #region Serialization
        public string Serialize()
        {
            try
            {
                return JsonConvert.SerializeObject(this);
            }
            catch
            {
                Logging.Info($"ERROR: Failed to serialize ServersFilterConfig");
                return "";
            }
        }

        public static ServersFilterConfig Deserialize(string serializedData)
        {
            if (string.IsNullOrEmpty(serializedData))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<ServersFilterConfig>(serializedData);
            }
            catch
            {
                Logging.Info("ERROR: Failed to de-serialize ServersFilterConfig");
                return null;
            }
        }
        #endregion //Serialization
    }
}
