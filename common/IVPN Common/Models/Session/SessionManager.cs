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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IVPN.Interfaces;
using IVPN.RESTApi;

namespace IVPN.Models.Session
{
    /// <summary>
    /// Checks session status in background
    /// </summary>
    public class SessionManager : ISessionManager
    {
        #region Singleton
        private static SessionManager __Instance;
        public static SessionManager CreateSessionManager(ISessionKeeper sessionKeeper, IService service)
        {
            if (__Instance == null)
                __Instance = new SessionManager(sessionKeeper, service);
            return __Instance;
        }
        #endregion //Singleton

        #region Internal variables

        // Settings object
        private readonly ISessionKeeper __SessionKeeper;
        private readonly IService __Service;
        #endregion //Internal variables

        #region Public functionality
        public event OnSessionRequestErrorDelegate OnSessionRequestError = delegate { };

        private SessionManager(ISessionKeeper sessionKeeper, IService service)
        {
            __SessionKeeper = sessionKeeper;
            __Service = service;
        }

        /// <summary> New session </summary>
        public async Task CreateNewSessionAsync(string accountID, bool isForceDeleteAllSessions = false)
        {            
            try
            {
                var resp = await __Service.Login(accountID, isForceDeleteAllSessions);
                if (resp.APIStatus == 0)
                    throw new Exception("Internal error: Failed to create session");
                else if (resp.APIStatus != (int)ApiStatusCode.Success)
                {
                    if (resp.APIStatus == (int)ApiStatusCode.SessionTooManySessions)
                        OnSessionRequestError(resp.APIStatus, resp.APIErrorMessage, resp.Account);

                    throw new IVPNRestRequestApiException(HttpStatusCode.OK, (ApiStatusCode)resp.APIStatus, resp.APIErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Logging.Info($"{ex}");
                throw;
            }
        }

        /// <summary> Delete current session </summary>
        public async Task DeleteCurrentSessionAsync(CancellationToken? cancellationToken = null)
        {
            try
            {
                try
                {
                    await __Service.Logout();
                }
                catch (Exception ex)
                {
                    Logging.Info($"Failed to delete session from IVPN server: {ex}");
                }                
            }
            catch (Exception ex)
            {
                Logging.Info($"{ex}");
                throw;
            }
        }

        /// <summary> Request account check in background </summary>        
        public void RequestStatusCheck()
        {
            Task.Run(async () =>
            {
                try
                {
                    if (!__SessionKeeper.IsLoggedIn())
                        return; // User not loged-in () no registered sesion - nonthing to check

                    await __Service.AccountStatus();
                }
                catch (Exception ex)
                {
                    Logging.Info($"{ex}");
                } 
            });
        }
        #endregion //Public functionality
    }
}
