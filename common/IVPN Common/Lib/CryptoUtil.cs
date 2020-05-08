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

namespace IVPN.Lib
{
    public class CryptoUtil
    {
        static byte[] entropy = System.Text.Encoding.UTF8.GetBytes("McNfJg5jJ;jJ.cL:'[!80^!*-dfBJnkd~!KPr3408");

        public static string EncryptString(string password)
        {
            try 
            {
                if (password == null)
                    password = "";
                
                byte [] encryptedData = System.Security.Cryptography.ProtectedData.Protect (
                    System.Text.Encoding.Unicode.GetBytes (password),
                    entropy,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);

                return Convert.ToBase64String (encryptedData);
            }
            catch (Exception ex)
            {
                Logging.Info(string.Format("Encryption error: {0}", ex));
                throw;
            }
        }

        public static string DecryptString(string encryptedData)
        {
            if (string.IsNullOrEmpty(encryptedData))
                return "";
            
            try
            {
                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    entropy,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);

                return System.Text.Encoding.Unicode.GetString(decryptedData);
            }
            catch (Exception ex)
            {
                Logging.Info(string.Format("Decryption error: {0}", ex));
                return "";
            }
        }
    }
}
