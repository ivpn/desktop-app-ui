using System;
using System.Threading.Tasks;
using IVPN.Interfaces;
using IVPN.Models;

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
                        __AppState.Settings.Reset();
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
