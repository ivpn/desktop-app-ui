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
using System.Net;

namespace IVPN
{
    public class SessionInfo
    {
        public string AccountID { get; }
        public string Session { get; }

        public string WgPublicKey { get; }
        public IPAddress WgLocalIP { get; }
        public DateTime WgKeyGenerated { get; }
        public TimeSpan WgKeyRotateInterval { get; }

        public SessionInfo(
            string accountID,
            string session,
            string wgPublicKey,
            IPAddress wgLocalIP,
            DateTime wgKeyGenerated,
            TimeSpan wgKeyRotateInterval)
        {
            AccountID = accountID;
            Session = session;
            WgPublicKey = wgPublicKey;
            WgLocalIP = wgLocalIP;
            WgKeyGenerated = wgKeyGenerated;
            WgKeyRotateInterval = wgKeyRotateInterval;
        }

        public bool IsWireGuardKeysInitialized()
        {
            return !string.IsNullOrEmpty(WgPublicKey);
        }

        public DateTime GetKeysExpiryDate()
        {
            return WgKeyGenerated.Add(WgKeyRotateInterval);
        }
    }
}
