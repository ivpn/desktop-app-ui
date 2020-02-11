using System;
using System.Threading.Tasks;
using IVPN.Interfaces;
using IVPN.Models;
using IVPN.Models.Configuration;

namespace IVPN.ViewModels
{
    public class ViewModelWireguardSettings : ViewModelBase
    {
        private readonly ILocalizedStrings __LocalizedStrings;
        private readonly MainViewModel __MainViewModel;
        private WireguardKeysManager KeysManager => __MainViewModel.WireguardKeysManager;

        public ViewModelWireguardSettings(MainViewModel mainViewModel, ILocalizedStrings localizedStrings)
        {
            __LocalizedStrings = localizedStrings;
            __MainViewModel = mainViewModel;
            Settings = __MainViewModel.AppState.Settings;

            KeysManager.OnStarted += () => { IsUpdateInProgress = true; };
            KeysManager.OnStopped += () => { IsUpdateInProgress = false; };

            Settings.PropertyChanged += (sender, e) =>
            {
                if (string.Equals(e.PropertyName, nameof(Settings.WireGuardKeysTimestamp))
                || string.Equals(e.PropertyName, nameof(Settings.WireGuardKeysRegenerationIntervalHours)))
                {
                    RaisePropertyWillChange(nameof(Generated));
                    RaisePropertyChanged(nameof(Generated));

                    RaisePropertyWillChange(nameof(ExpirationDate));
                    RaisePropertyChanged(nameof(ExpirationDate));

                    RaisePropertyWillChange(nameof(AutoRegenerationDate));
                    RaisePropertyChanged(nameof(AutoRegenerationDate));

                    RaisePropertyWillChange(nameof(RegenerationIntervalDays));
                    RaisePropertyChanged(nameof(RegenerationIntervalDays));

                    RaisePropertyWillChange(nameof(WireGuardClientInternalIp));
                    RaisePropertyChanged(nameof(WireGuardClientInternalIp));

                    RaisePropertyWillChange(nameof(WireGuardClientPublicKey));
                    RaisePropertyChanged(nameof(WireGuardClientPublicKey));
                }
            };
            
            KeysManager.OnProgress += (string progress) =>
            {
                ProgressStatus = progress;
            };
        }

        public AppSettings Settings { get; }
        
        private bool __IsUpdateInProgress;
        public bool IsUpdateInProgress
        {
            get => __IsUpdateInProgress;
            private set
            {
                RaisePropertyWillChange();
                __IsUpdateInProgress = value;
                RaisePropertyChanged();
            }
        }

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
        private string __ProgressStatus;

        public string Generated => DateToString(Settings.WireGuardKeysTimestamp, false);
        public string ExpirationDate => DateToString(Settings.WireGuardKeysTimestamp.AddDays(WireguardKeysManager.HardExpirationIntervalDays), false);
        public string AutoRegenerationDate => DateToString(KeysManager.KeysExpiryDate, true);

        public int RegenerationIntervalDays 
        {
            get => Settings.WireGuardKeysRegenerationIntervalHours / 24;
            set => Settings.WireGuardKeysRegenerationIntervalHours = value * 24;
        }

        public string WireGuardClientInternalIp => Settings.WireGuardClientInternalIp;
        public string WireGuardClientPublicKey => Settings.WireGuardClientPublicKey;

        public async Task RegenerateNewKeyAsync()
        {
            try
            {
                if (__MainViewModel.IsKillSwitchEnabled && __MainViewModel.ConnectionState == ServiceState.Disconnected)
                {
                    NotifyError(__LocalizedStrings.LocalizedString("WG_Message_KeyGenerationErrorFirewallIsOn", "IVPN firewall is enabled, disable it to generate WG key."));
                    return;
                }

                if (__MainViewModel.ConnectionState != ServiceState.Disconnected
                    && Settings.VpnProtocolType == VpnProtocols.VpnType.WireGuard)
                {
                    NotifyError(__LocalizedStrings.LocalizedString("WG_Message_DisconnectToRegenerateKeys", "To regenerate keys for WireGuard protocol, please disconnect from the VPN."));
                    return;
                }

                await KeysManager.GenerateNewKeysAsync();
            }
            catch (Exception ex)
            {
                if (ex is TimeoutException)
                    NotifyError(__LocalizedStrings.LocalizedString("WG_Error_FailedToInitializeKey", "Failed to initialize WireGuard keys"),
                        __LocalizedStrings.LocalizedString("Error_ApiRequestTimeout", "Operation timeout. Please check your internet connection and try again."));
                else
                    NotifyError(__LocalizedStrings.LocalizedString("WG_Error_FailedToInitializeKey", "Failed to initialize WireGuard keys"),
                        ex.Message);
            }
        }

        private static string DateToString(DateTime dateTime, bool canShowToday)
        {
#if BETA_WG_GENERATION_1MIN || BETA_WG_GENERATION_10MINS || BETA_WG_GENERATION_HOURS
#warning "!!!!!!!!!!!!!!!!!!! BETA_WG_GENERATION_XXX !!!!!!!!!!!!!!!!!!!!!!!"
            if (canShowToday && DateTime.Now.Date >= dateTime.Date)
                return "Today [" + dateTime.ToString("hh:mm") +"]";
            return dateTime.ToString("d/MM hh:mm");
#else
            if (canShowToday && DateTime.Now.Date >= dateTime.Date)
                return "Today";
            return dateTime.ToString("d MMM yyyy");
#endif
        }
    }
}
