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
using System.Threading.Tasks;
using IVPN.Interfaces;
using IVPN.Models;
using IVPN.Models.Configuration;

namespace IVPN.ViewModels
{
    public class ViewModelLogOut : ViewModelBase
    {
        private readonly AppState __AppState;
        private readonly IAppNavigationService __NavigationService;
        private readonly MainViewModel __MainViewModel;

        public ViewModelLogOut(AppState appState, 
            IApplicationServices appServices, 
            IAppNavigationService navigationService,
            MainViewModel mainViewModel)
        {
            __AppState = appState;
            __NavigationService = navigationService;

            __MainViewModel = mainViewModel;
        }

        private string __ProgressStatus; 
        public string ProgressStatus
        {
            get => __ProgressStatus;
            private set
            {
                RaisePropertyWillChange();
                __ProgressStatus = value;
                RaisePropertyChanged();
            }
        }

        private bool __IsInProgress;
        public bool IsInProgress
        {
            get => __IsInProgress;
            private set
            {
                RaisePropertyWillChange();
                __IsInProgress = value;
                RaisePropertyChanged();
            }
        }

        public async Task DoLogOut(bool showSessionLimit)
        {
            try
            {
                IsInProgress = true;

                try
                {
                    __MainViewModel.ForceDisconnectAndDisableFirewall();
                }
                catch (Exception ex)
                {
                    // ignore
                    Logging.Info("[ERROR] Failed to ForceDisconnectAndDisableFirewall(): " + ex);
                }

                try
                {
                    ProgressStatus = "Deleting session from IVPN server...";
                    await __AppState.SessionManager.DeleteCurrentSessionAsync();
                }
                catch (Exception ex)
                {
                    // ignore
                    Logging.Info("[ERROR] Failed to remove WireGuard keys from server: " + ex);
                }

                ProgressStatus = "Erasing settings...";
                try
                {
                    await Task.Run(() =>
                    {
                        AppSettings.Instance().Reset();
                        __AppState.Reset();
                    }
                    );
                }
                catch (Exception ex)
                {
                    // ignore
                    Logging.Info("[ERROR] Failed to reset settings: " + ex);
                }
            }
            finally
            {
                IsInProgress = false;
                if (showSessionLimit)
                    __NavigationService.NavigateToSessionLimitPage(NavigationAnimation.FadeToRight);
                else
                    __NavigationService.NavigateToLogInPage(NavigationAnimation.FadeToRight);
            }
        }
    }
}
