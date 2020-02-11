using IVPN.Models;
using IVPN.ViewModels;

namespace IVPN.Interfaces
{
    public interface IMainWindow
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
