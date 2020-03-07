using System;
using IVPN.Interfaces;
using IVPN.Models;
using IVPN.Models.Configuration;
using IVPN.ViewModels;

namespace IVPNCommon.ViewModels
{
    /// <summary>
    /// ViewModel for Firewall notification window
    /// 
    /// Notification window “Firewall is enabled” should be visible on display (topmost) in case when Firewall is enabled and VPN connection is OFF.
    /// </summary>
    public class FloatingOverlayWindowViewModel : ViewModelBase
    {
        #region Variables

        private readonly AppState __AppState;
        private readonly IService __Service;

        public string FirewallStatus
        {
            get => __FirewallStatus;
            set
            {
                RaisePropertyWillChange();
                __FirewallStatus = value;
                RaisePropertyChanged();
            }
        }
        private string __FirewallStatus;

        public string VpnStatus
        {
            get => __VPNStatus;
            set
            {
                RaisePropertyWillChange();
                __VPNStatus = value;
                RaisePropertyChanged();
            }
        }
        private string __VPNStatus;

        public bool Visible
        {
            get => __IsVisible;
            private set
            {
                RaisePropertyWillChange();
                __IsVisible = value;
                RaisePropertyChanged();
            }
        }
        private bool __IsVisible;

        public bool IsBlockingAllTraffic
        {
            get => __IsBlockingAllTraffic;
            private set
            {
                if (__IsBlockingAllTraffic == value)
                    return;

                RaisePropertyWillChange();
                __IsBlockingAllTraffic = value;
                RaisePropertyChanged();
            }
        }
        private bool __IsBlockingAllTraffic;

        public AppSettings Settings =>  AppSettings.Instance();
        private IApplicationServices __AppServices;

        public MainViewModel MainViewModel => __MainViewModel;
        private readonly MainViewModel __MainViewModel;

        private readonly Progress<string> __ConnectionProgress = new Progress<string>();

        private bool __IsPauseNotificationVisible;
        public bool IsPauseNotificationVisible
        {
            get => __IsPauseNotificationVisible;
            private set
            {
                RaisePropertyWillChange();
                __IsPauseNotificationVisible = value;
                RaisePropertyChanged();
            }
        }
        #endregion // variables

        public FloatingOverlayWindowViewModel(AppState appState, IService service, IApplicationServices appServices, MainViewModel mainViewModel)
        {
            __AppState = appState;
            __Service = service;
            __Service.PropertyChanged += (sender, e) => { ShowNotificationIfNecessary(); };

            __AppServices = appServices;

            __MainViewModel = mainViewModel;
            __MainViewModel.PropertyChanged += __MainViewModel_PropertyChanged;

            AppSettings.Instance().OnSettingsSaved += (sender, e) => { ShowNotificationIfNecessary(); };

            __ConnectionProgress.ProgressChanged += (object sender, string e) => { UpdateVPNStatus(e); };
            __Service.RegisterConnectionProgressListener(__ConnectionProgress);
        }

        public void Initialize()
        {
            ShowNotificationIfNecessary();
        }

        private void __MainViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(__MainViewModel.PauseStatus)))
            {
                UpdateVPNStatus();

                ShowNotificationIfNecessary();
            }
        }

        private void UpdateVPNStatus(string status = null)
        {
            if (__Service.State == ServiceState.Connected && __MainViewModel.PauseStatus == MainViewModel.PauseStatusEnum.Paused)
                VpnStatus = string.Format($"{__MainViewModel.PauseStatus}");
            else
                VpnStatus = (string.IsNullOrEmpty(status))? __Service.State.ToString() : status;
        }

        private void ShowNotificationIfNecessary()
        {
            if (__Service.KillSwitchIsEnabled)
            {
                if (__Service.State != ServiceState.Connected)
                {
                    FirewallStatus = "Blocking all traffic";
                    IsBlockingAllTraffic = true;
                }
                else
                {
                    FirewallStatus = "On";
                    IsBlockingAllTraffic = false;
                }
            }
            else
            {
                FirewallStatus = "Off";
                IsBlockingAllTraffic = false;
            }

            if (string.IsNullOrEmpty(VpnStatus) && __Service.State != ServiceState.Uninitialized)
                UpdateVPNStatus();

            // show notification only in case if:
            // - Firewall is enabled
            // - VPN connection is disabled
            bool isFirewallNotificationVisible;
            if (Settings.DisableFirewallNotificationWindow == false
                && __Service.KillSwitchIsEnabled && __Service.State != ServiceState.Connected)
                isFirewallNotificationVisible = true;
            else
                isFirewallNotificationVisible = false;

            if (MainViewModel.PauseStatus == MainViewModel.PauseStatusEnum.Paused)
                IsPauseNotificationVisible = true;
            else
                IsPauseNotificationVisible = false;

            if (isFirewallNotificationVisible || IsPauseNotificationVisible)
                Visible = true;
            else
                Visible = false;
        }
    }
}
