using System;
using System.Runtime.CompilerServices;
using IVPN.Models.Configuration;

namespace IVPN.Interfaces
{
    public interface ISettingsProvider
    {
        void Save(AppSettings settings);

        void Load(AppSettings settings);

        // READ OLD-STYLE CREDENTIALS (compatibility with older client versions) 
        bool GetOldStyleCredentials(
            out string AccountID,
            out string Session,
            out string OvpnUser,
            out string OvpnPass,
            out string WgPublicKey,
            out string WgPrivateKey,
            out string WgLocalIP,
            out Int64 WgKeyGenerated);

        /// <summary>
        /// Reset to defaults
        /// </summary>
        void Reset(AppSettings settings);

        /// <summary>
        /// Some settings should be saved immediately.
        /// For example, "RunOnLogin" for macOS implementation
        /// </summary>
        void OnSettingsChanged(AppSettings settings, [CallerMemberName] string propertyName = "");
    }
}
