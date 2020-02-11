using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using IVPN.Models;
using IVPN.Models.Configuration;
using IVPN.ViewModels;
using IVPN.Windows;

namespace IVPN.Views
{
    /// <summary>
    /// Interaction logic for wp_MainPage.xaml
    /// </summary>
    public partial class wp_MainPage : Page, INotifyPropertyChanged
    {
        public MainViewModel ViewModel { get; }
        private PrivateEmailsManagerViewModel __ViewModelPrivateEmails { get; }

        public ProofsViewModel ProofsViewModel { get; }

        public RelayCommand SelectServerCommand { get; }
        public RelayCommand SelectEntryServerCommand { get; }
        public RelayCommand SelectExitServerCommand { get;  }
        public RelayCommand PauseButtonPressedCommand { get; }

        public enum AnimationTypeEnum
        {
            None,
            Connecting,
            Connected,
            Disconnecting,
            Disconnected
        }

        private AnimationTypeEnum __AnimationType;
        public AnimationTypeEnum AnimationType
        {
            get
            {
                // do not show connected animation when window is not active (to reduce CPU load)
                if (!Application.Current.MainWindow.IsActive
                    && __AnimationType == AnimationTypeEnum.Connected)
                    return AnimationTypeEnum.None;
                return __AnimationType; 
            }
            set
            {
                if (__AnimationType == value)
                    return;
                __AnimationType = value;
                OnPropertyChanged();
            }
        }

        private string __ConnectButtonText;
        public string ConnectButtonText
        {
            get { return __ConnectButtonText; }
            set
            {
                if (string.Equals(__ConnectButtonText, value))
                    return;
                __ConnectButtonText = value;
                OnPropertyChanged();
            }
        }

