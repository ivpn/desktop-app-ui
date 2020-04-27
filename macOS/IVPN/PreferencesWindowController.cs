using System;
using System.Collections.Generic;
using System.ComponentModel;
using Foundation;
using AppKit;

using CoreGraphics;

using IVPN.ViewModels;
using IVPN.Models;
using IVPN.Models.Configuration;
using IVPNCommon.ViewModels;
using IVPN.GuiHelpers;

namespace IVPN
{
    public partial class PreferencesWindowController : AppKit.NSWindowController, ISynchronizeInvoke
    {
        private MainViewModel __MainViewModel;
        private ViewModelNetworksSettings __NetworksViewModel;
        private ViewModelWireguardSettings __WireguardSetingsViewModel;

        private NSView __CurrentSettingsView;
        private AppSettings __Settings;
        private List<IDisposable> __Observers;
        private Service __Service;

        #region Constructors

        // Called when created from unmanaged code
        public PreferencesWindowController(IntPtr handle) : base(handle)
        {
            Initialize();
        }
        
        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public PreferencesWindowController(NSCoder coder) : base(coder)
        {
            Initialize();
        }
        
        // Call to load from the XIB/NIB file
        public PreferencesWindowController(
            AppSettings settings, 
            Service service, 
            MainViewModel mainViewModel) : base("PreferencesWindow")
        {
            Initialize();

            __Service = service;
            
            SetMainViewModel(mainViewModel);
            SetSettings(settings);
        }
        
        // Shared initialization code
        void Initialize()
        {
            
        }

        #endregion

        public override void WindowDidLoad()
        {
            base.WindowDidLoad();

            GuiBtnLaunchAtLogin.IntValue = (__Settings.RunOnLogin) ? 1 : 0;
        }

        private void SetMainViewModel(MainViewModel mainViewModel)
        {
            __MainViewModel = mainViewModel;

            __WireguardSetingsViewModel = new ViewModelWireguardSettings(__MainViewModel, __Service, LocalizedStrings.Instance);
            __WireguardSetingsViewModel.OnError += __WireguardSetingsViewModel_OnError;
            WireguardSettingsViewModelObservable = new ObservableObject(__WireguardSetingsViewModel);

            __MainViewModel.WireguardKeysManager.OnStarted += WireguardKeysManager_OnStarted;
            __MainViewModel.WireguardKeysManager.OnStopped += WireguardKeysManager_OnStopped;
        }

        private void SetSettings(AppSettings settings)
        {
            __Settings = settings;
            __Settings.PropertyChanged += __Settings_PropertyChanged;

            __MainViewModel.AppState.OnSessionChanged += AppState_OnSessionChanged;

            Settings = new AppSettingsAdapter(__Settings);
            NetworkActions = new ObservableObject(__Settings.NetworkActions);

            __NetworksViewModel = new ViewModelNetworksSettings(__Settings.NetworkActions, this);
            NetworksViewModelObservable = new ObservableObject(__NetworksViewModel);

            __Observers = new List<IDisposable>();
            __Observers.Add(Settings.AddObserver(new NSString("ProxyTypeId"), NSKeyValueObservingOptions.New, (NSObservedChange e) => {                
                if (Window.FirstResponder is NSTextView)
                    UnfocusElement();
                }));

            __Observers.Add(Settings.AddObserver(new NSString("FirewallAllowLAN"), NSKeyValueObservingOptions.New, (NSObservedChange e) => {
                __MainViewModel.KillSwitchAllowLAN = __Settings.FirewallAllowLAN;
            }));

            __Observers.Add(Settings.AddObserver(new NSString("FirewallAllowLANMulticast"), NSKeyValueObservingOptions.New, (NSObservedChange e) => {
                __MainViewModel.KillSwitchAllowLANMulticast = __Settings.FirewallAllowLANMulticast;
            }));

            __Observers.Add(Settings.AddObserver(new NSString("ServiceUseObfsProxy"), NSKeyValueObservingOptions.New, (NSObservedChange e) => {
                __Service.Proxy.SetPreference("enable_obfsproxy", settings.ServiceUseObfsProxy ? "1" : "0");
            }));

            __Observers.Add(Settings.AddObserver(new NSString("IsLoggingEnabled"), NSKeyValueObservingOptions.New, (NSObservedChange e) => {
                __Service.Proxy.SetPreference("enable_logging", settings.IsLoggingEnabled ? "1" : "0");
            }));

            __Observers.Add(Settings.AddObserver(new NSString("FirewallTypeId"), NSKeyValueObservingOptions.New, (NSObservedChange e) => {
                bool isPersistent = settings.FirewallType == IVPNFirewallType.Persistent;
                __MainViewModel.KillSwitchIsPersistent = isPersistent;
            }));

			__Observers.Add (Settings.AddObserver (new NSString ("OpenVPNExtraParameters"), NSKeyValueObservingOptions.New, (NSObservedChange e) => {
                
                if (!Helpers.OpenVPN.OpenVPNConfigChecker.IsIsUserParametersAllowed(settings.OpenVPNExtraParameters, out string errorDesc))
                {                    
                    IVPNAlert.Show(//this.Window, - do not use window argument (alert will be possible to show when window closing)
                        __MainViewModel.AppServices.LocalizedString("OpenVPNParamsNotSupported", "Some OpenVPN additional parameters are not supported"),
                        errorDesc,
                        NSAlertStyle.Warning);
                    return;
                }

                __Service.Proxy.SetPreference ("open_vpn_extra_parameters", settings.OpenVPNExtraParameters );
			}));

            __Observers.Add(Settings.AddObserver(new NSString("StopServerOnClientDisconnect"), NSKeyValueObservingOptions.New, (NSObservedChange e) => {
                __Service.Proxy.SetPreference("is_stop_server_on_client_disconnect", __Settings.StopServerOnClientDisconnect ? "1" : "0");
            }));
        }

