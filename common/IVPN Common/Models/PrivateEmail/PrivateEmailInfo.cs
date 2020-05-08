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

﻿namespace IVPN.Models.PrivateEmail
{
    /// <summary>
    /// Information about single private email
    /// </summary>
    public class PrivateEmailInfo
    {
        /// <summary> Private email address </summary>
        public string Email { get; }

        /// <summary> Email addres to which will be forwarded all mails</summary>
        public string ForwardToEmail { get; }

        /// <summary> Notes </summary>
        public string Notes { get; }

        public PrivateEmailInfo (string email, string forwardToEmail, string notes)
        {
            Email = email;
            ForwardToEmail = forwardToEmail;
            Notes = notes;
        }
    }
}
