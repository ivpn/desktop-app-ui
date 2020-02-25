using System;

using System.Collections.Generic;
using IVPN.VpnProtocols;

namespace IVPN.Responses
{
    [Serializable]
    public class IVPNResponse {
        public string Type { get; set; }
    }

    public class IVPNEmptyResponse : IVPNResponse { }

    public class IVPNErrorResponse : IVPNResponse
    {
        public string ErrorMessage;
    }

    public class IVPNHelloResponse : IVPNResponse
    {
        public string Version;
    }

    public class IVPNServerListResponse : IVPNResponse
    {
        public VpnServersInfo VpnServers;
    }

    public class IVPNPingServersResponse : IVPNResponse
    {
        public class PingResult
        {
            public string Host { get; set; }
            public int Ping { get; set; }
        }
        public List<PingResult> PingResults { get; set; }
    }

    public class IVPNVpnStateResponse : IVPNResponse
    {
        public string State;
        public string StateAdditionalInfo;

        public override string ToString()
        {
            return $"[IVPNStateResponse state={State}{(string.IsNullOrEmpty(StateAdditionalInfo) ? "" : ":" + StateAdditionalInfo)}]";
        }
    }

    public class IVPNConnectedResponse : IVPNResponse
    {
        public ulong TimeSecFrom1970;
        public string ClientIP;
        public string ServerIP;
        public VpnType VpnType;
    }

    public class IVPNDisconnectedResponse : IVPNResponse
    {
        public bool Failure;
        public DisconnectionReason Reason;
        public string ReasonDescription;        
    }

    public class IVPNServiceExitingResponse : IVPNResponse { }

    public class IVPNDiagnosticsGeneratedResponse : IVPNResponse
    {
        public string ServiceLog;
        public string ServiceLog0;

        public string OpenvpnLog;
        public string OpenvpnLog0;

        public string EnvironmentLog;
    }

    public class IVPNKillSwitchStatusResponse : IVPNResponse
    {
        public bool IsEnabled;
        public bool IsPersistent;
        public bool IsAllowLAN;
        public bool IsAllowMulticast;
    }

    public class IVPNKillSwitchGetIsPestistentResponse : IVPNResponse
    {
        public bool IsPersistent;
    }

    public class IVPNSetAlternateDnsResponse : IVPNResponse
    {
        public bool     IsSuccess;
        public string   ChangedDNS;
    }
}
