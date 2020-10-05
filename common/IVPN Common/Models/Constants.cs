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

﻿using System.Collections.Generic;

namespace IVPN
{
    public class Constants
    {
        /// <summary>
        /// When long time no response from server during connection - watchdog can cancel current connection process
        /// and try to connect to another port.
        /// 
        /// Note:
        ///     Ideally, it should be bigger than 'hand-window XXX' seconds from openvpn configuration.
        /// </summary>
        public const int ConnectionWatchDogTimerTimeoutMs = 7000; 

        public static Dictionary<string, string> STATE_DESCRIPTIONS = new Dictionary<string, string> {
            {"WAIT", "Waiting for server..."},
            {"RESOLVE", "Looking up hostname..."},
            {"AUTH", "Authenticating..."},
            {"GETCONFIG", "Obtaining configuration..."},
            {"ASSIGNIP", "Assigning IP address..."},
            {"ADDROUTES", "Adding routes..."},
            {"RECONNECTING", "Reconnecting..."},
            {"DISCONNECTED", "Disconnected"} // XXX
        };


        #region Experiments
        public static string CAPABILITIES_PRIVATE_EMAILS = @"private-emails";
        public static string CAPABILITIES_MULTIHOP = @"multihop";
        #endregion

        public static string GetRenewUrl (string username)
        {
            return @"https://www.ivpn.net/account";
        }

        public static string GetSignUpUrl ()
        {
            return @"https://ivpn.net/signup?os=" + Platform.ShortPlatformName;
        }

        public static string IVPNHelpUrl => @"https://www.ivpn.net/knowledgebase";
    }
}
