using System;
using IVPN.VpnProtocols;
using IVPN.VpnProtocols.OpenVPN;
using IVPN.VpnProtocols.WireGuard;
    
namespace IVPN.Requests
{
    [Serializable]
    public abstract class Request
    {
        public string Command => GetType().Name;
        public long Idx { get; set; }
    }

    public class Hello : Request
    {
        public string Version;
        public UInt64 Secret;

        // GetServersList == true - client requests to send back info about all servers
        public bool GetServersList;

        // GetStatus == true - client requests current status (Vpn connection, Firewal... etc.)
        public bool GetStatus;

        // GetConfigParams == true - client requests config parameters (user-defined OpevVPN file location ... etc.)
        public bool GetConfigParams;

        // KeepDaemonAlone informs daemon\service to do nothing when client disconnects
        // 		false (default) - VPN disconnects when client disconnects from a daemon
        // 		true - do nothing when client disconnects from a daemon (if VPN is connected - do not disconnect)
        public bool KeepDaemonAlone;

        // Register credentials (if not logged in)
        // Used when updating from an old client version
        public RawCredentials SetRawCredentials;
    }

    // RawCredentials - RAW credentials
    public class RawCredentials {
        public string AccountID;
        public string Session;
        public string OvpnUser;
        public string OvpnPass;
        public string WgPublicKey;
        public string WgPrivateKey;
        public string WgLocalIP;
        public Int64 WgKeyGenerated;
    }

    /// <summary>
    /// Ping servers and return ping-time for each server.
    /// 
    /// We need to do it from privilaged mode (agent\service)
    /// because there is a problems of using 'Ping' class from user mode for macOS (Mono implementation limitation)
    /// </summary>
    public class PingServers : Request 
    {
        /// <summary>
        /// Count tells pinger to stop after sending (and receiving) Count echo packets.
        /// </summary>
        public int RetryCount;

        /// <summary>
        /// Timeout specifies a timeout before ping exits, regardless of how many packets have been received.
        /// </summary>
        public int TimeOutMs;
    }

    public class Connect : Request
    {
        public VpnType VpnType; 

        public WireGuardConnectionParameters WireGuardParameters;

        public OpenVPNConnectionParameters OpenVpnParameters;
        
        public string CurrentDNS;

        public override string ToString()
        {
            return $"{base.ToString()} type={VpnType}; OpeVpnPrarams=({OpenVpnParameters}); WireGuardParams=({WireGuardParameters})";
        }
    }

    public class Disconnect : Request    {}

    public class KillSwitchGetStatus : Request    {}

    public class KillSwitchSetEnabled : Request
    {
        public bool IsEnabled;
    }

    public class KillSwitchSetAllowLAN : Request
    {
        public bool AllowLAN;
    }

    public class KillSwitchSetAllowLANMulticast : Request
    {
        public bool AllowLANMulticast;
    }

    public class KillSwitchSetIsPersistent : Request
    {
        public bool IsPersistent;
    }

    public class KillSwitchGetIsPestistent : Request    {}

    public class SetPreference : Request
    {
        public string Key;

        public string Value;
    }

    public class GenerateDiagnostics : Request
    {
        public VpnType VpnProtocolType;
    }

    public class PauseConnection : Request {}

    public class ResumeConnection : Request {}

    public class SetAlternateDns : Request
    {
        /// <summary>
        /// if DNS == IPAddress.None (255.255.255.255) -> disable alternate DNS
        /// </summary>
        public string DNS; 
    }

    public class SessionNew : Request
    {
        public string AccountID;
        public bool ForceLogin;
    }

    public class AccountStatus : Request { }

    public class SessionDelete : Request { }

    public class WireGuardGenerateNewKeys : Request
    {
        public bool OnlyUpdateIfNecessary;
    }

    public class WireGuardSetKeysRotationInterval : Request
    {
        public Int64 Interval; // seconds
    }
}
