using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using IVPN;
using IVPN.Models.Configuration;
using IVPN.ViewModels;
using IVPN.WiFi;

namespace IVPNCommon.ViewModels
{
    /// <summary>
    /// ViewModel for managing Trusted\Untrusted WiFi network actions 
    /// </summary>
    public class ViewModelNetworksSettings : ViewModelBase
    {
        // event in use for macOS implementation
        public delegate void OnNetworkActionsChangedDelegate();
        public event OnNetworkActionsChangedDelegate OnNetworkActionsChanged = delegate { };

        /// <summary> base configuration </summary>
        public NetworkActionsConfig NetworkActions { get; }

        public RelayCommand ResetToDefaultsCommand { get; }

        private readonly IWiFiWrapper __WiFiScanner;
        private readonly ISynchronizeInvoke __SyncInvoke;

        readonly object __NetworksListLocker = new object();

        /// <summary> all visible networks for user </summary>
        public ObservableCollection<NetworkActionsConfig.NetworkAction> AllNetworkActions
        {
            get => __AllNetworkActions;
            private set
            {
                RaisePropertyWillChange();
                __AllNetworkActions = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<NetworkActionsConfig.NetworkAction> __AllNetworkActions = new ObservableCollection<NetworkActionsConfig.NetworkAction>();

        /// <summary>
        /// false - will be shown only current connected network and networks from settings
        /// true - show all available networks
        /// </summary>
        public bool IsShowAllNetworks
        {
            get => __IsShowAllNetworks;
            set
            {
                RaisePropertyWillChange();

                __IsShowAllNetworks = value;
                RequestNetworksRescan();

                RaisePropertyChanged();
            }
        }
        private bool __IsShowAllNetworks;

        /// <summary>
        /// False if WiFi functionality disabled (e.g. when Windows WLAN service is not started)
        /// </summary>
        public bool IsWiFiFunctionalityOn => __WiFiScanner!=null;

        public ViewModelNetworksSettings(NetworkActionsConfig networkActions, ISynchronizeInvoke syncInvoke)
        {
            __SyncInvoke = syncInvoke;
            NetworkActions = networkActions;
            NetworkActions.OnNetworkActionsChanged += NetworkActionsSettings_OnActionsListChanged;

            ResetToDefaultsCommand = new RelayCommand(ResetToDefaults);

            __WiFiScanner = Platform.GetImplementation(Platform.PlatformImplementation.WiFi) as IWiFiWrapper;
            if (__WiFiScanner != null)
            {
                __WiFiScanner.Initialize();
                __WiFiScanner.OnNetworksScanComplete += () => { AddNewNetworksFound(__WiFiScanner.GetWifiNetworks()); };
            }

            RequestNetworksRescan();
        }

        public void RequestNetworksRescan()
        {
            if (__WiFiScanner == null)
                return;

            lock (__NetworksListLocker)
            {
                IEnumerable<NetworkActionsConfig.NetworkAction> configuredActions = null;
                if (AllNetworkActions != null)
                {
                    // keep previously configured networks
                    configuredActions = AllNetworkActions.Where(x => x.Action != NetworkActionsConfig.WiFiActionTypeEnum.Default);
                }

                AllNetworkActions = new ObservableCollection<NetworkActionsConfig.NetworkAction>(NetworkActions.Actions);

                AddActions(configuredActions);

                if (IsShowAllNetworks)
                {
                    // copy currently known networks
                    AddNewNetworksFound(__WiFiScanner.GetWifiNetworks(), true);

                    Task.Factory.StartNew(() =>
                    {
                        // start new networks scan
                        __WiFiScanner.StartWiFiNetworksScan();
                    });
                }
                else
                {
                    // copy only current connected network
                    WifiState state = __WiFiScanner.CurrentState;
                    if (state?.Network != null)
                        AddNewNetworksFound(new WiFiNetworkInfo[] { new WiFiNetworkInfo(state.Network) }, true);
                }
            }
        }

        public void ResetToDefaults()
        {
            lock (__NetworksListLocker)
            {
                NetworkActions.DefaultActionType = NetworkActionsConfig.WiFiActionTypeEnum.None;
                NetworkActions.TrustedDisableKillSwitch = true;
                NetworkActions.TrustedDisconnectFromVPN = true;
                NetworkActions.UnTrustedEnableKillSwitch = true;
                NetworkActions.UnTrustedConnectToVPN = true;

                foreach (NetworkActionsConfig.NetworkAction networkAction in AllNetworkActions)
                    networkAction.Action = NetworkActionsConfig.WiFiActionTypeEnum.Default;
            }
        }

        public List<NetworkActionsConfig.NetworkAction> GetNetworkActionsInUse()
        {
            lock (__NetworksListLocker)
            {
                List<NetworkActionsConfig.NetworkAction> ret = AllNetworkActions.ToList();
                ret.Sort((x, y) => String.Compare(x.Network.SSID, y.Network.SSID, StringComparison.Ordinal));
                return ret;
            }
        }

        private void AddNewNetworksFound(IEnumerable<WiFiNetworkInfo> networks, bool isCallChangedEventManually = false)
        {
            if (__WiFiScanner == null || networks == null)
                return;

            IEnumerable<WiFiNetworkInfo> networkInfos = networks as WiFiNetworkInfo[] ?? networks.ToArray();

            __SyncInvoke.BeginInvoke(new Action(() =>
            {
                if (!networkInfos.Any())
                {
                    if (isCallChangedEventManually)
                        OnNetworkActionsChanged();
                    return;
                }

                HashSet<WiFiNetwork> hashedNetworks = new HashSet<WiFiNetwork>(AllNetworkActions.Select(x => x.Network));

                bool isListChanged = false;
                lock (__NetworksListLocker)
                {
                    foreach (WiFiNetworkInfo networkInfo in networkInfos)
                    {
                        if (networkInfo?.Network == null || hashedNetworks.Contains(networkInfo.Network) || string.IsNullOrEmpty(networkInfo.Network.SSID))
                            continue;

                        if (IsShowAllNetworks == false)
                        {
                            // only possible to add current connected network
                            WifiState state = __WiFiScanner.CurrentState;
                            if (state?.Network == null || !networkInfo.Network.Equals(state.Network))
                                continue;
                        }

                        isListChanged = true;
                        AllNetworkActions.Add(new NetworkActionsConfig.NetworkAction(networkInfo.Network));
                        hashedNetworks.Add(networkInfo.Network);
                    }
                }

                if (isCallChangedEventManually || isListChanged)
                    OnNetworkActionsChanged();

            }), null);
        }

        private void AddActions(IEnumerable<NetworkActionsConfig.NetworkAction> actionsList)
        {
            if (actionsList == null)
                return;
            
            IEnumerable<NetworkActionsConfig.NetworkAction> actions = actionsList as NetworkActionsConfig.NetworkAction[] ?? actionsList.ToArray();
            if (!actions.Any())
                return;

            __SyncInvoke.BeginInvoke(new Action(() =>
            {
                HashSet<WiFiNetwork> hashedNetworks = new HashSet<WiFiNetwork>(AllNetworkActions.Select(x => x.Network));

                lock (__NetworksListLocker)
                {
                    foreach (NetworkActionsConfig.NetworkAction action in actions)
                    {
                        if (hashedNetworks.Contains(action.Network) || string.IsNullOrEmpty(action.Network.SSID))
                            continue;
                        
                        AllNetworkActions.Add(action);
                        hashedNetworks.Add(action.Network);
                    }
                }
            }), null);
        }

        /// <summary>
        /// Base actions list was changed (in settings)
        /// 
        /// We should use one instance of network action definition for each network.
        /// This is necesary for correct GUI view.
        /// 
        /// So, here we are removing old instances of actions repreentation or/and adding new.
        /// </summary>
        void NetworkActionsSettings_OnActionsListChanged()
        {
            lock (__NetworksListLocker)
            {
                List<NetworkActionsConfig.NetworkAction> newActions = new List<NetworkActionsConfig.NetworkAction>();

                bool isNetworkListChanged = false;
                foreach (NetworkActionsConfig.NetworkAction action in NetworkActions.Actions)
                {
                    if (!AllNetworkActions.Contains(action))
                    {
                        int oldNetworkIndex = AllNetworkActions.Select(x => x.Network).ToList().IndexOf(action.Network);
                        if (oldNetworkIndex > 0)
                        {
                            // we already have network representation but it is another instance: replace instance
                            AllNetworkActions.RemoveAt(oldNetworkIndex);
                            AllNetworkActions.Insert(oldNetworkIndex, action);
                        }
                        else
                            newActions.Add(action);

                        isNetworkListChanged = true;
                    }
                }

                foreach (var newAction in newActions)
                    AllNetworkActions.Add(newAction);

                if (isNetworkListChanged)
                    OnNetworkActionsChanged();
            }
        }
    }
}
