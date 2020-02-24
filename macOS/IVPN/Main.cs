using System;
using System.IO;

using Foundation;
using AppKit;

using Security;
using System.Reflection;
using System.Threading;
using MacLib;

namespace IVPN
{
    class MainClass
    {
        static void Main(string[] args)
        {
            NSApplication.Init();

            // set app version (globally) immadiately after start
            Platform.Version = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion").ToString();

            AppDomain.CurrentDomain.UnhandledException += (sender, e) => 
            {                
                Logging.Info(e.ExceptionObject.ToString());
                ExceptionWindowController.Show (e.ExceptionObject as Exception, false, isModal: true);
            };

//            Logging.SetLogFile("/tmp/ivpn.log", true);
//            Logging.Enable();
//            Logging.OmitDate = true;
//
//            Logging.Info("Application starting...");

#if !DEBUG
            if (!FullUpgradeIfRequired())
                return;
#else
            if (!PartialUpgrade())
                return;
#endif

            try
            {
                // Register MacOS-specified detector of PowerMode change
                // Do not use 'Microsoft.Win32.SystemEvents.PowerModeChanged' event directly in common code
                // Instead, use: 'Platform.PowerModeChanged' 
                MacPowerChangeDetectorStatic.Initialize();
                MacPowerChangeDetectorStatic.PowerModeChanged += (object sender, Microsoft.Win32.PowerModeChangedEventArgs e) => { Platform.NotifyPowerModeChanged(sender, e); };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error initialising MacPowerChangeDetector: {ex}");
            }

            if (NSBundle.MainBundle.BundlePath == null)
                throw new Exception("Internal exception: NSBundle.MainBundle.BundlePath is null.");

            Platform.InstallationDirectory = @"/Applications/IVPN.app";

            NSApplication.Main(args);
        }

        private static bool WaitUntilAnotherVersionIsClosed()
        {
            NSRunningApplication runingApplication;

            while (MacHelpers.IsIVPNAppIsRunning(out runingApplication)) {

                NSAlert alert = NSAlert.WithMessage(
                                    "Please quit the IVPN client before installing a new version", 
                                    "Retry", "Terminate currently running IVPN client", "Quit", 
                                    "Another version of the IVPN client is running. Please quit/terminate the existing version before running the new one.");

                var runModalResult = alert.RunModal();

                if (runModalResult == 1)
                    continue;
                else if (runModalResult == 0) {
                    runingApplication.Terminate();
                    for (int i = 0; i < 50; i++) {
                        if (runingApplication.Terminated)
                            break;

                        Thread.Sleep(50);
                    }
                } else
                    return false;
            }

            return true;
        }

        private static bool PartialUpgrade()
        {
            if (!WaitUntilAnotherVersionIsClosed())
                return false;
            
            if (PrivilegeHelper.IsHelperUpgradeRequired())
                return AskAndUpgradeHelper();

            return true;
        }

        private static bool FullUpgradeIfRequired()
        {
            if (!IsRunFromApplicationFolder())
            {

                NSAlert alert = NSAlert.WithMessage(
                                    "IVPN client can only run from the Applications folder. Please move the IVPN.app into the /Applications folder",                                     
                                    "Quit", null, null, "");
                alert.RunModal();

                return false;

            }
            else
            if (PrivilegeHelper.IsHelperUpgradeRequired())
            {
                return AskAndUpgradeHelper();
            }

            return true;
        }

        private static bool IsIncorrectFileOwner()
        {
            return NSFileManager.DefaultManager.GetAttributes("/Applications/IVPN.app").GroupOwnerAccountID != 0;
        }

        private static bool IsRunFromApplicationFolder()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location).StartsWith("/Applications/IVPN.app/");
        }


        private static void UpgradePrivilegedHelper()
        {
            var flags = AuthorizationFlags.Defaults;

            using (var auth = Authorization.Create(flags)) {
                PrivilegeHelper.Upgrade(auth, flags);
            }
        }

        private static bool AskAndUpgradeHelper()
        {
            NSAlert alert = NSAlert.WithMessage(
                                "A new version of IVPN has been installed and privileged helper must be updated to continue.", 
                                "Update Helper", 
                                "Quit", null, "");

            if (alert.RunModal() != 1)
                return false;            

            UpgradePrivilegedHelper();

            return true;
        }
    }
}

