using System.Collections.Generic;
using System.Linq;
using IVPN.WiFi;

namespace IVPN.Models.Configuration
{
    /// <summary>
    /// Trusted\Untrusted WiFi actions configuration
    /// </summary>
    public class NetworkActionsConfig : ModelBase
    {
        // event in use for macOS implementation
        public delegate void OnNetworkActionsChangedDelegate();
        public event OnNetworkActionsChangedDelegate OnNetworkActionsChanged = delegate { };

        /// <summary> Is it necessary to connect VPN when connected to un-trusted WiFi </summary>
        public bool UnTrustedConnectToVPN
        {
            get => __UnTrustedConnectToVPN;
            set
            {
                DoPropertyWillChange();
                __UnTrustedConnectToVPN = value;
                DoPropertyChanged();
            }
        }
        private bool __UnTrustedConnectToVPN = true;

        /// <summary> Is it necessary to enable KillSwitch when connected to un-trusted WiFi </summary>
        public bool UnTrustedEnableKillSwitch
        {
            get => __UnTrustedEnableKillSwitch;
            set
            {
                DoPropertyWillChange();
                __UnTrustedEnableKillSwitch = value;
                DoPropertyChanged();
            }
        }
        private bool __UnTrustedEnableKillSwitch = true;

        /// <summary> Is it necessary to disconnect from VPN when connected to trusted WiFi </summary>
        public bool TrustedDisconnectFromVPN
        {
            get => __TrustedDisconnectFromVPN;
            set
            {
                DoPropertyWillChange();
                __TrustedDisconnectFromVPN = value;
                DoPropertyChanged();
            }
        }
        private bool __TrustedDisconnectFromVPN = true;

        /// <summary> Is it necessary to disable KillSwitch when connected to trusted WiFi </summary>
        public bool TrustedDisableKillSwitch
        {
            get => __TrustedDisableKillSwitch;
            set
            {
                DoPropertyWillChange();
                __TrustedDisableKillSwitch = value;
                DoPropertyChanged();
            }
        }
        private bool __TrustedDisableKillSwitch = true;

        /// <summary> Default behavior when connected to WiFi and there is no defined action in configuration for this network </summary>
        public WiFiActionTypeEnum DefaultActionType
        {
            get => __DefaultActionType;
            set
            {
                DoPropertyWillChange();
                __DefaultActionType = value;
                DoPropertyChanged();
            }
        } 

        private WiFiActionTypeEnum __DefaultActionType;

        public List<NetworkAction> Actions
        {
            set
            {
                __Actions = value;
                RemoveDuplicatedActions();

                OnNetworkActionsChanged();
            }

            get => __Actions ?? (__Actions = new List<NetworkAction>());
        }
        private List<NetworkAction> __Actions;
        
        public NetworkActionsConfig()
        {
            DefaultActionType = WiFiActionTypeEnum.None;
        }

        public WiFiActionTypeEnum GetActionForNetwork(WiFiNetwork network)
        {
            foreach (NetworkAction action in Actions)
            {
                if (action.Network.Equals(network))
                    return action.Action;
            }

            return WiFiActionTypeEnum.Default;
        }

        public void SetActionForNetwork(WiFiNetwork network, WiFiActionTypeEnum action)
        {
            if (network == null || string.IsNullOrEmpty(network.SSID))
                return;
            
            foreach (NetworkAction netAction in Actions)
            {
                if (netAction.Network.Equals(network))
                {
                    netAction.Action = action;
                    return;
                }
            }

            // network was not found
            Actions.Add(new NetworkAction(network, action));
            OnNetworkActionsChanged();
        }

        private void RemoveDuplicatedActions()
        {
            // remove network definitios which are not in use
            List<NetworkAction> tmpList = Actions.Where(x => x.Action != WiFiActionTypeEnum.Default).ToList();

            Actions.Clear();

            // remove duplicate networks definition (if they are)
            HashSet<WiFiNetwork> hashedNetworks = new HashSet<WiFiNetwork>();
            foreach (var action in tmpList)
            {
                if (hashedNetworks.Contains(action.Network))
                    continue;
                Actions.Add(action);
                hashedNetworks.Add(action.Network);
            }
        }

        #region Serialization helpers
        public string Serialize()
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(this);
            }
            catch
            {
                Logging.Info("ERROR: Failed to serialize NetworkActions");
                return "";
            }
        }

        public static NetworkActionsConfig Deserialize(string serializedData)
        {
            if (string.IsNullOrEmpty(serializedData))
                return null;

            try
            {
                NetworkActionsConfig ret = Newtonsoft.Json.JsonConvert.DeserializeObject<NetworkActionsConfig>(serializedData);

                // erase rubbish from actions list
                ret.RemoveDuplicatedActions();

                return ret;
            }
            catch
            {
                Logging.Info("ERROR: Failed to de-serialize NetworkActions");
                return null;
            }
        }
        #endregion //Serialization helpers

        #region Type definitions
        public enum WiFiActionTypeEnum
        {
            None,
            Default,
            Untrusted,
            Trusted
        }

        /// <summary>
        /// Configuration for one WiFi
        /// </summary>
        public class NetworkAction : ModelBase
        {
            public WiFiNetwork Network { get; }

            public WiFiActionTypeEnum Action
            {
                get => __Action;
                set
                {
                    DoPropertyWillChange();
                    __Action = value;
                    DoPropertyChanged();
                }
            }
            private WiFiActionTypeEnum __Action;

            public NetworkAction(WiFiNetwork network, WiFiActionTypeEnum action = WiFiActionTypeEnum.Default)
            {
                Network = network;
                Action = action;
            }
        }
        #endregion //Type definitions
    }
}