        public wp_MainPage()
        {
            InitializeComponent();

            if (!(Application.Current.MainWindow is MainWindow mainWindow))
                throw new InvalidOperationException("App.Current.MainWindow as MainWindow == null");

            __ViewModelPrivateEmails = mainWindow.PrivateEmailsViewModel;
            ProofsViewModel = mainWindow.ProofsViewModel;

            ViewModel = mainWindow.MainViewModel;
            ViewModel.PropertyChanged+= ViewModelOnPropertyChanged;
            ViewModel.OnError += (text, description) =>
            {
                if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(description))
                {
                    if (text.Length < 60)
                        MessageBox.Show(description, text, MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        MessageBox.Show(text + Environment.NewLine + Environment.NewLine + description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                    MessageBox.Show(text + Environment.NewLine + description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            ViewModel.AppState.OnSessionStatusChanged += (info) => UpdateSessionInfo(info);
            ViewModel.OnAccountSuspended += ViewModelOnAccountSuspended;

            UpdateSessionInfo(ViewModel.AppState.SessionStatusInfo);

            Application.Current.MainWindow.Activated += MainWindowActivation;
            Application.Current.MainWindow.Deactivated += MainWindowActivation;

            DataContext = this;
            
            SelectServerCommand = new RelayCommand(() =>
                {
                    ViewModel.SelectServerCommand.Execute(null);
                });
            SelectEntryServerCommand = new RelayCommand(() =>
            {
                ViewModel.SelectEntryServerCommand.Execute(null);
            });
            SelectExitServerCommand = new RelayCommand(() =>
            {
                ViewModel.SelectExitServerCommand.Execute(null);
            });
            PauseButtonPressedCommand = new RelayCommand(() =>
            {
                GuiPauseMenuPopup.IsOpen = true;
            });

            // programically define location of 'Pause' menu PopUp menu (location relatively to PlacementTarget 'GuiPauseButton')
            GuiPauseMenuPopup.CustomPopupPlacementCallback = (size, targetSize, offset) => new[] { new CustomPopupPlacement(new Point(0, 0), PopupPrimaryAxis.Vertical) };
        }
        
        private void MainWindowActivation(object sender, EventArgs eventArgs)
        {
            OnPropertyChanged(nameof(AnimationType));
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (GuiUtils.IsInvokedInGuiThread(this, () => ViewModelOnPropertyChanged(sender, propertyChangedEventArgs)))
                return;

            if (propertyChangedEventArgs.PropertyName.Equals(nameof(ViewModel.ConnectionState))
                || propertyChangedEventArgs.PropertyName.Equals(nameof(ViewModel.PauseStatus)))
            {
                AnimationTypeEnum? animation = null;
                string btnText = "";

                switch (ViewModel.ConnectionState)
                {
                    case ServiceState.Connected:
                        animation = AnimationTypeEnum.Connected;
                        btnText = "disconnect";

                        switch (ViewModel.PauseStatus)
                        {
                            case MainViewModel.PauseStatusEnum.Pausing:
                                animation = AnimationTypeEnum.Disconnecting;
                                btnText = "pausing";
                                break;
                            case MainViewModel.PauseStatusEnum.Resuming:
                                animation = AnimationTypeEnum.Connecting;
                                btnText = "resuming";
                                break;
                            case MainViewModel.PauseStatusEnum.Paused:
                                animation = AnimationTypeEnum.Disconnected;
                                btnText = "resume";
                                break;
                        }
                        break;

                    case ServiceState.Disconnected:
                        animation = AnimationTypeEnum.Disconnected;
                        btnText = "connect";
                        break;

                    case ServiceState.Connecting:
                    case ServiceState.ReconnectingOnService:
                    case ServiceState.ReconnectingOnClient:
                        animation = AnimationTypeEnum.Connecting;
                        btnText = "connecting";
                        break;

                    case ServiceState.CancellingConnection:
                    case ServiceState.Disconnecting:
                        animation = AnimationTypeEnum.Disconnecting;
                        btnText = "disconnecting";
                        break;
                }
                
                if (animation != null)
                {
                    AnimationType = (AnimationTypeEnum) animation;
                    ConnectButtonText = btnText;
                }
            }
            else if (propertyChangedEventArgs.PropertyName.Equals(nameof(ViewModel.ConnectionError)))
            {
                if (!string.IsNullOrEmpty(ViewModel.ConnectionError))
                    MessageBox.Show(ViewModel.ConnectionError, StringUtils.String("Error_ConnectionError"), MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (propertyChangedEventArgs.PropertyName.Equals(nameof(ViewModel.ConnectionState))
                || propertyChangedEventArgs.PropertyName.Equals(nameof(ViewModel.IsKillSwitchEnabled)))
            {
                ShowFirewallOffPopupWarningIfRequired();
            }
        }

        private void ShowFirewallOffPopupWarningIfRequired()
        {
            if (Application.Current.MainWindow == null 
                || ViewModel.ConnectionState!= ServiceState.Connected
                || ViewModel.PauseStatus != MainViewModel.PauseStatusEnum.Resumed
                || ViewModel.IsKillSwitchEnabled
                || ViewModel.NavigationService.CurrentPage != NavigationTarget.MainPage
                || Application.Current.MainWindow.WindowState == WindowState.Minimized)
                return;
            GuiFirewallIsOffPopup.IsOpen = true;
        }

        private bool CheckIsCanChangeHop()
        {
            if (ViewModel.ConnectionState != ServiceState.Disconnected || ViewModel.PauseStatus != MainViewModel.PauseStatusEnum.Resumed)
            {
                GuiConnectedHopChangeWarningPopup.IsOpen = true;
                return false;
            }

            return true;
        }

        private void GuiButtonSingleHop_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckIsCanChangeHop())
                return;

            ViewModel.IsMultiHop = false;
        }

        private void GuiButtonMultiHop_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckIsCanChangeHop())
                return;

            ViewModel.IsMultiHop = true;
        }

        private void GuiConnectionInfoButton_Click(object sender, RoutedEventArgs e)
        {
            GuiConnectionInfoPopup.IsOpen = true;
            ProofsViewModel.UpdateCommand.Execute(null);
            
        }

        #region Private emails
        private void GuiPrivateEmailsButton_OnClick(object sender, RoutedEventArgs e)
        {
            GuiPrivateEmailsManuPopup.IsOpen = true;
        }

        private void GuiMenuItemGeneratePrivateEmail_OnClick(object sender, RoutedEventArgs e)
        {
            GuiPrivateEmailsManuPopup.IsOpen = false;
            PrivateEmailGenerateWindow.GenerateEmail(__ViewModelPrivateEmails);
        }

        private void MenuItemManagePrivateEmails_OnClick(object sender, RoutedEventArgs e)
        {
            GuiPrivateEmailsManuPopup.IsOpen = false;
            PrivateEmailManager.Show(__ViewModelPrivateEmails);
        }
        #endregion // Private emails

        #region Account expiration info
        private void ViewModelOnAccountSuspended(Models.Session.SessionStatus sessionInfo)
        {
            if (GuiUtils.IsInvokedInGuiThread(this, () => UpdateSessionInfo(sessionInfo)))
                return;

            UpdateSessionInfo(sessionInfo);
        }
        
        private void UpdateSessionInfo(Models.Session.SessionStatus sessionInfo)
        {
            if (GuiUtils.IsInvokedInGuiThread(this, () => UpdateSessionInfo(sessionInfo)))
                return;
            
            GuiAccountExpirationInfoButton.Visibility = Visibility.Hidden;
            if (sessionInfo == null)
                return;

            string text;
            Uri imageUri;

            // update text
            if (!sessionInfo.IsActive)
            {
                text = StringUtils.String("Label_SubscriptionExpired");
                if (sessionInfo.IsOnFreeTrial)
                    text = StringUtils.String("Label_FreeTrialExpired");

                text += " "+ StringUtils.String("Label_AccountExpiredUpgradeNow");

                imageUri = new Uri("pack://application:,,,/" +
                               Assembly.GetExecutingAssembly().GetName().Name
                               + ";component/Resources/iconStatusBad.png", UriKind.RelativeOrAbsolute);
            }
            else 
            {
                // show nothing if account Renewable (WillAutoRebill)
                if (sessionInfo.WillAutoRebill)
                    return;

                int daysLeft = (int) (sessionInfo.ActiveUtil - DateTime.Now).TotalDays;
                if (daysLeft < 0)
                    daysLeft = 0;

                // do not show notification when more than 4 days left
                if (daysLeft>=4)
                    return;
                
                if (daysLeft == 0)
                {
                    text = StringUtils.String("Label_AccountDaysLeft_LastDay");
                    if (sessionInfo.IsOnFreeTrial)
                        text = StringUtils.String("Label_FreeTrialDaysLeft_LastDay");
                }
                else if (daysLeft == 1)
                {
                    text = StringUtils.String("Label_AccountDaysLeft_OneDay");
                    if (sessionInfo.IsOnFreeTrial)
                        text = StringUtils.String("Label_FreeTrialDaysLeft_OneDay");
                }
                else
                {
                    text = StringUtils.String("Label_AccountDaysLeft_PARAMETRIZED");
                    if (sessionInfo.IsOnFreeTrial)
                        text = StringUtils.String("Label_FreeTrialDaysLeft_PARAMETRIZED");

                    text = string.Format(text, daysLeft);
                }

                imageUri = new Uri("pack://application:,,,/" +
                               Assembly.GetExecutingAssembly().GetName().Name
                               + ";component/Resources/iconStatusModerate.png", UriKind.RelativeOrAbsolute);
            }

            GuiAccountExpirationInfoText.Text = text;
            GuiAccountExpirationInfoImage.Source = new BitmapImage(imageUri);

            // set notification visible
            GuiAccountExpirationInfoButton.Visibility = Visibility.Visible;
        }

        private void GuiAccountExpirationInfoButton_OnClick(object sender, RoutedEventArgs e)
        {
            ShowAccountExpireDialog();
        }

        private void ShowAccountExpireDialog()
        {
            if (ViewModel.AppState.SessionStatusInfo == null)
                return;

            SubscriptionExpireWindow.Show(ViewModel.AppState.SessionStatusInfo, ViewModel.AppState?.Settings?.Username);
        }
        #endregion Account expiration info

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion //INotifyPropertyChanged

        #region Trusted\Untrusted WiFi
        // In use for WPF binding to ComboBox
        public IList<NetworkActionsConfig.WiFiActionTypeEnum> NetworkPossibleActions => new List<NetworkActionsConfig.WiFiActionTypeEnum>
        {
            NetworkActionsConfig.WiFiActionTypeEnum.Untrusted,
            NetworkActionsConfig.WiFiActionTypeEnum.Trusted,
            //NetworkActionsConfig.WiFiActionTypeEnum.None, // 'No action' available only for 'Default' action
            NetworkActionsConfig.WiFiActionTypeEnum.Default
        };
        #endregion //Trusted\Untrusted WiFi

        #region Pause\Resume
        
        private void GuiButtonPause_OnClick(object sender, RoutedEventArgs e)
        {
            GuiPauseMenuPopup.IsOpen = false;

            Button btn = sender as Button;
            if (btn == null)
                return;

            string strPauseTimeSec = btn.Tag as string;
            if (string.IsNullOrEmpty(strPauseTimeSec))
                strPauseTimeSec = "0";

            if (double.TryParse(strPauseTimeSec, out var pauseSec)==false)
                return;

            if (pauseSec < 1)
            {
                if (TimeIntervalDialog.ShowInputTimeIntervalDialog(out pauseSec, App.Current.MainWindow) == false)
                    return;
            }

            ViewModel.PauseCommand.Execute(pauseSec);
        }

        private void GuiPauseTimeLeftButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (TimeIntervalDialog.ShowInputTimeIntervalDialog(out var pauseSec, App.Current.MainWindow) == false)
                return;

            ViewModel.SetPauseTime(pauseSec);
        }

        #endregion //Pause\Resume
    }
}
