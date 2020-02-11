using System;
using IVPN.Models;

namespace IVPN.Interfaces
{
    public interface IAppNavigationService
    {
        event EventHandler<NavigationTarget> Navigated;

        void NavigateToMainPage(NavigationAnimation animation);
        void NavigateToInitPage(NavigationAnimation animation);
        void NavigateToLogInPage(NavigationAnimation animation, bool doLogIn = false, bool doForceLogin = false);
        void NavigateToSessionLimitPage(NavigationAnimation animation);
        void NavigateToLogOutPage(NavigationAnimation animation);
        void NavigateToSingUpPage(NavigationAnimation animation);
        
        void NavigateToServerSelection(NavigationAnimation animation = NavigationAnimation.FadeToLeft);
        void NavigateToEntryServerSelection(NavigationAnimation animation = NavigationAnimation.FadeToLeft);
        void NavigateToExitServerSelection(NavigationAnimation animation = NavigationAnimation.FadeToLeft);

        void NavigateToAutomaticServerConfiguration(NavigationAnimation animation = NavigationAnimation.FadeToLeft);

        void OpenUrl(string url);

        void ServerLocationSelectedAutomatic();
        void ServerLocationSelected(ServerLocation serverLocation);

        void GoBack();

        void ShowSettingsWindow();

        NavigationTarget CurrentPage
        {
            get;
        }
    }
}
