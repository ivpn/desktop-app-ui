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

﻿using IVPN.Models;
using System;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IVPNCommon.Interfaces;

namespace IVPN.Interfaces
{
    public delegate void VPNDisconnected(bool failure, DisconnectionReason reason, string reasonDescription);
    public delegate void VPNConnected(ConnectionInfo connectionInfo);

    public interface IService: INotifyPropertyChanged
    {
        event VPNDisconnected Disconnected;
        event VPNConnected Connected;

        event EventHandler ServiceInitialized;
        event EventHandler ServiceExited;

        event IVPNClientProxy.AlternateDNSChangedHandler AlternateDNSChanged;

        Task<ConnectionResult> Connect(IProgress<string> progress,
                                 CancellationToken cancellationToken,
                                 ConnectionTarget connectionTarget);
        void Disconnect();
        void Exit();

        IServers Servers { get; }

        ServiceState State { get; }
        bool KillSwitchIsEnabled { get; set; }

        bool KillSwitchAllowLAN { set; }

        bool KillSwitchAllowLANMulticast { set; }

        bool KillSwitchIsPersistent { get;  set; }

        ConnectionTarget ConnectionTarget { get; }

        Task PauseOn();
        Task PauseOff();

        Task<Responses.SessionNewResponse> Login(string accountID, bool forceDeleteAllSesssions);
        Task Logout();
        Task<Responses.AccountStatusResponse> AccountStatus();

        Task WireGuardGeneratedKeys(bool generateIfNecessary);
        Task WireGuardKeysSetRotationInterval(Int64 interval);

        Task<bool> SetDns(IPAddress dns);

        /// <summary>
        /// Register connection progress object
        /// All registered objects will be notified about progress during connection
        /// </summary>
        /// <param name="progress"></param>
        void RegisterConnectionProgressListener(IProgress<string> progress);
        
    }
}
