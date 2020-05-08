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

using System;

namespace IVPN.Shell
{
    public class ShellCommandResult
    {
        private bool __IsAborted;
        private int __ExitCode;
        private string __Output;
        private string __ErrorOutput;
        
        public ShellCommandResult(bool isAborted, int exitCode, string output, string errorOutput)
        {
            __IsAborted = isAborted;
            __ExitCode = exitCode;
            __Output = output;
            __ErrorOutput = errorOutput;
        }

        public bool IsAborted
        {
            get {
                return __IsAborted;
            }
        }

        public int ExitCode 
        {
            get {
                return __ExitCode;
            }
        }

        public string Output
        {
            get {
                return __Output;
            }
        }

        public string ErrorOutput
        {
            get {
                return __ErrorOutput;
            }
        }

    }
}

