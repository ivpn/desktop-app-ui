using System;
using System.Collections.Generic;
using System.Net;
using IVPN.Models;
using IVPN.Models.Configuration;
using IVPN.VpnProtocols;

namespace IVPN
{
    public class ConnectionTarget
    {
        private ConnectionTarget(ServerLocation server, DestinationPort port, List<DestinationPort> portsToReconnect, IPAddress currentManualDns)
        {
            Server = server;
            Port = port;
            CurrentManualDns = currentManualDns;

            if (portsToReconnect != null && portsToReconnect.Count > 0)
            {
                if (!portsToReconnect.Contains(port))
                    throw new ArgumentOutOfRangeException(nameof(portsToReconnect), "Preffered ports list does not caontain Port which is defined as first");

                PortsToReconnect.AddRange(portsToReconnect);
            }
        }

        public ConnectionTarget(ServerLocation server, DestinationPort port, List<DestinationPort> portsToReconnect, IPAddress currentManualDns, string username, string password, ProxyOptions proxyOptions, string wireguardInternalClientIp, string wireguardPrivateKey)
            : this(server, port, portsToReconnect, currentManualDns)
        {
            // TODO: necessary to think how to divide implementation for OpenVPN and Wireguard 

            OpenVpnUsername = username;
            OpenVpnPassword = password;
            OpenVpnProxyOptions = proxyOptions;
            WireGuardInternalClientIp = wireguardInternalClientIp;
            WireGuardLocalPrivateKey = wireguardPrivateKey;
        }

        public ServerLocation Server { get; }

        public DestinationPort Port { get; private set; }

        public List<DestinationPort> PortsToReconnect { get; } = new List<DestinationPort>();

        public IPAddress CurrentManualDns { get; }

        public DestinationPort ChangeToNextPort()
        {
            if (PortsToReconnect.Count <= 0)
                return Port;

            int idx = PortsToReconnect.IndexOf(Port);

            int newIdx = idx + 1;
            if (newIdx >= PortsToReconnect.Count)
                newIdx = 0;

            Port = PortsToReconnect[newIdx];
            return Port;
        }

        #region OpenVpn specific configuration parameters
        public string OpenVpnUsername { get; }

        public string OpenVpnPassword { get; }

        public ProxyOptions OpenVpnProxyOptions { get; }
        #endregion //OpenVpn specific configuration parameters

        #region WireGuard specific configuration parameters
        public string WireGuardLocalPrivateKey { get; }

        public string WireGuardInternalClientIp { get; }
        #endregion //WireGuard specific configuration parameters
    }
}

