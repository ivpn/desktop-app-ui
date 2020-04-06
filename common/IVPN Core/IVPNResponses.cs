using System;

using System.Collections.Generic;
using IVPN.VpnProtocols;

namespace IVPN.Responses
{
    [Serializable]
    public class IVPNResponse {
        public string Command { get; set; }
        public long Idx { get; set; }
    }

    public class IVPNEmptyResponse : IVPNResponse { }

    public class IVPNErrorResponse : IVPNResponse
    {
        public string ErrorMessage;
    }

    public class IVPNHelloResponse : IVPNResponse
    {
        public string Version;
        public SessionInfo Session;
    }

    public class SessionInfo
    {
        public string AccountID;
        public string Session;
        public string WgPublicKey;
        public string WgLocalIP;
        public Int64 WgKeyGenerated;       // Unix time
        public Int64 WgKeysRegenInerval;   // seconds
    }

    public class AccountInfo
    {
        public bool Active;
        public Int64 ActiveUntil;
        public string CurrentPlan;
        public string PaymentMethod;
        public bool IsRenewable;
        public bool WillAutoRebill;
        public bool IsFreeTrial;
        public string[] Capabilities;
        public bool Upgradable;
        public string UpgradeToPlan;
        public string UpgradeToURL;
        public int Limit;
    }

    public class SessionNewResponse : IVPNResponse
    {
        public int APIStatus;
        public string APIErrorMessage;
        public SessionInfo Session;
        public AccountInfo Account;
    }

    public class SessionStatusResponse : IVPNResponse
    {
        public int APIStatus;
        public string APIErrorMessage;
        public SessionInfo Session;
        public AccountInfo Account;
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
