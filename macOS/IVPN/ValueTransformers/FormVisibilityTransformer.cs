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

using Foundation;
using IVPN.Models;

namespace IVPN
{
    public class FormVisibilityTransformer: NSValueTransformer
    {
        protected NSObject ToNSBoolean(bool value)
        {
            return NSNumber.FromBoolean(value);
        }

        protected ServiceState GetServiceState(NSObject value)
        {
            var intValue = (int)(NSNumber)value;
            return (ServiceState)intValue;
        }

        protected bool IsConnected(NSObject value)
        {
            var state = GetServiceState(value);

            if (state == ServiceState.Connected)
                return true;

            return false;
        }

        protected bool IsConnecting(NSObject value)
        {
            var state = GetServiceState(value);

            if (state == ServiceState.Connecting ||
                state == ServiceState.Disconnecting ||
                state == ServiceState.CancellingConnection ||
                state == ServiceState.ReconnectingOnClient ||
                state == ServiceState.ReconnectingOnService)
                return true;

            return false;
        }

        protected bool IsDisconnected(NSObject value)
        {
            var state = GetServiceState(value);

            if (state == ServiceState.Disconnected ||
                state == ServiceState.Uninitialized)
                return true;

            return false;
        }
    }
}

