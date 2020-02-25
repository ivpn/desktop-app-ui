using System;
using System.IO;
using Foundation;
using Security;

using IVPN;

namespace MacLib
{

    public delegate void LaunchAgentStartedDelegate(int port, UInt64 secret);

    public class PrivilegeHelper
    {
        private const string HELPER_LABEL = "net.ivpn.client.Helper";

        public PrivilegeHelper()
        {

        }

        public static bool IsHelperInstalled()
        {
            NSDictionary dict = MacHelpers.SMJobDictionary(HELPER_LABEL);
            if (dict == null)
                return false;

            return true;
        }

        public static bool IsHelperUpgradeRequired()
        {
            if (!IsHelperInstalled()) {
                return false;
            }

            string installedHelperVersion = GetInstalledVersion();
            string currentHelperVersion = GetCurrentVersion();

            if (installedHelperVersion != currentHelperVersion)
                return true;

            return false;
        }

        public static bool InstallHelper() 
        {
            Logging.Info("Installing helper...");

            bool result = MacHelpers.LibInstallHelper(HELPER_LABEL);
            LogHelperInstallationResult(result);
            return result;
        }

        public static bool InstallHelper(Authorization authorization)
        {
            Logging.Info("Installing helper with authorization...");

            bool result = MacHelpers.LibInstallHelperWithAuthorization(HELPER_LABEL, authorization.Handle);
            LogHelperInstallationResult(result);
            return result;
        }

        private static void LogHelperInstallationResult(bool result)
        {
            if (!result) {
                Logging.Info("Helper installation failed.");
                return;
            }

            Logging.Info("Helper installed successfuly");
        }

        public static bool Uninstall(Authorization auth)
        {
            string pListFile = Path.Combine("/Library/LaunchDaemons", HELPER_LABEL + ".plist");
            string privilegeHelper = Path.Combine("/Library/PrivilegedHelperTools", HELPER_LABEL);
         
            Logging.Info("Uninstalling old helper...");

            if (!MacHelpers.LibUninstallHelperWithAuthorization(HELPER_LABEL, auth.Handle)) {
                Logging.Info("Cannot remove helper");
                return false;
            }

            if (!MacHelpers.RemoveFile(auth, pListFile))
            {
                Logging.Info(String.Format("Cannot remove file: {0} ", pListFile));
                return false;
            }

            if (!MacHelpers.RemoveFile(auth, privilegeHelper)) {
                Logging.Info(String.Format("Cannot remove file: {0} ", privilegeHelper));
                return false;
            }

            return true;
        }

        public static bool Upgrade(Authorization auth, AuthorizationFlags flags)
        {
            Logging.Info("Upgrading old helper with the new one...");

            // Hack to force "authentication required" window to pop-up;
            auth.ExecuteWithPrivileges("/bin/echo", flags, new string[] { });

            if (!Uninstall(auth)) {
                Logging.Info("Uninstallation seems not complete successfuly.");
            }

            return InstallHelper(auth);
        }

        /// <summary>
        /// Reference to delegate to avoid processing it by GarbageCollector
        /// (we SHOUL NOT pass unnamed action to native code!)
        /// </summary>
        private static Action<int, UInt64> __AgentStartedDelegate;

        public static void StartAndConnectToLaunchAgent(Action<int, UInt64> cb) 
        {
            // save reference to delegate to avoid processing it by GarbageCollector
            // (we SHOUL NOT pass unnamed action to native code!)
            __AgentStartedDelegate = cb;

            MacHelpers.LibLaunchAgent(HELPER_LABEL, (int port, UInt64 secret) => 
            {
                Logging.Info("GOT BACK PORT => " + port);
                __AgentStartedDelegate(port, secret);
            });
        }

        private static string GetInstalledVersion()
        {
            NSDictionary dict = MacHelpers.SMJobDictionary(HELPER_LABEL);
            if (dict == null)
                return null;

            var obj = dict.ValueForKey(new NSString("ProgramArguments"));
            NSArray programArguments = (NSArray)obj;
            var programPath = programArguments.GetItem<NSString>(0).ToString();

            return MacHelpers.GetBundleVersion(programPath);
        }

        private static string GetCurrentVersion()
        {
            var bundlePath = 
                System.IO.Path.Combine(
                    NSBundle.MainBundle.BundlePath, 
                    "Contents/Library/LaunchServices", 
                    HELPER_LABEL);
                                
            return MacHelpers.GetBundleVersion(bundlePath);
        }
            
    }
}

