using IVPN.Models;
using IVPN.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using IVPN.Lib;
using IVPN.Models.Configuration;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IVPN.VpnProtocols;
using IVPNCommon.ViewModels;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using IVPN.Responses;

namespace IVPN.Windows
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged, ISynchronizeInvoke
    {
        private bool __IsDiagnosticsInProgress;
        private string __LogsProgressString;


        public Service Service { get; set; }
        public string AppVersion => Platform.Version;
        public AppState AppState {get; private set;}
        public ViewModelNetworksSettings NetworksSettings { get; }
        public ViewModelWireguardSettings WireGuardSettings { get; }

        private static SettingsWindow __Instance;
        public static void ShowSettingsWindow(Service service, ViewModelWireguardSettings wgSettingsModel)
        {
            CloseSettingsWindow();

            __Instance = new SettingsWindow(App.Settings, wgSettingsModel)
            {
                Service = service,
                Owner = Application.Current.MainWindow
            };

            __Instance.ShowDialog();
        }

        public static void CloseSettingsWindow()
        {
            __Instance?.Close();
            __Instance = null;
        }

        private SettingsWindow(AppSettings settings, ViewModelWireguardSettings wgSettingsModel)
        {
            InitializeComponent();
            Settings = settings;
            AppState = AppState.Instance();
            
            Settings.FreezeSettings(); // save corrent settings state (in order to have possibility to restore)

            // if WiFi functionality is disable - hide trusted\untrusted wifi configuration
            NetworksSettings = new ViewModelNetworksSettings(Settings.NetworkActions, this);
            if (NetworksSettings.IsWiFiFunctionalityOn == false)
                GuiTabItemNetworks.Visibility = Visibility.Collapsed;

            DataContext = this;

            switch (Settings.ProxyType)
            {
                case ProxyType.None:
                    rbProxyNone.IsChecked = true;
                    break;
                case ProxyType.Auto:
                    rbProxyAuto.IsChecked = true;
                    break;
                case ProxyType.Http:
                    rbProxyHTTP.IsChecked = true;
                    break;
                case ProxyType.Socks:
                    rbProxySOCKS.IsChecked = true;
                    break;
            }
            
            txtProxyPassword.Password = Settings.ProxyUnsafePassword;

            WireGuardSettings = wgSettingsModel;
            WireGuardSettings.OnError += WireGuardSettings_OnError;
            Closing += (sender, args) => { WireGuardSettings.OnError -= WireGuardSettings_OnError; };

            // WireGuard key regeneration interval
            for (int i = 1; i <= 30; i++)
                GuiComboBoxWgKeyRegenerateDays.Items.Add(i);
            GuiComboBoxWgKeyRegenerateDays.SelectedIndex = WireGuardSettings.RegenerationIntervalDays - 1;

            if (GuiComboBoxWgKeyRegenerateDays.SelectedIndex < 0)
                GuiComboBoxWgKeyRegenerateDays.SelectedIndex = 0;
            GuiComboBoxWgKeyRegenerateDays.SelectionChanged += (sender, args) => { WireGuardSettings.RegenerationIntervalDays = GuiComboBoxWgKeyRegenerateDays.SelectedIndex + 1; };

            Settings.PropertyChanged += Settings_OnPropertyChanged;
            Closing += (sender, args) => { Settings.PropertyChanged -= Settings_OnPropertyChanged; };
        }

        private void Settings_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, nameof(Settings.VpnProtocolType)))
                RaisePropertyChanged(nameof(VpnProtocolType));
        }

        private void WireGuardSettings_OnError(string errortext, string errordescription)
        {
            if (string.IsNullOrEmpty(errordescription))
            {
                errordescription = errortext;
                errortext = "Error";
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                var wgsettingsWnd = __WireguardSettingsWindow;
                MessageBox.Show(((wgsettingsWnd == null) ? this : (Window)wgsettingsWnd), 
                    errordescription, errortext, MessageBoxButton.OK, MessageBoxImage.Error);
            }));
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Service.Proxy.DiagnosticsGenerated -= Proxy_DiagnosticsGenerated;

            Settings.UnfreezeSettings(isRestoreFreezedState: true); // restore settings from freezed state (if old settings snapshot is still available)

            base.OnClosing(e);

            if (IsDiagnosticsInProgress)
                e.Cancel = true;
        }
        
        public VpnType VpnProtocolType
        {
            get => Settings.VpnProtocolType;
            set
            {
                if (Settings.VpnProtocolType == value)
                    return;

                var vm = ((App)Application.Current).GetMainViewModel();
                if (vm.ConnectionState != ServiceState.Disconnected)
                {
                    MessageBox.Show(
                        StringUtils.String("Message_DisconnectToSwitchVPNProtocol", "To switch VPN protocol, please disconnect from the VPN."),
                        StringUtils.String("Message_UnableToChangeVpnType", "Unable to change VPN type"), 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                Settings.VpnProtocolType = value;
                
                RaisePropertyChanged();
            }
        }

        public bool ApplySettings()
        {
            // check OpenVPN extra parameters defined bu user
            // Some parameters can be deprecated (e.g. parameters which can execute external command)
            if (!Helpers.OpenVPN.OpenVPNConfigChecker.IsIsUserParametersAllowed(Settings.OpenVPNExtraParameters, out var errorInfo))
            {
                MessageBox.Show(StringUtils.String("OpenVPNParamsNotSupported")
                    + Environment.NewLine 
                    + errorInfo
                    + Environment.NewLine + Environment.NewLine + StringUtils.String("CheckSettings"),

                    "Not supported OpenVPN extra parameters", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            UpdateProxyType();

            Settings.ProxySafePassword = CryptoUtil.EncryptString(txtProxyPassword.Password);
            Settings.NetworkActions.Actions = NetworksSettings.GetNetworkActionsInUse();

            Settings.UnfreezeSettings(isRestoreFreezedState: false); // forged about old freezed settings data
            Settings.Save(); // save current settings state
            
            // Notify service about changes in original settings
            DoUpdateServiceSettings(Settings);

            return true;
        }

        private void UpdateProxyType()
        {
            Settings.ProxyType = ProxyType.None;

            if (rbProxyAuto.IsChecked == true)
                Settings.ProxyType = ProxyType.Auto;
            else if (rbProxyNone.IsChecked == true)
                Settings.ProxyType = ProxyType.None;
            else if (rbProxyHTTP.IsChecked == true)
                Settings.ProxyType = ProxyType.Http;
            else if (rbProxySOCKS.IsChecked == true)
                Settings.ProxyType = ProxyType.Socks;

            RaisePropertyChanged(nameof(Settings));
        }

        private void txtProxyPort_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9]+").IsMatch(e.Text);
        }

        #region Diagnostic report
        private ErrorReporterEvent __DiagReport;
        private bool __DiagReportRequestToSend;
        private string __DiagReportUserComments;

        private void PreviewLogsButton_Click(object sender, RoutedEventArgs e)
        {
            RequestDiagnosticLogs(false);
        }

        private void SubmitLogsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowInputDialog(StringUtils.String("Diagnostics_SubmittingLogs_InputDescriptionText"),
                (string comments) =>
                {
                    LogsProgressString = StringUtils.String("Diagnostics_SubmittingLogs");
                    IsDiagnosticsInProgress = true;

                    if (__DiagReport == null)
                        RequestDiagnosticLogs(true, comments);
                    else
                        System.Threading.Tasks.Task.Run(() => SubmitDiagnosticReport(__DiagReport, comments));
                },
                StringUtils.String("Button_Send"),
                StringUtils.String("Button_Cancel")
            );
        }

        private void RequestDiagnosticLogs(bool requiredToSend, string userComments = null)
        {
            __DiagReport = null;
            __DiagReportRequestToSend = requiredToSend;
            __DiagReportUserComments = userComments;

            LogsProgressString = StringUtils.String("Diagnostics_GeneratingLogs");
            IsDiagnosticsInProgress = true;

            Service.Proxy.DiagnosticsGenerated -= Proxy_DiagnosticsGenerated; // ensure we are not subscribed multiple times
            Service.Proxy.DiagnosticsGenerated += Proxy_DiagnosticsGenerated;
            
            Service.Proxy.GenerateDiagnosticLogs(Settings.VpnProtocolType);
        }
        
        void Proxy_DiagnosticsGenerated(IVPNDiagnosticsGeneratedResponse diagInfoResponse)
        {
            try
            {
                __DiagReport = ErrorReporter.PrepareDiagReportToSend(Settings,
                    diagInfoResponse.EnvironmentLog,
                    diagInfoResponse.ServiceLog,
                    diagInfoResponse.ServiceLog0,
                    diagInfoResponse.OpenvpnLog,
                    diagInfoResponse.OpenvpnLog0);

                if (__DiagReportRequestToSend)
                {
                    SubmitDiagnosticReport(__DiagReport, __DiagReportUserComments);
                }
                else
                {
                    string logFile = Path.GetTempFileName();
                    StreamWriter writer = new StreamWriter(new FileStream(logFile, FileMode.Create));
                    writer.WriteLine(__DiagReport.ToString());
                    writer.Flush();
                    writer.Close();

                    Process p = Process.Start("notepad.exe", logFile);
                    if (p != null)
                    {
                        p.Exited += (object sender, EventArgs e) =>
                        {
                            try
                            {
                                File.Delete(logFile);
                            }
                            catch
                            {
                                // ignored
                            }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed to generate report", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            IsDiagnosticsInProgress = false;
        }
        
        private void SubmitDiagnosticReport(ErrorReporterEvent diagReport, string userComments)
        {
            string error = null;
            try
            {
                ErrorReporter.SendReport(diagReport, userComments);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Logging.Info($"[ERROR] Submiting diagnostic logs: {ex}");
            }
            finally
            {
                __DiagReport = null;

                IsDiagnosticsInProgress = false;
                if (string.IsNullOrEmpty(error))
                    App.TrayIconDiagnosticsSubmitted(true, "");
                else
                    App.TrayIconDiagnosticsSubmitted(false, error);
            }
        }
        #endregion // Diagnostic report

        private void DoUpdateServiceSettings(AppSettings settings)
        {
            if (Service == null)
                return;

            if (Service.State == ServiceState.Uninitialized)
                return;

            Service.Proxy.SetPreference("enable_logging", settings.IsLoggingEnabled ? "1" : "0");

            Service.Proxy.SetPreference("enable_obfsproxy", settings.ServiceUseObfsProxy ? "1" : "0");

            Service.Proxy.SetPreference("is_stop_server_on_client_disconnect", settings.StopServerOnClientDisconnect ? "1" : "0");

            Service.Proxy.SetPreference("connect_insecure_wifi", settings.ServiceConnectOnInsecureWifi ? "1" : "0");

            Service.Proxy.SetPreference("open_vpn_extra_parameters", settings.OpenVPNExtraParameters);

            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                var mainViewModel = mainWindow.MainViewModel;

                // Update Kill Switch with the IVPN Firewall settings
                if (mainViewModel != null)
                {
                    mainViewModel.KillSwitchAllowLANMulticast = settings.FirewallAllowLANMulticast;
                    mainViewModel.KillSwitchAllowLAN = settings.FirewallAllowLAN;                    
                    mainViewModel.KillSwitchIsPersistent = (settings.FirewallType == IVPNFirewallType.Persistent);
                }
            }
        }

        private void GuiButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current is App application)
            {
                if (application.Logout())
                    Close();
            }
        }

        private void OkClicked(object sender, RoutedEventArgs e)
        {
            if (ApplySettings())
                DialogResult = true;
        }

        public AppSettings Settings { get; }

        public bool IsDiagnosticsInProgress
        {
            get => __IsDiagnosticsInProgress;
            set
            {
                __IsDiagnosticsInProgress = value;
                RaisePropertyChanged();
            }
        }

        public string LogsProgressString
        {
            get => __LogsProgressString;
            set
            {
                __LogsProgressString = value;
                RaisePropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ProxyType_Changed(object sender, RoutedEventArgs e)
        {
            UpdateProxyType();
        }

        private void UpdateFirewallType(object sender, RoutedEventArgs e)
        {
            if (FirewallPersistent.IsChecked == true)
                Settings.FirewallType = IVPNFirewallType.Persistent;
            else
                Settings.FirewallType = IVPNFirewallType.Manual;

            if (Settings.FirewallType == IVPNFirewallType.Persistent)
            {
                ShowIVPNFirewallToolTipIfRequired();
            }
        }

        private FirewallAlwaysOnNotification __NotificationWindow;

        private void ShowIVPNFirewallToolTipIfRequired()
        {
            if (__NotificationWindow != null)
                return;

            if (!IsActive)
                return;

            if (Settings.IsIVPNAlwaysOnWarningDisplayed)
                return;

            Settings.IsIVPNAlwaysOnWarningDisplayed = true;
            
            Point relativePoint = FirewallPersistent.TransformToAncestor(this).Transform(new Point(0, 0));

            __NotificationWindow = new FirewallAlwaysOnNotification
            {
                ParentWindow = this,
                VerticalOffset = relativePoint.Y,
                HorizontalOffset = relativePoint.X
            };
            __NotificationWindow.Show();
            __NotificationWindow.Activate();
            __NotificationWindow.Closed += (snd, evt) => { __NotificationWindow = null; };
        }

        protected override void OnClosed(EventArgs e)
        {
            __NotificationWindow?.Close();
        }

        // In use for WPF binding to ComboBox
        public IList<NetworkActionsConfig.WiFiActionTypeEnum> NetworkPossibleDefaultActions => new List<NetworkActionsConfig.WiFiActionTypeEnum>
        {
            NetworkActionsConfig.WiFiActionTypeEnum.Untrusted,
            NetworkActionsConfig.WiFiActionTypeEnum.Trusted,
            NetworkActionsConfig.WiFiActionTypeEnum.None
        };
        // In use for WPF binding to ComboBox
        public IList<NetworkActionsConfig.WiFiActionTypeEnum> NetworkPossibleActions => new List<NetworkActionsConfig.WiFiActionTypeEnum>
        {
            NetworkActionsConfig.WiFiActionTypeEnum.Untrusted,
            NetworkActionsConfig.WiFiActionTypeEnum.Trusted,
            //NetworkActionsConfig.WiFiActionTypeEnum.None, // 'No action' available only for 'Default' action
            NetworkActionsConfig.WiFiActionTypeEnum.Default
        };

        private void GuiHyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }
        
        private void ShowInputDialog(string caption, Action <string> okOkAction, string okButtonText = null, string cancelButtonText = null )
        {
            GuiInputBoxCaptionText.Text = caption;

            if (!string.IsNullOrEmpty(okButtonText))
                GuiInputBoxYesButton.Content = okButtonText;
            if (!string.IsNullOrEmpty(cancelButtonText))
                GuiInputBoxNoButton.Content = cancelButtonText;

            GuiInputBoxYesButton.Command = new RelayCommand(() =>
            {
                GuiInputBox.Visibility = Visibility.Collapsed;
                okOkAction(GuiInputBoxText.Text);
            });
            GuiInputBoxNoButton.Command = new RelayCommand(() => { GuiInputBox.Visibility = Visibility.Collapsed; });

            GuiInputBoxText.Text = "";
            GuiInputBox.Visibility = Visibility.Visible;
            GuiInputBoxText.Focus();
        }

        private void GuiButtonNetworksSetAllToDefault_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show(this, StringUtils.String("Meggase_ResetAllNetworkActionsText"), StringUtils.String("Meggase_ResetAllNetworkActionsCaption"), MessageBoxButton.YesNo, MessageBoxImage.Question))
                NetworksSettings.ResetToDefaultsCommand.Execute(null);
        }

        #region ISynchronizeInvoke
        public IAsyncResult BeginInvoke(Delegate method, object[] args)
        {
            Dispatcher.BeginInvoke(method, args);
            return null;
        }

        public object EndInvoke(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public object Invoke(Delegate method, object[] args)
        {
           return Dispatcher.Invoke(method, args);
        }

        public bool InvokeRequired => true;

        #endregion // ISynchronizeInvoke

        private WireguardSettingsWindow __WireguardSettingsWindow;
        private void ButtonWireGuardConfig_Click(object sender, RoutedEventArgs e)
        {
            __WireguardSettingsWindow = new WireguardSettingsWindow(WireGuardSettings) { Owner = Application.Current.MainWindow };
            __WireguardSettingsWindow.ShowDialog();
            __WireguardSettingsWindow = null;
        }

        private async void ProtocolTypeBtn_Checked(object sender, RoutedEventArgs e)
        {            
            if (Settings.VpnProtocolType == VpnType.WireGuard && ! (AppState.Instance().Session?.IsWireGuardKeysInitialized() ?? false))
            {
                await WireGuardSettings.RegenerateNewKeyAsync();
            }
        }

        private void BtnOpenvpnPopup_Click(object sender, RoutedEventArgs e)
        {
            PopupOpenvpnInfo.IsOpen = true;
        }

        private void BtnWireguardPopup_Click(object sender, RoutedEventArgs e)
        {
            PopupWireguardInfo.IsOpen = true;
        }
    }
}
