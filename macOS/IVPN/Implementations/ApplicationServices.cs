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

﻿using System;
using IVPN.Interfaces;
using IVPN.Models;
using System.Threading.Tasks;

using MacLib;

namespace IVPN
{
    public class ApplicationServices: IApplicationServices
    {
        // On Mac OS X we have an issue
        // which causes the application to hide after installing Helper dialog box. 
        public event EventHandler HelperMethodInstalled = delegate { };
        
        ILocalizedStrings __LocalizedStrings;

        public ApplicationServices(ILocalizedStrings localizedStrings)
        {
            __LocalizedStrings = localizedStrings;
        }
            
        public string LocalizedString(string key, string defaultText = null)
        {
            return __LocalizedStrings.LocalizedString(key, defaultText);
        }

        public async Task<ServiceStartResult> StartService()
        {
            if (!PrivilegeHelper.IsHelperInstalled()) 
            {
                Logging.Info("helper is not installed");

                bool installHelperResults = false;

                await Task.Run(() => {
                    installHelperResults = PrivilegeHelper.InstallHelper();
                });

                HelperMethodInstalled(this, new EventArgs());

                if (!installHelperResults) {
                    Logging.Info("helper installation failed!");

                    return new ServiceStartResult(true, "There was an error during installation of the helper. Please try again and contact support if the problem persists.");
                }
            }

            return new ServiceStartResult(false);
        }

        private TaskCompletionSource<ServiceAttachResult> __TaskCompletion;

        public Task<ServiceAttachResult> AttachToService()
        {
            __TaskCompletion = new TaskCompletionSource<ServiceAttachResult>();

            PrivilegeHelper.StartAndConnectToLaunchAgent((int connectionPort, UInt64 secret) => {
                if (connectionPort > 0)                    
                    __TaskCompletion.SetResult(new ServiceAttachResult(connectionPort, secret));
                else 
                    __TaskCompletion.SetResult(new ServiceAttachResult("There was an error launching IVPN Agent."));
            });


            return __TaskCompletion.Task;
        }

        public bool IsExiting {
            get {
                return false;
            }
        }

        public bool IsServiceStarted {
            get 
            {
                if (!PrivilegeHelper.IsHelperInstalled())
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Check is service installed (applicable for Windows implementation)
        /// If success - must return True
        /// </summary>
        public bool IsServiceExists => true;
    }
}

