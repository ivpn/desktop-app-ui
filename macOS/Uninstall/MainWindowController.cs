//
//  IVPN Client Desktop
//  https://github.com/ivpn/desktop-app-ui
//
//  Created by Stelnykovych Alexandr.
//  Copyright (c) 2020 Privatus Limited.
//
//  This file is part of the IVPN Client Desktop.
//
//  The IVPN Client Desktop is free software: you can redistribute it and/or
//  modify it under the terms of the GNU General Public License as published by the Free
//  Software Foundation, either version 3 of the License, or (at your option) any later version.
//
//  The IVPN Client Desktop is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
//  details.
//
//  You should have received a copy of the GNU General Public License
//  along with the IVPN Client Desktop. If not, see <https://www.gnu.org/licenses/>.
//

﻿
using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;
using Security;
using MacLib;

using System.Threading;
using IVPN;

namespace IVPN_Uninstaller
{
    public partial class MainWindowController : AppKit.NSWindowController
    {
        #region Constructors

        // Called when created from unmanaged code
        public MainWindowController(IntPtr handle) : base(handle)
        {
            Initialize();
        }
        
        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public MainWindowController(NSCoder coder) : base(coder)
        {
            Initialize();
        }
        
        // Call to load from the XIB/NIB file
        public MainWindowController() : base("MainWindow")
        {
            Initialize();
        }
        
        // Shared initialization code
        void Initialize()
        {
        }

        #endregion

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            Window.Center();
        }

        private bool __IsUninstalled;

        private static bool WaitUntilAnotherVersionIsClosed()
        {
            NSRunningApplication runingApplication;

            while (MacHelpers.IsIVPNAppIsRunning(out runingApplication)) {

                NSAlert alert = NSAlert.WithMessage(                    
                    "Please quit the IVPN client to continue with uninstallation.", 
                    "Retry", "Terminate currently running IVPN client", "Cancel", 
                    "IVPN client is running, please quit/terminate " +
                    "it before uninstallation.");

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
                }
                else
                    return false;
            }

            return true;
        }

        partial void Uninstall(NSObject sender)
        {
            if (!WaitUntilAnotherVersionIsClosed())
                return;

            if (!DoUninstall()) {
                NSAlert alert = NSAlert.WithMessage(                   
                    "There was an error while uninstalling IVPN Client.", 
                    "OK", null, null,
                    "IVPN client could not be uninstalled");
                
                var runModalResult = alert.RunModal();

                if (runModalResult == 1) {
                    return;
                }
                
                return;
            }
                

            IsUninstalled = true;
        }

        private static AuthorizationFlags GetAuthorizationFlags()
        {
            return AuthorizationFlags.Defaults;
        }

        private bool DoUninstall()
        {
            var flags = GetAuthorizationFlags();

            bool isSuccess = true;
            using (var auth = Authorization.Create(flags)) {

                if (PrivilegeHelper.IsHelperInstalled()) 
                {
                    // Hack to force "authentication required" window to pop-up;
                    auth.ExecuteWithPrivileges("/bin/echo", flags, new string[] { });

                    if (!PrivilegeHelper.Uninstall(auth))
                        return false;
                }


                const string IVPNAppBundleID = "net.ivpn.client.IVPN";
                // Erasing app NSUserDefaults data
                var ret = IVPN.Shell.ShellCommand.RunCommand("defaults", $"delete {IVPNAppBundleID}");
                if (ret.IsAborted || ret.ExitCode != 0)
                {
                    Logging.Info("Failed to delete application user defaults." + ((string.IsNullOrEmpty(ret.ErrorOutput)) ? "" : ret.ErrorOutput));
                    isSuccess = false;
                }

                // Erasing KeyChain
                int i = 0;
                while (IVPN.Shell.ShellCommand.RunCommand("security", $"delete-generic-password -s {IVPNAppBundleID}").ExitCode==0) 
                {
                    if (i++ > 1000) // ensure that we will not have infinite loop
                        break;
                }

                string[] filesToRemove = new string[] {
                    "/Library/Logs/IVPN Agent.log",
                    "/Library/Logs/IVPN Agent.log.0",
                    "/Library/Logs/IVPN Agent CrashInfo.log",
                    "/Library/Logs/IVPN Agent CrashInfo.log.0",
                    "/Library/Application Support/net.ivpn.client.Agent/last-btime", // seems, the file created by OS 
                    System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Personal),  
                        "Library/Preferences/net.ivpn.client.IVPN.plist")
                };

                string[] foldersToRemove = new string[] {
                    "/Applications/IVPN.app",
                    System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "Library/Application Support/IVPN"),
                    "/Library/Application Support/IVPN",
                    "~/Library/Application Support/IVPN",
                    "/Library/Application Support/net.ivpn.client.Agent/LocalMachine", // seems, the folder created by OS 
                    "/Library/Application Support/net.ivpn.client.Agent" // seems, the folder created by OS 
                };

                foreach (var file in filesToRemove) {
                    if (!MacHelpers.RemoveFile(auth, file)) {
                        Logging.Info( String.Format("Cannot remove: {0}", file));
                        isSuccess = false;
                    }
                }

                foreach (var folder in foldersToRemove) {
                    if (!MacHelpers.RemoveDirectory(auth, folder)) {
                        Logging.Info( String.Format("Cannot remove: {0}", folder));
                        isSuccess = false;
                    }
                }
            }

            return isSuccess;
        }

        partial void Quit(NSObject sender)
        {                        
            NSApplication.SharedApplication.Terminate(this);
        }

        [Export("IsUninstalled")]
        public bool IsUninstalled 
        {
            get {
                return __IsUninstalled;
            }
            set {
                WillChangeValue("IsUninstalled");

                __IsUninstalled = value;

                DidChangeValue("IsUninstalled");
            }
        }

        //strongly typed window accessor
        public new MainWindow Window {
            get {
                return (MainWindow)base.Window;
            }
        }
    }
}

