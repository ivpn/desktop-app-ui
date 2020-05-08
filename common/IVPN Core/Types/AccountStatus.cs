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
using System.Collections.Generic;

namespace IVPN
{
    /// <summary>
    /// Information about user account
    /// </summary>
    public class AccountStatus
    {
        public AccountStatus(bool isActive, DateTime activeUtil, bool isRenewable, bool willAutoRebill, bool isOnFreeTrial, string[] capabilities)
        {
            IsActive = isActive;
            ActiveUtil = activeUtil;
            IsRenewable = isRenewable;
            WillAutoRebill = willAutoRebill;
            IsOnFreeTrial = isOnFreeTrial;

			if (capabilities==null || capabilities.Length<=0)
                Capabilities = new List<string> ();
            else
                Capabilities = new List<string> ( capabilities );
        }

        public bool IsActive { get; }
		public DateTime ActiveUtil { get; }
		public bool IsRenewable { get; }
		public bool WillAutoRebill { get; }
		public bool IsOnFreeTrial { get; }
		public List <string> Capabilities { get; }
    }
}
