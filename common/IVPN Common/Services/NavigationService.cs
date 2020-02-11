using System;

using IVPN.Interfaces;
using IVPN.Models;

namespace IVPN
{
    public class NavigationService : IAppNavigationService
    {
        public event EventHandler<NavigationTarget> Navigated = delegate { };

        private IMainWindow __MainWindowController;
        private NavigationTarget __CurrentPage;

        public NavigationService(IMainWindow mainWindowController)
        {
            __MainWindowController = mainWindowController;
        }

        #region IAppNavigationService implementation

        public void NavigateToMainPage(NavigationAnimation animation)
        {
            // Avoid enter Main page when we are logging-out. Can happen on startup (on first check of session status) when session was deleted from API server.
            // TODO: bad architecture. 
            if (CurrentPage == NavigationTarget.LogOutPage)
                return;

            __MainWindowController.ShowMainPage(animation);
            CurrentPage = NavigationTarget.MainPage;
        }

        public void NavigateToInitPage(NavigationAnimation animation)
        {
            __MainWindowController.ShowInitPage(animation);
            CurrentPage = NavigationTarget.InitPage;
        }

        public void NavigateToLogInPage(NavigationAnimation animation, bool doLogIn = false, bool doForceLogin = false)
        {
            // firewall should be disabled on LogIn page 
            __MainWindowController.MainViewModel.ForceDisconnectAndDisableFirewall();
            
            __MainWindowController.ShowLogInPage(animation, doLogIn, doForceLogin);
            CurrentPage = NavigationTarget.LogInPage;
        }
        
        public void NavigateToSessionLimitPage(NavigationAnimation animation)
        {
            // firewall should be disabled on LogIn page 
            __MainWindowController.MainViewModel.ForceDisconnectAndDisableFirewall();

            // if user is authenticated - do the LogOut first
            if (__MainWindowController.AppState.IsAuthenticated())
            {
                __MainWindowController.ShowLogOutPage(animation, showSessionLimit: true);
                CurrentPage = NavigationTarget.LogOutPage;
            }
            else
            {
                __MainWindowController.ShowSessionLimitPage(animation);
                CurrentPage = NavigationTarget.SessionLimitPage;
            }
        }

        public void NavigateToLogOutPage(NavigationAnimation animation)
        {
            try
            {
                __MainWindowController.ShowLogOutPage(animation);
                CurrentPage = NavigationTarget.LogOutPage;
            }
            catch
            {
                NavigateToLogInPage(NavigationAnimation.FadeToRight);
                CurrentPage = NavigationTarget.LogInPage;
            }
        }

        public void NavigateToSingUpPage(NavigationAnimation animation)
        {
            __MainWindowController.ShowSingUpPage(animation);
            CurrentPage = NavigationTarget.SingUpPage;
        }

        public void NavigateToAutomaticServerConfiguration(NavigationAnimation animation = NavigationAnimation.FadeToLeft)
        {
            __MainWindowController.ShowFastestServerConfiguration(animation);
            CurrentPage = NavigationTarget.AutomaticServerConfiguration;
        }

        public void NavigateToServerSelection(NavigationAnimation animation)
        {
            if (__MainWindowController.MainViewModel.IsAutomaticServerSelection)
                __MainWindowController.ServerListViewModel.SetAutomaticServerSelection(ServerSelectionType.SingleServer);
            else
                __MainWindowController.ServerListViewModel.SetSelectedServer(__MainWindowController.MainViewModel.SelectedServer, ServerSelectionType.SingleServer);

            __MainWindowController.ShowServersPage(animation);
            CurrentPage = NavigationTarget.ServerSelection;
        }

        public void NavigateToEntryServerSelection(NavigationAnimation animation)
        {
            __MainWindowController.ServerListViewModel.SetSelectedServer(__MainWindowController.MainViewModel.SelectedServer, ServerSelectionType.EntryServer);

            __MainWindowController.ShowServersPage(animation);
            CurrentPage = NavigationTarget.ServerSelection;
        }

        public void NavigateToExitServerSelection(NavigationAnimation animation)
        {
            __MainWindowController.ServerListViewModel.SetSelectedServer(__MainWindowController.MainViewModel.SelectedExitServer, ServerSelectionType.ExitServer);

            __MainWindowController.ShowServersPage(animation);
            CurrentPage = NavigationTarget.ServerSelection;
        }

        public void OpenUrl(string url)
        {
            if (string.IsNullOrEmpty(url)
                || !Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                return;

            __MainWindowController.OpenUrl(url);
        }

        public void ServerLocationSelectedAutomatic()
        {
            __MainWindowController.MainViewModel.IsAutomaticServerSelection = true;
            NavigateToMainPage(NavigationAnimation.FadeToRight);
            CurrentPage = NavigationTarget.MainPage;
        }

        public void ServerLocationSelected(ServerLocation serverLocation)
        {
            if (__MainWindowController.ServerListViewModel.ServerSelectionType == ServerSelectionType.ExitServer)
                __MainWindowController.MainViewModel.SelectedExitServer = serverLocation;
            else
                __MainWindowController.MainViewModel.SelectedServer = serverLocation;

            NavigateToMainPage(NavigationAnimation.FadeToRight);
            CurrentPage = NavigationTarget.MainPage;
        }

        public void GoBack()
        {
            switch (CurrentPage)
            {
                case NavigationTarget.AutomaticServerConfiguration:
                    // Save configuration
                    // perform save in background thread to avoid GUI freeze
                    System.Threading.Tasks.Task.Run(() => __MainWindowController.MainViewModel.Settings.Save());
                    __MainWindowController.MainViewModel.ReInitializeFastestSever();

                    NavigateToServerSelection(NavigationAnimation.FadeToRight);
                    break;

                default:
                    NavigateToMainPage(NavigationAnimation.FadeToRight);
                    break;
            }
        }

        public void ShowSettingsWindow()
        {
            __MainWindowController.ShowPreferencesWindow();
        }

        private void RaiseNavigated(NavigationTarget navigationTarget)
        {
            Navigated(this, navigationTarget);
        }

        public NavigationTarget CurrentPage
        {
            get => __CurrentPage;

            private set
            {
                __CurrentPage = value;
                RaiseNavigated(value);
            }
        }

        #endregion
    }
}

