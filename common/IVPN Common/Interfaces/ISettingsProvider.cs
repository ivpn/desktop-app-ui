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
