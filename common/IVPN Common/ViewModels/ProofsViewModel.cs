using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using IVPN.Exceptions;
using IVPN.Interfaces;
using IVPN.Lib;
using IVPNCommon.Api;

namespace IVPN.ViewModels
{
    public class ProofsViewModel : ViewModelBase, IOperationStartStopNotifier
    {
        #region Internal variables
        private readonly ILocalizedStrings __LocalizedStrings;
        private MainViewModel __MainViewModel;
        private CancellationTokenSource __CancellationTokenSource;
        private bool __IsUpdateInProgress;
        #endregion //Internal variables

        #region Public functionality
        public ICommand UpdateCommand { get; } 
        public event OnOperationExecutionEventDelegate OnWillExecute = delegate {};
        public event OnOperationExecutionEventDelegate OnDidExecute = delegate { };
        
        public ProofsViewModel(ILocalizedStrings localizedStrings)
        {
            __LocalizedStrings = localizedStrings;
            UpdateCommand = new RelayCommand(Update);
        }

        public void SetMainViewModel(MainViewModel mainViewModel)
        {
            if (mainViewModel == null)
                return;
            __MainViewModel = mainViewModel;
        }

        public ApiResponseGeoLookup GeoLookup
        {
            get => __GeoLookup;
            set
            {
                RaisePropertyWillChange();
                __GeoLookup = value;
                RaisePropertyChanged();
            }
        }
        private ApiResponseGeoLookup __GeoLookup;

        public string Error
        {
            get => __Error;
            set
            {
                RaisePropertyWillChange();
                __Error = value;
                if (!string.IsNullOrEmpty(__Error))
                    State = StateEnum.Error;
                RaisePropertyChanged();
            }
        }
        private string __Error;

        public enum StateEnum
        {
            None,
            Ok,
            Updating,
            Error,
        }

        public StateEnum State
        {
            get => __State;
            set
            {
                RaisePropertyWillChange();
                __State = value;
                RaisePropertyChanged();
            }
        }
        private StateEnum __State;

        #endregion Public functionality

        #region Private functionality
        private async void Update()
        {
            if (__IsUpdateInProgress)
                return;

            try
            {
                OnWillExecute(this);

                Error = null;
                GeoLookup = null;
                __IsUpdateInProgress = true;
                __CancellationTokenSource = new CancellationTokenSource();

                State = StateEnum.Updating;
                await ApiRequest(__CancellationTokenSource.Token);
            }
            finally
            {
                if (State != StateEnum.Error)
                {
                    State = GeoLookup == null ? StateEnum.None : StateEnum.Ok;
                }

                __IsUpdateInProgress = false;
                __CancellationTokenSource = null;
                OnDidExecute(this);
            }
        }

        private async Task ApiRequest(CancellationToken cancellationToken)
        {
            try
            {
                GeoLookup = await ApiServices.Instance.GeoLookup(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (TimeoutException)
            {
                if (__MainViewModel != null 
                    && __MainViewModel.IsKillSwitchEnabled 
                    && __MainViewModel.ConnectionState == Models.ServiceState.Disconnected)
                    Error = __LocalizedStrings.LocalizedString("Error_ApiRequestTimeoutBecauseFirewall");
                else
                    Error = __LocalizedStrings.LocalizedString("Error_ApiRequestTimeout");
                NotifyError(Error);
                return;
            }
            catch (WebException ex)
            {
                Logging.Info($"REST request exception : {ex}");
                if (__MainViewModel != null
                    && __MainViewModel.IsKillSwitchEnabled
                    && __MainViewModel.ConnectionState == Models.ServiceState.Disconnected)
                    Error = __LocalizedStrings.LocalizedString("Error_ApiRequestTimeoutBecauseFirewall");
                else
                    Error = __LocalizedStrings.LocalizedString("Error_RestServer_ConnectionError");

                NotifyError(
                    __LocalizedStrings.LocalizedString("Error_RestServer_ConnectionError_Title"),
                    Error);
                return;
            }
            catch (Exception ex)
            {
                Logging.Info($"REST request exception : {ex}");
                Error = __LocalizedStrings.LocalizedString("Error_RestServer_Communication");
                NotifyError(Error
                            + Environment.NewLine
                            + Environment.NewLine
                            + $"{IVPNException.GetDetailedMessage(ex)}");
                return;
            }
            /*
            if (result != ApiResponse.StatusType.OK)
            {
                NotifyError(__AppServices.LocalizedString("Error_ApiPasswordResetError"));
                return;
            }*/
        }
        #endregion Private functionality
    }
}
