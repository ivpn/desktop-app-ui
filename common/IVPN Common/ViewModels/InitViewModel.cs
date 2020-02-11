using System;
using System.Threading.Tasks;
using System.Windows.Input;
using IVPN.Exceptions;
using IVPN.Models;
using IVPN.Interfaces;
using IVPN.Models.Configuration;

namespace IVPN.ViewModels
{
    public class InitViewModel : ViewModelBase
    {
        private bool __IsServiceError;
        private bool __IsHideStandardErrorDescription;
        private string __ServiceErrorCaption;
        private string __ServiceError;

        private bool __IsFailedToLoadServers;

        private bool __IsInProgress;
        private string __ProgressMessage;

        private bool __IsInitializing;

        private readonly AppState __AppState;
        private readonly Service __Service;
        private readonly AppSettings __Settings;
        private readonly IApplicationServices __AppServices;
        private readonly IAppNavigationService __NavigationService;

        public InitViewModel(
            AppState appState,
            IApplicationServices appServices,
            IAppNavigationService navigationService,
            Service service,
            AppSettings settings)
        {
            __AppState = appState;
            __AppServices = appServices;
            __NavigationService = navigationService;
            __Service = service;
            __Settings = settings;

            RetryCommand = new RelayCommand(RetryConnection);

            __IsInProgress = true;
            __ProgressMessage = __AppServices.LocalizedString("Initializing");

            __Service.ServiceExceptionRaised += ServiceExceptionRaised;
            __Service.ServiceDisconnected += ServiceDisconnected;
        }

        private void SetError(string errorCaption, string errorMessage, bool isHideStandardErrorDescription = false)
        {
            IsServiceError = true;
            IsHideStandardErrorDescription = isHideStandardErrorDescription;
            ServiceError = errorMessage;
            ServiceErrorCaption = errorCaption;
        }

        private void ClearError()
        {
            IsServiceError = false;
            ServiceError = "";
            ServiceErrorCaption = "";
        }

        private async void RetryConnection()
        {
            ClearError();

            await InitializeAsync();
        }

        public async Task InitializeAsync()
        {
            try
            {
                __IsInitializing = true;
                IsFailedToLoadServers = false;

                if (!await StartServiceAsync())
                    return;

                var port = await AttachToService();
                if (port == 0)
                    return;

                try
                {
                    await ConnectToServiceAsync(port);
                }
                catch (Exception ex)
                {
                    Logging.Info(string.Format("{0}", ex));
                    ServiceExceptionRaised(__Service, ex);
                    return;
                }

                if (__Service.IsConnectedToService)
                    UpdateServicePreferences();
            }
            finally
            {
                __IsInitializing = false;
            }
        }

        private void UpdateServicePreferences()
        {
            __Service.Proxy.SetPreference("enable_logging", __Settings.IsLoggingEnabled ? "1" : "0");
            __Service.Proxy.SetPreference("enable_obfsproxy", __Settings.ServiceUseObfsProxy ? "1" : "0");
            __Service.Proxy.SetPreference("is_stop_server_on_client_disconnect", __Settings.StopServerOnClientDisconnect ? "1" : "0");
        }

        private async Task<int> AttachToService()
        {
            ProgressMessage = __AppServices.LocalizedString("Connecting_Service_Progress");
            IsInProgress = true;

            ServiceAttachResult result = await __AppServices.AttachToService();

            ProgressMessage = "";
            IsInProgress = false;

            if (result.IsError)
            {
                SetError(__AppServices.LocalizedString("ErrorCaption_IVPNServiceCouldNotStart"), result.ErrorMessage);
                return 0;
            }

            return result.Port;
        }

        private async Task ConnectToServiceAsync(int port)
        {
            ClearError();

            ProgressMessage = __AppServices.LocalizedString("Connecting_Service_Progress");
            IsInProgress = true;

            try
            {
                await __Service.InitializeAsync(port);
            }
            finally
            {
                IsInProgress = false;
            }

            if (__Service.State != ServiceState.Uninitialized) 
            {
                // If username and password are defined - do not show LogIn View
                if (!__AppState.IsAuthenticated())
                    __NavigationService.NavigateToLogInPage (NavigationAnimation.FadeToLeft);
                else
                    __NavigationService.NavigateToMainPage (NavigationAnimation.FadeToLeft);
            }
        }

