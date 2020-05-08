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

﻿using IVPN.VpnProtocols;

namespace IVPN.Models.Configuration
{
    public class ServerPort: ModelBase
    {
        private DestinationPort __DestinationPort;
        private bool __IsEnabled;

        public ServerPort(DestinationPort destinationPort)
        {
            __DestinationPort = destinationPort;
            __IsEnabled = true;
        }

        public DestinationPort DestinationPort
        {
            get => __DestinationPort;
            set => __DestinationPort = value;
        }

        public DestinationPort.ProtocolEnum Protocol => __DestinationPort.Protocol;

        public override string ToString()
        {
            return __DestinationPort.ToString();
        }

        public int Port => __DestinationPort.Port;

        public bool IsEnabled
        {
            get => __IsEnabled;

            set
            {
                __IsEnabled = value;
                DoPropertyChanged();
            }
        }
    }
}
