using System;
using System.Drawing;
using IVPN.Properties;

namespace IVPN.AutoUpdate
{
    /// <summary>
    /// Application updater
    /// Wrapper for AppUpdater
    /// </summary>
    public class Updater
    {
        public static void Initialize()
        {
            try
            {
                // trying to pass application icon with resolution 64*64 
                // By default - loaded a icon 32*32
                AppUpdater.Gui.GuiController.SetApplicationIcon(new Icon(Resources.application, new Size(64, 64)));

                // Initialize signature check functionality
                AppUpdater.Updater.SetSignatureCheckParameters(Platform.OpenSSLExecutablePath, Resources.dsa_pub);
                // Initialize updater (start background thread)
                AppUpdater.Updater.Initialize(
                    "https://www.ivpn.net/releases/win/AppUpdater/ivpn_win_appcast.xml",
                    "https://www.ivpn.net/releases/win/AppUpdater/ivpn_win_appcast_manualupdate.xml",
                    60 * 60 * 12);
            }
            catch (Exception)
            {
                // ignore all errors
            }

        }

        /// <summary>
        /// Check for an update
        /// </summary>
        public static void CheckForUpdatesWithUi()
        {
            AppUpdater.Updater.CheckForUpdateAsync();
        }
    }
}