        private async Task<bool> StartServiceAsync()
        {
            // Check is service installed (applicable for Windows implementation)
            if (Platform.IsWindows && !__AppServices.IsServiceExists)
            {
                ProgressMessage = "";
                IsInProgress = false;
                
                SetError(__AppServices.LocalizedString("ErrorCaption_IVPNServiceCouldNotStart"),
                    "IVPN Windows service was not found on computer. Please, reinstall the application.",
                    isHideStandardErrorDescription: true);

                return false;
            }

            // Check is service started
            if (!__AppServices.IsServiceStarted)
            {
                ProgressMessage = __AppServices.LocalizedString("Starting_Service_Progress");
                IsInProgress = true;

                ServiceStartResult result = await __AppServices.StartService();

                ProgressMessage = "";
                IsInProgress = false;

                if (result.IsError)
                {
                    SetError(__AppServices.LocalizedString("ErrorCaption_IVPNServiceCouldNotStart"), result.ErrorMessage);

                    return false;
                }
            }

            return true;
        }
        
        private void ServiceExceptionRaised(Service sender, Exception exception)
        {
            if (__AppServices.IsExiting)
                return;

            // Crash application if client was already initialized
            if (__Service.State != ServiceState.Uninitialized)
                throw exception;

            if (exception is ServersNotLoaded)
                IsFailedToLoadServers = true;

            SetError(
                __AppServices.LocalizedString("ErrorCaption_CannotConnectoToService"),
                exception.Message
            );

            if (!__IsInitializing) // Do not navigate to InitPage. We already here.
                __NavigationService.NavigateToInitPage(NavigationAnimation.FadeToRight);
        }

        private void ServiceDisconnected(object sender, EventArgs e)
        {
            if (__AppServices.IsExiting)
                return;

            if (__Service.State != ServiceState.Uninitialized)
                throw new Exception("Client is in the invalid state");

            SetError(
                __AppServices.LocalizedString("ErrorCaption_CannotConnectoToService"),
                __AppServices.LocalizedString("Error_ConnectionClosed")
            );

            if (!__IsInitializing) // Do not navigate to InitPage. We already here.
                __NavigationService.NavigateToInitPage(NavigationAnimation.FadeToRight);
        }

        public ICommand RetryCommand { get; }

        public string ServiceErrorCaption
        {
            get => __ServiceErrorCaption;
            private set
            {
                RaisePropertyWillChange();
                __ServiceErrorCaption = value;
                RaisePropertyChanged();
            }
        }

        public string ServiceError
        {
            get => __ServiceError;
            private set
            {
                RaisePropertyWillChange();
                __ServiceError = value;
                RaisePropertyChanged();
            }
        }

        public bool IsFailedToLoadServers
        {
            get => __IsFailedToLoadServers;
            private set
            {
                RaisePropertyWillChange();
                __IsFailedToLoadServers = value;
                RaisePropertyChanged();
            }
        }

        public bool IsServiceError
        {
            get => __IsServiceError;
            private set
            {
                RaisePropertyWillChange();
                __IsServiceError = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// When TRUE - do not show any standard predefined error description. Use only 'ServiceError' to show.
        /// </summary>
        public bool IsHideStandardErrorDescription
        {
            get => __IsHideStandardErrorDescription;
            private set
            {
                RaisePropertyWillChange();
                __IsHideStandardErrorDescription = value;
                RaisePropertyChanged();
            }
        }

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

        public string ProgressMessage
        {
            get => __ProgressMessage;
            private set
            {
                RaisePropertyWillChange();
                __ProgressMessage = value;
                RaisePropertyChanged();
            }
        }

        public Service Service => __Service;
    }
}