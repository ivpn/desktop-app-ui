using System.Linq;
using System.Windows.Input;
using System.ComponentModel;
using IVPN.Models;
using IVPN.Interfaces;

namespace IVPN.ViewModels
{
    public class ServerListViewModel: ViewModelBase
    {
        #region Internal properties
        private readonly IApplicationServices __AppServices;
        private readonly IAppNavigationService __NavigationService;
        private readonly MainViewModel __MainViewModel;
        
        private bool __IsAutomaticServerSelected;
        private ServerSelectionType __ServerSelectionType;
        private string __DisallowedCountryCode;
        #endregion //Internal properties

        public ServerListViewModel(IApplicationServices appServices,
                                   IAppNavigationService navigationService, 
                                   IService service,
                                   MainViewModel mainViewModel
        )
        {
            __AppServices = appServices;
            __NavigationService = navigationService;
            Service = service;
            __MainViewModel = mainViewModel;
            __MainViewModel.PropertyChanged += MainViewModel_PropertyChanged;

            ReturnBackCommand = new RelayCommand(ReturnBack);
            SelectServerCommand = new RelayCommand<ServerLocation>(SelectServer);
            SelectAutomaticServerSelectionCommand = new RelayCommand(SelectAutomaticServerSelection);
            ConfigureAutomaticServerSelectionCommand = new RelayCommand(ConfigureAutomaticServerSelection);
        }
        
        public void SetAutomaticServerSelection(ServerSelectionType serverSelectionType)
        {
            foreach (var s in Service.Servers.ServersList)
                s.IsSelected = false;

            IsAutomaticServerSelected = true;
            ServerSelectionType = serverSelectionType;
        }

        public void SetSelectedServer(ServerLocation server, ServerSelectionType serverSelectionType)
        {
            foreach (var s in Service.Servers.ServersList)
                s.IsSelected = false;

            if (server != null)
            {
                var serverToSelect = Service.Servers.ServersList.FirstOrDefault(s => s.VpnServer.GatewayId == server.VpnServer.GatewayId);
                if (serverToSelect != null)
                    serverToSelect.IsSelected = true;
            }

            ServerSelectionType = serverSelectionType;
            IsAutomaticServerSelected = false;
        }

        #region Properties
        public IService Service { get; }

        public bool IsAutomaticServerSelected
        {
            get => __IsAutomaticServerSelected;
            private set
            {
                RaisePropertyWillChange();
                __IsAutomaticServerSelected = value;
                RaisePropertyChanged();
            }
        }

        public ServerSelectionType ServerSelectionType
        {
            get => __ServerSelectionType;
            private set
            {
                RaisePropertyWillChange();

                __ServerSelectionType = value;
                DisallowedCountryCode = GetDisallowedCountryCode();

                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// For Multi-Hop - it is not possible to select Entry and Exit servers from same country
        /// </summary>
        public string DisallowedCountryCode
        {
            get => __DisallowedCountryCode;
            set
            {
                RaisePropertyWillChange();

                __DisallowedCountryCode = value;

                foreach (var server in Service.Servers.ServersList)
                    server.IsCountryDisallowed = server.CountryCode == __DisallowedCountryCode;

                RaisePropertyChanged();
            }
        }
        #endregion //Properties

        #region Navigation
        public ICommand ReturnBackCommand { get; }

        public ICommand SelectServerCommand { get; }

        public ICommand SelectAutomaticServerSelectionCommand { get; }

        public ICommand ConfigureAutomaticServerSelectionCommand { get; }

        private void ReturnBack()
        {
            __NavigationService.NavigateToMainPage(NavigationAnimation.FadeToRight);
        }

        public void Navigated() 
        {
            Service.Servers.StartPingUpdate();
        }
        #endregion // Navigation

        #region Private methods
        private void MainViewModel_PropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) 
            {
                case "SelectedServer":
                case "SelectedExitServer":
                    DisallowedCountryCode = GetDisallowedCountryCode();  
                    break;
            }
        }
        
        private void SelectServer(ServerLocation serverLocation)
        {
            if (serverLocation.CountryCode == DisallowedCountryCode) 
            {
                NotifyError (__AppServices.LocalizedString ("Mesage_CannotSelectServerSameCountry"),
                             __AppServices.LocalizedString ("Mesage_CannotSelectServerSameCountry_Description"));
                
                return;
            }
            
            __NavigationService.ServerLocationSelected(serverLocation);
        }

        private void SelectAutomaticServerSelection()
        {
            __NavigationService.ServerLocationSelectedAutomatic();
        }

        private void ConfigureAutomaticServerSelection()
        {
            __NavigationService.NavigateToAutomaticServerConfiguration();
        }

        /// <summary>
        /// For Multi-Hop - it is not possible to select Entry and Exit servers from same country
        /// </summary>
        private string GetDisallowedCountryCode()
        {
            switch (ServerSelectionType) 
            {
                case ServerSelectionType.SingleServer:
                    return "";

                case ServerSelectionType.EntryServer:
                    
                    if (__MainViewModel.SelectedExitServer == null)
                        return "";

                    return __MainViewModel.SelectedExitServer.CountryCode;
                
                case ServerSelectionType.ExitServer:
                    if (__MainViewModel.SelectedServer == null)
                        return "";

                    return __MainViewModel.SelectedServer.CountryCode;
            }
            return "";
        }
        #endregion //Private methods

    }
}
