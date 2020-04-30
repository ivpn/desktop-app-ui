using IVPN.Models;
using IVPN.ViewModels;
using System.ComponentModel;

namespace IVPN.Interfaces
{
    public interface IMainWindow: ISynchronizeInvoke
    {
        void ShowPreferencesWindow();

        void ShowLogInPage(NavigationAnimation animation, bool doLogIn = false, bool doForceLogin = false);
        void ShowSessionLimitPage(NavigationAnimation animation);
        void ShowLogOutPage(NavigationAnimation animation, bool showSessionLimit = false);
        void ShowSingUpPage(NavigationAnimation animation);
        void ShowMainPage(NavigationAnimation animation);
        void ShowInitPage(NavigationAnimation animation);
        void ShowServersPage(NavigationAnimation animation);
        void ShowFastestServerConfiguration(NavigationAnimation animation);

        void OpenUrl(string url);

        AppState AppState { get; }
        MainViewModel MainViewModel { get; }
        ServerListViewModel ServerListViewModel { get; }
    }
}
