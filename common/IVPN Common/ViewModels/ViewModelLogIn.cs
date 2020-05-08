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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using IVPN.Interfaces;
using IVPN.Lib;
using IVPN.Models;
using IVPN.Models.Session;
using IVPN.RESTApi;
using IVPNCommon.Api;

namespace IVPN.ViewModels
{
    /// <summary>
    /// LogIn ViewModel
    /// </summary>
    public class ViewModelLogIn : ViewModelBase, IOperationStartStopNotifier
    {
        #region Internal variables
        private readonly AppState __AppState;
        private readonly IApplicationServices __AppServices;
        private readonly IAppNavigationService __NavigationService;
        private readonly WireguardKeysManager __WireguardKeysManager;
        protected CancellationTokenSource CancellationTokenSource;
        #endregion //Internal variables

        #region Events
        public delegate void OnAccountSuspendedDelegate(AccountStatus session);
        public event OnAccountSuspendedDelegate OnAccountSuspended = delegate { };

        public event OnOperationExecutionEventDelegate OnWillExecute = delegate { };
        public event OnOperationExecutionEventDelegate OnDidExecute = delegate { };

        public event OnErrorDelegate OnAccountCredentailsError = delegate { };
        #endregion //Events

        #region Public variables
        public ICommand LogInCommand { get; }
        public ICommand LogInAndDeleteAllSessionsCommand { get; }
        public ICommand StartFreeTrialCommand { get; }
        public string UserName
        {
            get => __Username;
            set
            {
                RaisePropertyWillChange();
                __Username = value?.Trim();
                RaisePropertyChanged();
            }
        }
        private string __Username;

        #endregion //Public variables

        #region Public functionality
        public ViewModelLogIn(
            AppState appState,
            IApplicationServices appServices,
            IAppNavigationService navigationService,
            WireguardKeysManager wgKeysManager)
        {
            __AppState = appState;
            __AppServices = appServices;
            __NavigationService = navigationService;
            __WireguardKeysManager = wgKeysManager;

            UserName = __AppState.Session?.AccountID ?? "";

            StartFreeTrialCommand = new RelayCommand(StartFreeTrial);
            LogInCommand = new RelayCommand(() => LogIn());
            LogInAndDeleteAllSessionsCommand = new RelayCommand(() => LogIn(forceDeleteAllSessions: true));
        }

        private async Task ApiRequest(CancellationToken cancellationToken, bool isForceDeleteAllSessions = false)
        {
            string accountID = UserName;
            try
            {
                await __AppState.SessionManager.CreateNewSessionAsync(accountID, isForceDeleteAllSessions);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (TimeoutException)
            {
                NotifyError(__AppServices.LocalizedString("Error_ApiRequestTimeout"));
                return;
            }
            catch (IVPNRestRequestApiException ex)
            {
                if (ex.ApiStatusCode == ApiStatusCode.SessionTooManySessions)
                    __NavigationService.NavigateToSessionLimitPage(NavigationAnimation.FadeToLeft); // Show 'session limit' page
                else if (ex.ApiStatusCode == ApiStatusCode.Unauthorized)
                    OnAccountCredentailsError(__AppServices.LocalizedString("Error_Authentication"));
                else
                {
                    Logging.Info("EXCEPTION on LogIn (API request): " + ex);
                    NotifyError(__AppServices.LocalizedString("Error_RestServer_ConnectionError_Title"), $"There was an error creating a new session{Environment.NewLine}{ex.Message}");
                }
                return;
            }
            catch (WebException ex)
            {
                Logging.Info($"EXCEPTION on LogIn (API request): {ex}");

                NotifyError(__AppServices.LocalizedString("Error_RestServer_ConnectionError_Title"),
                    __AppServices.LocalizedString("Error_RestServer_ConnectionError"));
                return;
            }
            catch (Exception ex)
            {
                Logging.Info($"EXCEPTION on LogIn (API request): {ex}");

                NotifyError(__AppServices.LocalizedString("Error_RestServer_ConnectionError_Title"), $"{ex.Message}");
                return;
            }

            if (!__AppState.IsLoggedIn())
            {
                OnAccountCredentailsError(__AppServices.LocalizedString("Error_Authentication"));
                return;
            }

            if (__AppState.AccountStatus!= null && __AppState.AccountStatus.IsActive == false)
            {
                OnAccountSuspended(__AppState.AccountStatus);
                return;
            }

            // LOGIN SUCCES:

            __NavigationService.NavigateToMainPage(NavigationAnimation.FadeToLeft);
        }

        private bool IsCredentialsOk()
        {
            // check username and password
            if (string.IsNullOrEmpty(UserName))
            {
                OnAccountCredentailsError(__AppServices.LocalizedString("Error_UserNameIsEmpty"));
                return false;
            }

            if (!ValidateUsername(UserName))
            {
                OnAccountCredentailsError(__AppServices.LocalizedString("Message_InvalidUsername"),
                    __AppServices.LocalizedString("Message_InvalidUsername_Description"));
                return false;
            }

            return true;
        }

        private bool PrepareBeforeLogIn()
        {
            if (CancellationTokenSource != null)
            {
                CancellationTokenSource.Cancel();
                CancellationTokenSource = null;
                return false;
            }

            // check username and password
            if (!IsCredentialsOk())
                return false;

            return true;
        }

        private async void LogIn(bool forceDeleteAllSessions = false)
        {
            if (!PrepareBeforeLogIn())
                return;

            try
            {
                OnWillExecute(this);

                CancellationTokenSource = new CancellationTokenSource();

                await ApiRequest(CancellationTokenSource.Token, forceDeleteAllSessions);
            }
            finally
            {
                CancellationTokenSource = null;
                OnDidExecute(this);
            }
        }

        private void StartFreeTrial()
        {
            __NavigationService.NavigateToSingUpPage(NavigationAnimation.FadeToLeft);
        }

        #endregion //Public functionality

        #region Private functionality
        private bool ValidateUsername(string username)
        {
            if (Regex.IsMatch(username, "^ivpn[a-zA-Z0-9]{7,8}$"))
                return true;

            return false;
        }
        #endregion // Private functionality
    }
}
