using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using IVPN.VpnProtocols;
using IVPN.VpnProtocols.OpenVPN;
using IVPN.VpnProtocols.WireGuard;

namespace IVPN
{
    #region Requests
    [DataContract]
    public class IVPNRequest
    {
    }

    [Serializable]
    public class IVPNHelloRequest : IVPNRequest
    {
        [DataMember]
        internal string Version = "0.0";
    }

    /// <summary>
    /// Request service to remove service-crash file
    /// (Platform.ServiceCrashInfoFilePath)
    /// 
    /// Client itseld can not remove it, because file was creaded by service with admin rights
    /// </summary>
    [Serializable]
    public class IVPNRemoveServiceCrashFile : IVPNRequest {}

    /// <summary>
    /// Ping servers and return ping-time for each server.
    /// 
    /// We need to do it from privilaged mode (agent\service)
    /// because there is a problems of using 'Ping' class from user mode for macOS (Mono implementation limitation)
    /// </summary>
    [Serializable]
    public class IVPNPingServers : IVPNRequest 
    {
        /// <summary>
        /// Count tells pinger to stop after sending (and receiving) Count echo packets.
        /// </summary>
        [DataMember]
        internal int retryCount;

        /// <summary>
        /// Timeout specifies a timeout before ping exits, regardless of how many packets have been received.
        /// </summary>
        [DataMember]
        internal int timeOutMs;
    }

    [Serializable]
    public class IVPNConnectRequest : IVPNRequest
    {
        [DataMember]
        internal VpnType VpnType;

        [DataMember]
        internal WireGuardConnectionParameters WireGuardParameters;

        [DataMember]
        internal OpenVPNConnectionParameters OpenVpnParameters;
        
        [DataMember]
        internal string CurrentDns;

        public override string ToString()
        {
            return $"{base.ToString()} type={VpnType}; OpeVpnPrarams=({OpenVpnParameters}); WireGuardParams=({WireGuardParameters})";
        }
    }

    [Serializable]
    public class IVPNDisconnectRequest : IVPNRequest
    {
    }

    [Serializable]
    public class IVPNKillSwitchGetStatusRequest : IVPNRequest
    {
    }

    [Serializable]
    public class IVPNKillSwitchSetEnabledRequest : IVPNRequest
    {
        [DataMember]
        internal bool IsEnabled;
    }

    [Serializable]
    public class IVPNKillSwitchSetAllowLANRequest : IVPNRequest
    {
        [DataMember]
        internal bool AllowLAN;
    }

    [Serializable]
    public class IVPNKillSwitchSetAllowLANMulticastRequest : IVPNRequest
    {
        [DataMember]
        internal bool AllowLANMulticast;
    }

    [Serializable]
    public class IVPNKillSwitchSetIsPersistentRequest : IVPNRequest
    {
        [DataMember]
        internal bool IsPersistent;
    }

    [Serializable]
    public class IVPNKillSwitchGetIsPestistentRequest : IVPNRequest
    {
    }

    [Serializable]
    public class IVPNSecurityPolicyActionRequest : IVPNRequest
    {
    }

    [Serializable]
    public class IVPNSetPreferenceRequest : IVPNRequest
    {
        [DataMember]
        internal string Key;

        [DataMember]
        internal string Value;
    }

    [Serializable]
    public class IVPNGenerateDiagnosticsRequest : IVPNRequest
    {
        [DataMember]
        internal VpnType VpnProtocolType;
    }

    [Serializable]
    public class IVPNPauseConnection : IVPNRequest {}

    [Serializable]
    public class IVPNResumeConnection : IVPNRequest {}

    [Serializable]
    public class IVPNWireGuardCredentialsUpdate : IVPNRequest
    {
        [DataMember]
        internal WireGuardLocalCredentials WireGuardCredentials;
    }

    [Serializable]
    public class IVPNSetAlternateDns : IVPNRequest
    {

        /// <summary>
        /// if DNS == IPAddress.None (255.255.255.255) -> disable alternate DNS
        /// </summary>
        [DataMember]
        internal string DNS; 
    }
    #endregion

    #region Responses
    [DataContract]
    public class IVPNResponse
    {
    }

    [DataContract]
    public class IVPNEmptyResponse: IVPNResponse
    {
    }

    [DataContract]
    public class IVPNErrorResponse : IVPNResponse
    {
        [DataMember]
        public string ErrorMessage;
    }

    [Serializable]
    public class IVPNHelloResponse : IVPNResponse
    {
        [DataMember]
        internal string Version = "1.0";
    }

    [Serializable]
    public class IVPNServerListResponse : IVPNResponse
    {
        [DataMember]
        public VpnServersInfo VpnServers;
    }

    [Serializable]
    public class IVPNPingServersResponse : IVPNResponse
    {
        [DataMember]
        public Dictionary<string, int> pingResults;
    }

    [Serializable]
    public class IVPNStateResponse : IVPNResponse
    {
        [DataMember]
        public string State;
        [DataMember]
        public string StateAdditionalInfo;

        public override string ToString()
        {
            return$"[IVPNStateResponse state={State}{(string.IsNullOrEmpty(StateAdditionalInfo) ? "" : ":" + StateAdditionalInfo)}]";
        }
    }

    [Serializable]
    public class IVPNConnectedResponse : IVPNResponse
    {
        [DataMember]
        public ulong TimeSecFrom1970;

        [DataMember]
        public string ClientIP;

        [DataMember]
        public string ServerIP;
    }

    [Serializable]
    public class IVPNDisconnectedResponse : IVPNResponse
    {
        [DataMember]
        public bool Failure;

        [DataMember]
        public IVPNServer.DisconnectionReason Reason;

        [DataMember]
        public string ReasonDescription;
    }

    [Serializable]
    public class IVPNGetPreferencesResponse : IVPNResponse
    {
        [DataMember]
        public Dictionary<string, string> Preferences;
    }

    [Serializable]
    public class IVPNConnectToLastServerResponse : IVPNResponse
    {
    }
    
    [Serializable]
    public class IVPNServiceExitingResponse : IVPNResponse
    {
    }

    [Serializable]
    public class IVPNDiagnosticsGeneratedResponse : IVPNResponse
    {
        [DataMember]
        public string ServiceLog;
        [DataMember]
        public string ServiceLog0;

        [DataMember]
        public string OpenvpnLog;
        [DataMember]
        public string OpenvpnLog0;

        [DataMember]
        public string EnvironmentLog;
    }

    [Serializable]
    public class IVPNKillSwitchGetStatusResponse : IVPNResponse
    {
        [DataMember]
        public bool IsEnabled;
    }

    [Serializable]
    public class IVPNKillSwitchGetIsPestistentResponse: IVPNResponse
    {
        [DataMember]
        public bool IsPersistent;
    }

    [Serializable]
    public class IVPNSetAlternateDnsResponse : IVPNResponse
    {
        [DataMember]
        public bool IsSuccess;

        [DataMember]
        public string ChangedDns;
    }
    #endregion
}
