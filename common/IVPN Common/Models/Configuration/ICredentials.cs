namespace IVPN.Models.Configuration
{
    public delegate void CredentialsChanged(ICredentials sender);

    public interface ICredentials
    {
        event CredentialsChanged OnCredentialsChanged;

        #region Getters
        string GetSessionToken();
        string GetVpnUser();
        string GetVpnPassword();
                
        /// <summary>
        /// TRUE if user\pass OR user\session\vpnUser\vpnPass available
        ///
        /// puserPass will be not in use for future releases (sessions will be used instead)
        /// </summary>
        bool IsUserLoggedIn();

        /// <summary>
        /// user\session\vpnUser\vpnPass available
        /// </summary>
        bool IsSessionAvailable();

        bool GetVpnCredentials(out string vpnUser, out string vpnPass);
        #endregion // Getters
        
        /// <summary> Returns TRUE in case if settings must to me saved </summary>
        bool SetSession(string username, string sessionToken, string vpnUser, string vpnPass, bool isPassEncrypded);
        bool DeleteSession();

        void Save();
    }
}
