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
