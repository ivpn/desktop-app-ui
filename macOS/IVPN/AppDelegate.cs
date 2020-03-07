using System;
using Foundation;
using AppKit;

using IVPN.Models;
using IVPN.Models.Configuration;

namespace IVPN
{
    public partial class AppDelegate : NSApplicationDelegate
    {
        MainWindowController __MainWindowController;
        AppSettings __Settings;
        SettingsProvider __SettingsProvider;

        public AppDelegate()
        {
            __SettingsProvider = new SettingsProvider();
        }

        public static AppSettings GetAppSettings()
        {
            AppDelegate appDelegate = NSApplication.SharedApplication.Delegate as AppDelegate;
            if (appDelegate == null)
                return null;
            return appDelegate.__Settings;
        }

        public static Interfaces.ILocalizedStrings GetLocalizedStrings()
        {
            return GetMainWindowController()?.GetLocalizedStrings();
        }

        public static MainWindowController GetMainWindowController()
        {
            AppDelegate appDelegate = NSApplication.SharedApplication.Delegate as AppDelegate;
            if (appDelegate == null)
                return null;
            return appDelegate.__MainWindowController;
        }

        public override void WillFinishLaunching(NSNotification notification)
        {
            // Load & initialize settings
            __Settings = AppSettings.InitInstance(__SettingsProvider);

            // processing MacIsShowIconInSystemDock parameter
            ShowIconInSystemDockIfNecessary();
            __Settings.OnSettingsSaved += (sender, e) => ShowIconInSystemDockIfNecessary();

            __MainWindowController = new MainWindowController(__Settings);

            __MainWindowController.RestoreWindowPositions();

            if (!__Settings.LaunchMinimized)
                __MainWindowController.Window.MakeKeyAndOrderFront(this);
            else
                __MainWindowController.Window.IsVisible = false; // fakeout to ensure it's not lazy loaded
        }

        public override bool ApplicationShouldHandleReopen(NSApplication sender, bool hasVisibleWindows)
        {
            if (!hasVisibleWindows)
                __MainWindowController.Window.MakeKeyAndOrderFront(this);

            return true;
        }

        public override NSApplicationTerminateReply ApplicationShouldTerminate(NSApplication sender)
        {
            __MainWindowController.SaveWindowPositions();

            if (__MainWindowController.MainViewModel == null)
                return NSApplicationTerminateReply.Now;

            var connectionState = __MainWindowController.MainViewModel.ConnectionState;
            if (connectionState == ServiceState.Disconnected ||
                connectionState == ServiceState.CancellingConnection ||
                connectionState == ServiceState.Disconnecting ||
                connectionState == ServiceState.Uninitialized)
            {
                __MainWindowController.MainViewModel.DisableFirewallOnExitIfRequired();
                return NSApplicationTerminateReply.Now;
            }

            Action stopForExit = () =>
            {
                __MainWindowController.MainViewModel.DisableFirewallOnExitIfRequired();
                __MainWindowController.MainViewModel.DisconnectCommand.Execute(null);
                __MainWindowController.AwaitingDisconnect = true;
            };

            if (__Settings.DoNotShowDialogOnAppClose)
            {
                stopForExit();
                return NSApplicationTerminateReply.Later;
            }

            NSAlert alert = NSAlert.WithMessage("You are connected to the VPN. Are you sure you want to disconnect and quit?", "Disconnect and Quit", "Cancel", "", "");
            NSRunningApplication.CurrentApplication.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows);

            if (alert.RunModal() == 1)
            {
                stopForExit();
                return NSApplicationTerminateReply.Later;
            }

            return NSApplicationTerminateReply.Cancel;
        }

        #region Application main menu handlers
        partial void openPreferencesSheet(NSObject sender)
        {
            __MainWindowController.ShowPreferencesWindow();
        }

        partial void menuItemOnOpenHelp(Foundation.NSObject sender)
        {
            NSWorkspace.SharedWorkspace.OpenUrl(new NSUrl(Constants.IVPNHelpUrl));
        }
        #endregion //Application main menu handlers

        private void ShowIconInSystemDockIfNecessary()
        {
            InvokeOnMainThread(() =>
           {
               try
               {
                   NSApplicationActivationPolicy currPolicy = NSApplication.SharedApplication.ActivationPolicy;
                   NSApplicationActivationPolicy requiredPolicy = NSApplicationActivationPolicy.Accessory;

                   if (__Settings.MacIsShowIconInSystemDock)
                       requiredPolicy = NSApplicationActivationPolicy.Regular;

                   if (!requiredPolicy.Equals(currPolicy))
                       NSApplication.SharedApplication.ActivationPolicy = requiredPolicy;
               }
               catch (Exception ex)
               {
                   Logging.Info(string.Format("{0}", ex));
               }
           });
        }
    }
}