        void __Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => __Settings_PropertyChanged(sender, e));
                return;
            }

            if (Equals(e.PropertyName, nameof(__Settings.RunOnLogin)))
                GuiBtnLaunchAtLogin.IntValue = (__Settings.RunOnLogin) ? 1 : 0;

            if (Equals(e.PropertyName, nameof(__Settings.VpnProtocolType)))
                UpdateSelectedVpnProtocolTypeUI();
        }


        private void AppState_OnSessionChanged(SessionInfo sessionInfo)
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(()=> AppState_OnSessionChanged(sessionInfo));
                return;
            }

            // quick trick: to update WG settings UI - just simulate viewmodel change
            var tmp = WireguardSettingsViewModelObservable;
            WillChangeValue("WireguardSettingsViewModel");
            WireguardSettingsViewModelObservable = null;
            DidChangeValue("WireguardSettingsViewModel");

            WillChangeValue("WireguardSettingsViewModel");
            WireguardSettingsViewModelObservable = tmp;
            DidChangeValue("WireguardSettingsViewModel");
        }

        partial void OnLaunchAtLoginChanged(NSObject sender)
        {
            SaveLaunchAtLoginItem();
        }

        private async void SaveLaunchAtLoginItem()
        {
            try
            {
                bool isLaunhAtLogin = GuiBtnLaunchAtLogin.IntValue != 0;
                await System.Threading.Tasks.Task.Run(() =>
                {
                __Settings.RunOnLogin = isLaunhAtLogin;
                });
            }
            catch (Exception ex)
            {               
                // Check if macOS Mojave (10.14) or greater
                if (NSProcessInfo.ProcessInfo.OperatingSystemVersion.Major >= 10
                    || (NSProcessInfo.ProcessInfo.OperatingSystemVersion.Major == 10 && NSProcessInfo.ProcessInfo.OperatingSystemVersion.Minor >= 14))
                    IVPN.GuiHelpers.IVPNAlert.Show(Window, $"Failed to save '{GuiBtnLaunchAtLogin.Title}'", "Please, check IVPN privileges to access 'System Events':\n\nSystem Preferences -> Security & Privacy ->  Privacy -> Automation");
                else
                    IVPN.GuiHelpers.IVPNAlert.Show(Window, $"Failed to save '{GuiBtnLaunchAtLogin.Title}'", ex.Message);
            }
            finally
            {
                GuiBtnLaunchAtLogin.IntValue = (__Settings.RunOnLogin) ? 1 : 0;
            }
        }

        partial void OnFirewallTypeChanged (Foundation.NSObject sender)
        {
            if (GuiBtnFirewallTypeOnDemand.IntValue > 0)
                Settings.FirewallTypeId = (int)IVPNFirewallType.Manual;
            else
                Settings.FirewallTypeId = (int)IVPNFirewallType.Persistent;
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            if (Settings.FirewallTypeId == (int)IVPNFirewallType.Manual)
                GuiBtnFirewallTypeOnDemand.IntValue = 1;
            else
                GuiBtnFirewallTypeAlwaysOn.IntValue = 1;

            GuiProgressViewWireguardKeysGeneration.Hidden = true;
            UpdateSelectedVpnProtocolTypeUI();

            Window.Center();
            GuiPanelWireguardConfigDetails.WillClose += GuiPanelWireguardConfigDetails_WillClose;

            Toolbar.SelectedItemIdentifier = "generalSettings";
            SwitchSettings(GeneralSettingsView);

            InitDefaultNetworkActionsButton();
            UpdateNetworksButtons();

            __NetworksViewModel.OnNetworkActionsChanged += UpdateNetworksButtons;
        }

        [Export("windowWillClose:")]
        public virtual void WillClose(NSNotification notification)
        {
            GuiPanelWireguardConfigDetails?.Close();

            UnfocusElement(); // update bindings
            __Settings.NetworkActions.Actions = __NetworksViewModel.GetNetworkActionsInUse();
            __Settings.Save();

            __MainViewModel.ApplySettings();
        }

        private void UnfocusElement()
        {
            Window.MakeFirstResponder(null);
        }

        [Export("validateToolbarItem:")]
        public bool ValidateToolbarItem (NSToolbarItem item)
        {
            return true;
        }

        /// <summary>
        /// Function called by mainWindow when showing already created window
        /// </summary>
        public void ReloadSettings()
        {
            WillChangeValue("settings");
            WillChangeValue("NetworkActions");
            DidChangeValue("NetworkActions");
            DidChangeValue("settings");

            __NetworksViewModel.RequestNetworksRescan();
        }

        private void SwitchSettings(NSView newSettings)
        {
            UnfocusElement(); // update bindings

            bool firstTime = __CurrentSettingsView == null;

            if (__CurrentSettingsView != null)
                __CurrentSettingsView.RemoveFromSuperview();   

            CGRect newFrame = new CGRect(
                newSettings.Frame.X, 
                SettingsView.Frame.Height - newSettings.Frame.Height, 
                newSettings.Frame.Width, 
                newSettings.Frame.Height);
            
            newSettings.Frame = newFrame;

            SettingsView.AddSubview(newSettings);

            __CurrentSettingsView = newSettings;

            // Set new window height
            var newHeight = newSettings.Frame.Height + (Window.Frame.Height - SettingsView.Frame.Bottom);

            CGRect newRect = UIUtils.UpdateHeight(Window.Frame, (float)newHeight);
            Window.SetFrame(newRect, true, !firstTime);
        }


        partial void ShowConnectionSettings(NSObject sender)
        {
            SwitchSettings(ConnectionSettingsView);
        }

        partial void ShowDiagnosticsSettings(NSObject sender)
        {
            
        }

        partial void ShowFirewallSettings(NSObject sender)
        {
            SwitchSettings(FirewallSettingsView);
        }

        partial void ShowGeneralSettings(NSObject sender)
        {
            SwitchSettings(GeneralSettingsView);
        }

        partial void ShowOpenVPNSettings (NSObject sender)
        {
            SwitchSettings (OpenVPNSettings);
        }

        partial void ShowNetworksSettings(NSObject sender)
        {
            SwitchSettings(NetworksSettings);
        }

        partial void ShowAntitrackerSettings(NSObject sender)
        {
            SwitchSettings(AntiTrackerSettings);
        }

        partial void ShowDnsSettings(NSObject sender)
        {
            SwitchSettings(DnsSettings);
        }

        //strongly typed window accessor
        public new PreferencesWindow Window {
            get {
                return (PreferencesWindow)base.Window;
            }
        }

        [Export("settings")]
        public AppSettingsAdapter Settings { get; private set; }

        [Export("NetworkActions")]
        public ObservableObject NetworkActions { get; private set; }

        [Export("NetworksViewModel")]
        public ObservableObject NetworksViewModelObservable { get; private set; }

        [Export("WireguardSettingsViewModel")]
        public ObservableObject WireguardSettingsViewModelObservable { get; private set; }

        #region Selecting VPN protocol type
        private bool CheckIsPossibleToChangeVpnType(VpnProtocols.VpnType t)
        {
            if (__Settings.VpnProtocolType == t)
                return false;

            if (__MainViewModel.ConnectionState != ServiceState.Disconnected)
            {
                IVPNAlert.Show(LocalizedStrings.Instance.LocalizedString("Message_DisconnectToSwitchVPNProtocol"));
                return false;
            }

            return true;
        }

        partial void OnProtocolChangedOpenVPN(NSObject sender)
        {
            if (CheckIsPossibleToChangeVpnType(VpnProtocols.VpnType.OpenVPN))
                __Settings.VpnProtocolType = VpnProtocols.VpnType.OpenVPN;

            UpdateSelectedVpnProtocolTypeUI();
        }

        partial void OnProtocolChangedWireGuard(NSObject sender)
        {
            if (!__MainViewModel.AppState?.Session?.IsWireGuardKeysInitialized() ?? false)
            {
                GenerateWireguardKeysBtn(this);
                return;
            }

            DoChangeWireGuardProtocol();
        }

        private void DoChangeWireGuardProtocol()
        {
            if (CheckIsPossibleToChangeVpnType(VpnProtocols.VpnType.WireGuard) && (__MainViewModel.AppState?.Session?.IsWireGuardKeysInitialized() ?? false))
                __Settings.VpnProtocolType = VpnProtocols.VpnType.WireGuard;

            UpdateSelectedVpnProtocolTypeUI();
        }

        private bool __IsVpnProtocolChanging;
        private void UpdateSelectedVpnProtocolTypeUI(VpnProtocols.VpnType? vpnTypeToShow = null)
        {
            if (__IsVpnProtocolChanging)
                return;

            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => UpdateSelectedVpnProtocolTypeUI(vpnTypeToShow));
                return;
            }

            VpnProtocols.VpnType vpnType = __Settings.VpnProtocolType;
            if (vpnTypeToShow != null)
                vpnType = (VpnProtocols.VpnType)vpnTypeToShow;

            // remove OpenVPN Extra Parameters view
            for (int i = 0; i < Toolbar.VisibleItems.Length; i++)
            {
                if (Toolbar.VisibleItems[i] == OpenVpnViewExtraParameters)
                {
                    Toolbar.RemoveItem(i);
                    break;
                }
            }

            // remove all tab-items from 'VPN configurations' Tab
            if (GuiTabConfigOpenVPN.TabView != null)
                GuiProtocolsConfigTabView.Remove(GuiTabConfigOpenVPN);
            if (GuiTabConfigWireGuard.TabView != null)
                GuiProtocolsConfigTabView.Remove(GuiTabConfigWireGuard);

            if (vpnType == VpnProtocols.VpnType.WireGuard)
            {
                GuiBtnProtocolTypeOpenVPN.IntValue = 0;
                GuiBtnProtocolTypeWireGuard.IntValue = 1;
                GuiProtocolsConfigTabView.Add(GuiTabConfigWireGuard);
            }
            else
            {
                GuiBtnProtocolTypeOpenVPN.IntValue = 1;
                GuiBtnProtocolTypeWireGuard.IntValue = 0;
                GuiProtocolsConfigTabView.Add(GuiTabConfigOpenVPN);
                Toolbar.InsertItem(OpenVpnViewExtraParameters.Identifier, Toolbar.VisibleItems.Length);
            }
        }

        partial void OnGuiBtnOpenvpnTooltip(NSObject sender)
        {
            var popover = new NSPopover { Behavior = NSPopoverBehavior.Transient };
            NSViewController informationPopoverController = new NSViewController();
            informationPopoverController.View = GuiPanelOpenvpnTooltip;

            popover.ContentViewController = informationPopoverController;
            popover.Show(GuiButtonOpenvpnTooltip.Bounds, GuiButtonOpenvpnTooltip, NSRectEdge.MinYEdge);
        }

        partial void OnGuiBtnWireguardTooltip(Foundation.NSObject sender)
        {
            var popover = new NSPopover { Behavior = NSPopoverBehavior.Transient };
            NSViewController informationPopoverController = new NSViewController();
            informationPopoverController.View = GuiPanelWireguardTooltip;

            popover.ContentViewController = informationPopoverController;
            popover.Show(GuiBtnWireguardTooltip.Bounds, GuiBtnWireguardTooltip, NSRectEdge.MinYEdge);
        }

        #endregion //Selecting VPN protocol type

        #region WireGuard

        void __WireguardSetingsViewModel_OnError(string errorText, string errorDescription = "")
        {
            if (GuiPanelWireguardConfigDetails.IsVisible)
                IVPNAlert.Show(GuiPanelWireguardConfigDetails, errorText, errorDescription, NSAlertStyle.Warning);
            else
                IVPNAlert.Show(Window, errorText, errorDescription, NSAlertStyle.Warning);
        }

        partial void ShowWireguardConfigDetails(NSObject sender)
        {
            GuiPanelWireguardConfigDetails.MakeKeyAndOrderFront(this);
            GuiPanelWireguardConfigDetails.Center();
            NSApplication.SharedApplication.RunModalForWindow(GuiPanelWireguardConfigDetails);
        }

        void GuiPanelWireguardConfigDetails_WillClose(object sender, EventArgs e)
        {
            NSApplication.SharedApplication.StopModal();
        }

        partial void GenerateWireguardKeysBtn(NSObject sender)
        {
            UpdateSelectedVpnProtocolTypeUI(VpnProtocols.VpnType.WireGuard);
            InvokeOnMainThread(async () =>
            {
                try
                {
                    // Do not update protocol change on UI until operatoion finished 
                    __IsVpnProtocolChanging = true;

                    await __WireguardSetingsViewModel.RegenerateNewKeyAsync();
                }
                finally
                {
                    __IsVpnProtocolChanging = false;
                    UpdateSelectedVpnProtocolTypeUI(VpnProtocols.VpnType.WireGuard);
                    DoChangeWireGuardProtocol();
                }
            }); 
        }

        void WireguardKeysManager_OnStarted()
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => WireguardKeysManager_OnStarted());
                return;
            }

            GuiViewWireguardConfig.Hidden = true;
            GuiProgressViewWireguardKeysGeneration.Hidden = false;
            GuiProgressWireguardKeysGeneration.StartAnimation(this);
            GuiWireguardConfigDetailsProgressIndicator.StartAnimation(this);

            GuiPanelWireguardConfigDetails.ContentView = GuiWireguardConfigDetailsViewProgress;
        }

        void WireguardKeysManager_OnStopped()
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => WireguardKeysManager_OnStopped());
                return;
            }

            GuiViewWireguardConfig.Hidden = false;
            GuiProgressViewWireguardKeysGeneration.Hidden = true;
            GuiProgressWireguardKeysGeneration.StopAnimation(this);
            GuiWireguardConfigDetailsProgressIndicator.StopAnimation(this);

            GuiPanelWireguardConfigDetails.ContentView = GuiWireguardConfigDetailsView;
        }

        #endregion //WireGuard

        #region trusted\untrusted networks settings
        partial void SetAllNetworksToDefaultAction(NSObject sender)
        {
            NSAlert alert = new NSAlert();
            alert.AlertStyle = NSAlertStyle.Informational;

            alert.AddButton(LocalizedStrings.Instance.LocalizedString("Button_Yes"));
            alert.AddButton(LocalizedStrings.Instance.LocalizedString("Button_Cancel"));
            alert.MessageText = LocalizedStrings.Instance.LocalizedString("Meggase_ResetAllNetworkActionsCaption");
            alert.InformativeText = LocalizedStrings.Instance.LocalizedString("Meggase_ResetAllNetworkActionsText");

            NSRunningApplication.CurrentApplication.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows);

            if (alert.RunModal() == 1000)
            {
                __NetworksViewModel.ResetToDefaultsCommand.Execute(null);
                ReloadSettings();
                InitDefaultNetworkActionsButton();
            }
        }

        private void InitDefaultNetworkActionsButton()
        {
            NSMenuItem menuUntrusted = new NSMenuItem("", OnNetworksDefaultAction_Changed)
            {
                AttributedTitle = AttributedString.Create(LocalizedStrings.Instance.LocalizedString("NetworkConfig_Untrusted"), NSColor.SystemRedColor, NSTextAlignment.Left),
                Tag = (int)NetworkActionsConfig.WiFiActionTypeEnum.Untrusted
            };

            NSMenuItem menuTrusted = new NSMenuItem("", OnNetworksDefaultAction_Changed)
            {
                AttributedTitle = AttributedString.Create(LocalizedStrings.Instance.LocalizedString("NetworkConfig_Trusted"), NSColor.SystemBlueColor, NSTextAlignment.Left),
                Tag = (int)NetworkActionsConfig.WiFiActionTypeEnum.Trusted
            };

            NSMenuItem menuNoAction = new NSMenuItem("", OnNetworksDefaultAction_Changed)
            {
                AttributedTitle = AttributedString.Create(LocalizedStrings.Instance.LocalizedString("NetworkConfig_NoAction"), NSColor.SystemGrayColor, NSTextAlignment.Left),
                Tag = (int)NetworkActionsConfig.WiFiActionTypeEnum.None
            };

            NetworksDefaultActionBtn.Menu.RemoveAllItems();
            NetworksDefaultActionBtn.Menu.AddItem(menuUntrusted);
            NetworksDefaultActionBtn.Menu.AddItem(menuTrusted); 
            NetworksDefaultActionBtn.Menu.AddItem(menuNoAction);

            if (NetworksDefaultActionBtn.Menu.Delegate == null)
                NetworksDefaultActionBtn.Menu.Delegate = new MenuDelegateInvertHighlitedItem();
            
            NetworksDefaultActionBtn.SelectItemWithTag((int)__Settings.NetworkActions.DefaultActionType);
        }

        void OnNetworksDefaultAction_Changed(object sender, EventArgs e)
        {
            NSMenuItem menuItem = sender as NSMenuItem;
            if (menuItem == null)
                return;

            __Settings.NetworkActions.DefaultActionType = (NetworkActionsConfig.WiFiActionTypeEnum)(int)menuItem.Tag;
        }

        private NSView __NetworksListView;
        private void UpdateNetworksButtons()
        {
            if (NetworksView == null)
                return;

            ViewStacker stacker = new ViewStacker();
            List<NetworkActionsConfig.NetworkAction> networks = new List<NetworkActionsConfig.NetworkAction>(__NetworksViewModel.AllNetworkActions);

            foreach (var network in networks)
            {
                NetworkActionButton btn = new NetworkActionButton(network, __NetworksViewModel, (float)NetworksView.EnclosingScrollView.ContentSize.Width);
                stacker.Add(btn);
            }

            var newView = stacker.CreateView();

            nfloat minViewHeight = NetworksView.EnclosingScrollView.ContentSize.Height;
            if (minViewHeight > newView.Frame.Height) // height can not be smaller than EnclosingScrollView height
                newView.Frame = new CGRect(newView.Frame.X, newView.Frame.Y, newView.Frame.Width, minViewHeight);
            
            NetworksView.Frame = newView.Frame;

            if (__NetworksListView == null)
                NetworksView.AddSubview(newView);
            else
                NetworksView.ReplaceSubviewWith(__NetworksListView, newView);
            
            NetworksView.ScrollPoint(new CGPoint(0, newView.Frame.Bottom));

            __NetworksListView = newView;
        }

        #endregion //#region trusted\untrusted networks settings

        #region ISynchronizeInvoke
        IAsyncResult ISynchronizeInvoke.BeginInvoke(Delegate method, object[] args)
        {
            BeginInvokeOnMainThread(() =>
            {
                ((Action)method)();
            });
            return null;
        }

        object ISynchronizeInvoke.EndInvoke(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        object ISynchronizeInvoke.Invoke(Delegate method, object[] args)
        {
            InvokeOnMainThread(() =>
            {
                ((Action)method)();
            });
            return null;
        }

        bool ISynchronizeInvoke.InvokeRequired
        {
            get
            {
                return !NSThread.IsMain;
            }
        }
        #endregion //ISynchronizeInvoke
    }
}

