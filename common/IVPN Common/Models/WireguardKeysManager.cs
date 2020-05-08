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
using System.Threading;
using System.Threading.Tasks;
using IVPN.Interfaces;
using IVPN.Models.Configuration;

namespace IVPN.Models
{
    public class WireguardKeysManager : ModelBase
    {
        public const int HardExpirationIntervalDays = 40;

        private readonly SemaphoreSlim __LockerSemaphore = new SemaphoreSlim(1,1);
        private readonly Func<bool> __IsCanUpdateKey;
        private readonly IService __Service;

        #region Public functionality
        public delegate void OnProgressDelegate(string progress);
        public event OnProgressDelegate OnProgress = delegate {};

        public delegate void OnStartStopDelegate();
        public event OnStartStopDelegate OnStarted = delegate { };
        public event OnStartStopDelegate OnStopped = delegate { };

        public WireguardKeysManager(IService service, Func<bool> isCanUpdateKey)
        {
            __Service = service;
            __IsCanUpdateKey = isCanUpdateKey;
        }

        /// <summary>
        /// Generates the new key.
        /// If there was previous key defined - it will be removed firstly
        /// </summary>
        /// <returns>The new key async.</returns>
        public async Task GenerateNewKeysAsync()
        {
            await __LockerSemaphore.WaitAsync();
            try
            {
                OnStarted();
                OnProgress("Generating new keys...");
                await __Service.WireGuardGeneratedKeys(false);
            }
            finally
            {
                OnProgress("");
                OnStopped();
                __LockerSemaphore.Release();
            }
        }

        /// <summary>
        ///     Checks if keys should be re-generated and regenerate them (if necessary).
        ///     This method also starting timer for auto-update mechanism.
        /// </summary>
        /// <returns>
        ///     TRUE - if key upgrade not required or upgrade was success
        ///     FALSE - when upgrade was failed
        /// </returns>
        public async Task<bool> RegenerateKeysIfNecessary()
        {
            if (!__IsCanUpdateKey())
                return true;

            await __LockerSemaphore.WaitAsync();
            try
            {
                OnStarted();
                OnProgress("Generating new keys...");
                await __Service.WireGuardGeneratedKeys(true);
                return true;
            }
            finally
            {
                OnProgress("");
                OnStopped();
                __LockerSemaphore.Release();
            }
        }
        #endregion //Public functionality
    }
}
