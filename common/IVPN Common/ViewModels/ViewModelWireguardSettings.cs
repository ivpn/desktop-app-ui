//
//  IVPN Client Desktop
//  https://github.com/ivpn/desktop-app-ui
//
//  Created by Stelnykovych Alexandr.
//  Copyright (c) 2020 Privatus Limited.
//
//  This file is part of the IVPN Client Desktop.
//
//  The IVPN Client Desktop is free software: you can redistribute it and/or
//  modify it under the terms of the GNU General Public License as published by the Free
//  Software Foundation, either version 3 of the License, or (at your option) any later version.
//
//  The IVPN Client Desktop is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
//  details.
//
//  You should have received a copy of the GNU General Public License
//  along with the IVPN Client Desktop. If not, see <https://www.gnu.org/licenses/>.
//

﻿using System;
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
        private readonly IService __Service;
        private WireguardKeysManager KeysManager => __MainViewModel.WireguardKeysManager;

        public ViewModelWireguardSettings(MainViewModel mainViewModel, IService service, ILocalizedStrings localizedStrings)
        {
            __Service = service;
            __LocalizedStrings = localizedStrings;
            __MainViewModel = mainViewModel;
            Settings = AppSettings.Instance();
                        
            KeysManager.OnStarted += () => { IsUpdateInProgress = true; };
            KeysManager.OnStopped += () => { IsUpdateInProgress = false; };

            __MainViewModel.AppState.OnSessionChanged += (SessionInfo sessionInfo) =>
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

        public string Generated => DateToString(__MainViewModel.AppState.Session?.WgKeyGenerated ?? default, false);
        public string ExpirationDate => DateToString((__MainViewModel.AppState.Session?.WgKeyGenerated ?? default).AddDays(WireguardKeysManager.HardExpirationIntervalDays), false);
        public string AutoRegenerationDate => DateToString(__MainViewModel.AppState.Session?.GetKeysExpiryDate() ?? default, true);

        public int RegenerationIntervalDays 
        {
            get => (__MainViewModel.AppState.Session?.WgKeyRotateInterval ?? default).Days;

            set
            {
                __Service.WireGuardKeysSetRotationInterval((Int64)TimeSpan.FromDays(value).TotalSeconds);                
            }
        }

        public string WireGuardClientInternalIp => __MainViewModel.AppState.Session?.WgLocalIP?.ToString() ?? "";
        public string WireGuardClientPublicKey => __MainViewModel.AppState.Session?.WgPublicKey ?? "";

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
            if (canShowToday && DateTime.Now.Date >= dateTime.Date)
                return "Today";
            return dateTime.ToString("d MMM yyyy");
        }
    }
}
