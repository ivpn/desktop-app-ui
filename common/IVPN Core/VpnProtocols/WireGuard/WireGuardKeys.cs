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

﻿using System.Threading.Tasks;
using IVPN.Exceptions;
using IVPN.Shell;

namespace IVPN.VpnProtocols.WireGuard
{
    public class Keys
    {

        /// <summary>
        /// Generate new pair of Private\Public keys
        /// </summary>
        /// <returns>Public key</returns>
        /// <param name="privateKey">Private key</param>
        private static string GenerateKeys(out string privateKey)
        {
            // private key generation
            ShellCommandResult result = ShellCommand.RunCommand(Platform.WireGuardWgExecutablePath, $"genkey", null, 5000, true);
            if (result.ExitCode != 0)
                throw new IVPNException($"Private key generation error: {result.ExitCode}" + ((result.ErrorOutput == null) ? "" : $" Error: {result.ErrorOutput}"));
            privateKey = result.Output.Trim();

            // public key generation
            result = ShellCommand.RunCommand(Platform.WireGuardWgExecutablePath, $"pubkey", privateKey, 5000, true);
            if (result.ExitCode != 0)
                throw new IVPNException($"Public key generation error: {result.ExitCode}" + ((result.ErrorOutput == null) ? "" : $" Error: {result.ErrorOutput}"));

            return result.Output.Trim();
        }

        public static async Task<string[]> GenerateKeysAsync()
        {
            string[] ret = new string[2];

            string privateKey = "";
            string publicKey = "";

            await Task.Run(() => publicKey = GenerateKeys(out privateKey));

            ret[0] = privateKey;
            ret[1] = publicKey;

            return ret;
        }
    }
}
