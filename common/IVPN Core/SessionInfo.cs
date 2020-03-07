using System;
using System.Net;

namespace IVPN
{
    public class SessionInfo
    {
        public string AccountID { get; }
        public string Session { get; }

        public string WgPublicKey { get; }
        public IPAddress WgLocalIP { get; }
        public DateTime WgKeyGenerated { get; }
        public TimeSpan WgKeyRotateInterval { get; }

        public SessionInfo(
            string accountID,
            string session,
            string wgPublicKey,
            IPAddress wgLocalIP,
            DateTime wgKeyGenerated,
            TimeSpan wgKeyRotateInterval)
        {
            AccountID = accountID;
            Session = session;
            WgPublicKey = wgPublicKey;
            WgLocalIP = wgLocalIP;
            WgKeyGenerated = wgKeyGenerated;
            WgKeyRotateInterval = wgKeyRotateInterval;
        }

        public bool IsWireGuardKeysInitialized()
        {
            return !string.IsNullOrEmpty(WgPublicKey);
        }

        public DateTime GetKeysExpiryDate()
        {
            return WgKeyGenerated.Add(WgKeyRotateInterval);
        }
    }
}
