using System.Collections.Generic;

namespace IVPN
{
    public class Constants
    {
        /// <summary>
        /// When long time no response from server during connection - watchdog can cancel current connection process
        /// and try to connect to another port.
        /// 
        /// Note:
        ///     Ideally, it should be bigger than 'hand-window XXX' seconds from openvpn configuration.
        /// </summary>
        public const int ConnectionWatchDogTimerTimeoutMs = 7000; 

        public static Dictionary<string, string> STATE_DESCRIPTIONS = new Dictionary<string, string> {
            {"WAIT", "Waiting for server..."},
            {"RESOLVE", "Looking up hostname..."},
            {"AUTH", "Authenticating..."},
            {"GETCONFIG", "Obtaining configuration..."},
            {"ASSIGNIP", "Assigning IP address..."},
            {"ADDROUTES", "Adding routes..."},
            {"RECONNECTING", "Reconnecting..."},
            {"DISCONNECTED", "Disconnected"} // XXX
        };


        #region Experiments
        public static string CAPABILITIES_PRIVATE_EMAILS = @"private-emails";
        public static string CAPABILITIES_MULTIHOP = @"multihop";
        #endregion

        public static string GetRenewUrl (string username)
        {
            return @"https://www.ivpn.net/clientarea/renew/" + username + "?client=" + Platform.ShortPlatformName;
        }

        public static string GetSignUpUrl ()
        {
            return @"https://ivpn.net/signup?os=" + Platform.ShortPlatformName;
        }

        public static string IVPNHelpUrl => @"https://www.ivpn.net/knowledgebase";
    }
}
