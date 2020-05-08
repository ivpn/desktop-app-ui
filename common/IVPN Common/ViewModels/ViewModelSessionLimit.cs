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

﻿using System.Windows.Input;
using IVPN.Interfaces;
using IVPN.Models;
using IVPN.RESTApi;

namespace IVPN.ViewModels
{
    public class ViewModelSessionLimit : ViewModelBase
    {
        #region Internal variables
        private readonly AppState __AppState;
        private readonly IApplicationServices __AppServices;
        private readonly IAppNavigationService __NavigationService;
        private readonly ViewModelLogIn __LogInViewModel;

        #endregion //Internal variables

        public ICommand TryAgainCommand { get; }
        public ICommand LogOutAllSessionsCommand { get; }
        public ICommand UpgradeToProPlanCommand { get; }
        public ICommand GoBackCommand { get; private set; }

        public string UpgradeToUrl { get; private set; } 

        public ViewModelSessionLimit(
            ViewModelLogIn logInViewModel,
            AppState appState,
            IApplicationServices appServices,
            IAppNavigationService navigationService)
        {
            __LogInViewModel = logInViewModel;
            __AppState = appState;
            __AppServices = appServices;
            __NavigationService = navigationService;

            TryAgainCommand = new RelayCommand(TryAgain);
            LogOutAllSessionsCommand = new RelayCommand(LogOutAllSessions);
            UpgradeToProPlanCommand = new RelayCommand(UpgradeToProPlan);
            GoBackCommand = new RelayCommand(GoBack);

            __LogInViewModel.PropertyChanged += __LogInViewModel_PropertyChanged;

            __AppState.SessionManager.OnSessionRequestError += SessionManager_OnSessionRequestError;
        }

        private void SessionManager_OnSessionRequestError(int apiStatus, string apiErrMes, Responses.AccountInfo account)
        {
            if (apiStatus != (int)ApiStatusCode.SessionTooManySessions)
                return;
            
            UpgradeToUrl = account.UpgradeToURL;
            IsCanUpgrade = account.Upgradable && !string.IsNullOrEmpty(UpgradeToUrl);
        }

        private void __LogInViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (nameof(ViewModelLogIn.UserName).Equals(e.PropertyName))
            {
                RaisePropertyWillChange(nameof(IsCanLogOutAllSessions));
                RaisePropertyChanged(nameof(IsCanLogOutAllSessions));
            }
        }

        private void TryAgain()
        {
            // Just show LogIn page and try to LogIn
            __NavigationService.NavigateToLogInPage(NavigationAnimation.FadeToRight, true);
        }

        private void LogOutAllSessions()
        {
            // Just show LogIn page and try to force LogIn
            __NavigationService.NavigateToLogInPage(NavigationAnimation.FadeToRight, true, true);
        }

        private void UpgradeToProPlan()
        {
            __NavigationService.OpenUrl(UpgradeToUrl);
        }

        private void GoBack()
        {
            __NavigationService.NavigateToLogInPage(NavigationAnimation.FadeToRight);
        }

        private bool __IsCanUpgrade;
        public bool IsCanUpgrade
        {
            get => __IsCanUpgrade;
            private set
            {
                RaisePropertyWillChange();
                __IsCanUpgrade = value;
                RaisePropertyChanged();
            }
        }
        
        public bool IsCanLogOutAllSessions
        {
            get => !string.IsNullOrEmpty(__LogInViewModel.UserName);            
        }
    }
}
