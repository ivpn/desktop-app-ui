using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using IVPN.Models;
using IVPN.Windows;
using IVPN.ViewModels;
using System.Threading;
using System.ComponentModel;
using System.IO;
using IVPN.Exceptions;
using IVPN.Interfaces;

namespace IVPN
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ISynchronizeInvoke, IMainWindow
    {
        #region Variables

        #region Internal variables
        FloatingOverlayWindow __NotificationWindow;

        private readonly Queue<NavigationRequest> __NavigationQueue;
        private bool __NavigationInProgress;
        private bool __TrayMinimizeNotified;

        private Frame __CurrentFrame;
        private Frame __TransitionFrame;

        private ApplicationServices __AppServices;
        private NavigationService __NavService;
        private Service __Service;

        private InitViewModel __InitViewModel;
        private ViewModelLogIn __LogInViewModel;
        private ViewModelLogOut __LogOutViewModel;
        private ViewModelSessionLimit __SessionLimitViewModel;
        private MainViewModel __MainViewModel;
        private ServerListViewModel __ServerListViewModel;
        private PrivateEmailsManagerViewModel __PrivateEmailViewModel;
        private ProofsViewModel __ProofsViewModel;
        private ViewModelFastestServerSettings __FastestServerSettingsViewModel;
        private ViewModelWireguardSettings __WireguardSettingsViewModel;
        #endregion //Internal variables

        #region Public variables
        public AppState AppState { get; }
        public ApplicationServices AppServices => __AppServices ?? (__AppServices = new ApplicationServices(new StringUtils()));
        public NavigationService NavigationService => __NavService ?? (__NavService = new NavigationService(this));
        public Service Service => __Service ?? (__Service = new Service(this, new Servers(App.Settings)));

        public InitViewModel InitViewModel => __InitViewModel ?? (__InitViewModel = new InitViewModel(AppState, AppServices, NavigationService, Service, App.Settings));
        public ViewModelLogIn LogInViewModel => __LogInViewModel ?? (__LogInViewModel = new ViewModelLogIn(AppState, AppServices, NavigationService, MainViewModel.WireguardKeysManager));
        public ViewModelLogOut LogOutViewModel => __LogOutViewModel ?? (__LogOutViewModel = new ViewModelLogOut(AppState, AppServices, NavigationService, MainViewModel));
        public ViewModelSessionLimit SessionLimitViewModel => __SessionLimitViewModel ?? (__SessionLimitViewModel = new ViewModelSessionLimit(LogInViewModel, AppState, AppServices, NavigationService));
        public MainViewModel MainViewModel => __MainViewModel ?? (__MainViewModel = new MainViewModel(AppState, this, NavigationService, (App)(App.Current), AppServices, Service));
        public ServerListViewModel ServerListViewModel => __ServerListViewModel ?? (__ServerListViewModel = new ServerListViewModel(AppServices, NavigationService, Service, MainViewModel));
        public PrivateEmailsManagerViewModel PrivateEmailsViewModel => __PrivateEmailViewModel ?? (__PrivateEmailViewModel = new PrivateEmailsManagerViewModel(AppState, AppServices));
        public ProofsViewModel ProofsViewModel => __ProofsViewModel ?? (__ProofsViewModel = new ProofsViewModel(AppServices));
        public ViewModelFastestServerSettings FastestServerSettingsViewModel => __FastestServerSettingsViewModel ?? (__FastestServerSettingsViewModel = new ViewModelFastestServerSettings(AppServices, NavigationService, Service, MainViewModel));
        public ViewModelWireguardSettings WireguardSettingsViewModel => __WireguardSettingsViewModel ?? (__WireguardSettingsViewModel = new ViewModelWireguardSettings(MainViewModel, Service, AppServices));

        #endregion //Public variables

        #endregion //Variables

        public MainWindow()
        {
            InitializeComponent();
            MinimizeIfStartMinimized();

            AppState = AppState.Initialize(Service);

            /// Initialize API service object
            System.Net.IPAddress.TryParse(App.Settings.AlternateAPIHost, out System.Net.IPAddress alternateAPIHost);
            IVPNCommon.Api.ApiServices.Instance.Initialize(alternateAPIHost);
            // save into settings when alternate host changed
            IVPNCommon.Api.ApiServices.Instance.AlternateHostChanged += (System.Net.IPAddress ip) =>
            {
                App.Settings.AlternateAPIHost = (ip == null) ? "" : ip.ToString();
            };

            Service.ServiceExited += ServiceExited;

            __NavigationQueue = new Queue<NavigationRequest>();

            // Navigate to initial page
            NavigationService.NavigateToInitPage(NavigationAnimation.None);

            InitializeAsync();

            // check if application started from autorun
            HashSet<string> args = new HashSet<string>(Environment.GetCommandLineArgs());
            if (args.Contains(SettingsProvider.AutoStartHiddenCmdArg))
            {
                if (App.Settings.FirewallType == IVPNFirewallType.Persistent)
                {
                    if (!App.Settings.RunOnLogin)
                    {
                        WindowState = WindowState.Minimized;
                        ShowInTaskbar = false;
                    }
                }
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize 'back' button on window title
            if (GuiBackButtonOnTitle != null)
            {
                GuiBackButtonOnTitle.Click += (theSender, eventArgs) =>
                {
                    NavigationService.GoBack();
                    //ShowMainPage(NavigationAnimation.FadeToRight);
                };
            }

            //Initialize notification window (notify user that Firewall is enabled and no connection: all trafic is blocked)
            //
            // Notification window\icon “Firewall is enabled” should be visible on display (topmost) in case when Firewall is enabled and VPN connection is OFF.
            // Notification window should be shown by IVPN client.
            // Therefore, IVPN client should automatically start on user login in case when Firewall is persistent (“Always-on Firewall” settings parameter is ON):
            //
            // “Start at login” - ON;  “Start minimized” - OFF : start normally and show notification window
            // “Start at login” - ON;  “Start minimized" - ON  : start minimized and show notification window
            // “Start at login” - OFF; “Start minimized” - OFF : start minimized and show notification window
            // “Start at login” - OFF; “Start minimized” - ON  : start minimized and show notification window
            __NotificationWindow = new FloatingOverlayWindow(AppState, AppServices, Service, this, MainViewModel);
            __NotificationWindow.Show();

            ProofsViewModel.SetMainViewModel(MainViewModel);

            // initialize updater
            AutoUpdate.Updater.Initialize();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            RestoreWindowLastPosition();
        }

        private async void InitializeAsync()
        {
            try
            {
                WindowsWiFiWrapper.Create(); // initialize WiFi wrapper
            }
            catch (Exception ex)
            {
                if (App.Settings.IsNetworkActionsEnabled)
                {
                    App.Settings.IsNetworkActionsEnabled = false;
                    App.Settings.Save();
                }

                Logging.Info("IMPORTANT: WiFi manager is not initialized. Probably, Windows-WiFi service stopped. ("+IVPNException.GetDetailedMessage(ex) + ")");
            }

            MainViewModel.PropertyChanged += MainViewModel_PropertyChanged;

            if (Service.State == ServiceState.Uninitialized)
                await InitViewModel.InitializeAsync();
        }
                
        private void MainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.ConnectionState))
            {
                App.UpdateTrayIcon(MainViewModel.ConnectionState, MainViewModel.PauseStatus);
            }
            else if (e.PropertyName == nameof(MainViewModel.PauseStatus))
            {
                App.UpdateTrayIcon(MainViewModel.ConnectionState, MainViewModel.PauseStatus);
            }
            else if (e.PropertyName == nameof(MainViewModel.ConnectionProgressString))
            {
                App.UpdateConnectionProgressString(MainViewModel.ConnectionProgressString);
            }
        }

        void ServiceExited(object sender, EventArgs e)
        {
            if (Application.Current == null)
                return;

            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null)
                return;

            mainWindow.ExitApplication(true);
        }

        private void MinimizeIfStartMinimized()
        {
            if (Properties.Settings.Default.LaunchMinimized)
            {
                if (Properties.Settings.Default.MinimizeToTray)
                    this.Hide();
                else
                    WindowState = WindowState.Minimized;
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (App.IsExiting)
                return;

            if (Properties.Settings.Default.MinimizeToTray)
            {
                if (WindowState == WindowState.Minimized)
                {
                    if (!__TrayMinimizeNotified)
                    {
                        App.TrayIcon.ShowBalloonTip(5000, App.PRODUCT_NAME + " Minimized", "Click the IVPN icon to show the application.", System.Windows.Forms.ToolTipIcon.Info);
                        __TrayMinimizeNotified = true;
                    }

                    this.Hide();
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            if (App.IsExiting)
                return;

            e.Cancel = true;
            if (App.Settings.MinimizeToTray
                && NavigationService.CurrentPage != NavigationTarget.InitPage
                && NavigationService.CurrentPage != NavigationTarget.LogInPage
                && NavigationService.CurrentPage != NavigationTarget.LogOutPage
                && NavigationService.CurrentPage != NavigationTarget.ResetPasswordPage
                && NavigationService.CurrentPage != NavigationTarget.SingUpPage)
            {
                WindowState = WindowState.Minimized;
                ShowInTaskbar = false;
                return;
            }

            // In case if KillSwitch is persistant - disconnect VPN and minimize into tray
            // When VPN-off and firewall on - will be shown topmost nitification-window "All traffic blocked"
            if ((App.Settings.FirewallType == IVPNFirewallType.Persistent 
                    || (App.Settings.FirewallDeactivationOnExit == false && Service.KillSwitchIsEnabled))
                && App.Settings.DisableFirewallNotificationWindow == false
                && Service.IsConnectedToService)
            {
                Action<bool> onDisconnected = t =>
                {
                    if (t)
                    {
                        Dispatcher.Invoke(() =>
                            {
                                WindowState = WindowState.Minimized;
                                ShowInTaskbar = false;
                                return;
                            }
                        );
                    }
                };

                AskAndDisconnect(onDisconnected);

                return;
            }

            ExitApplication(false);
        }

        private void SaveWindowPosition()
        {
            App.Settings.LastWindowPosition = GetWindowPosition(this);
            App.Settings.LastNotificationWindowPosition = GetWindowPosition(__NotificationWindow);
            App.Settings.Save();
        }

        private void RestoreWindowLastPosition()
        {
            RestoreWindowLastPosition(this, App.Settings.LastWindowPosition);
            RestoreWindowLastPosition(__NotificationWindow, App.Settings.LastNotificationWindowPosition);
        }

        private static string GetWindowPosition(Window wnd)
        {
            if (wnd == null)
                return null;
            return String.Format("{0:D},{1:D}", (int)wnd.Left, (int)wnd.Top);
        }

        private static void RestoreWindowLastPosition(Window wnd, string position)
        {
            if (wnd!=null && !String.IsNullOrEmpty(position))
            {
                var windowPosition = position.Split(new char[] { ',' });
                if (windowPosition.Length == 2)
                {
                    if (!int.TryParse(windowPosition[0], out var left))
                        return;

                    if (!int.TryParse(windowPosition[1], out var top))
                        return;

                    if (IsOnScreen(left, top, (int)wnd.ActualWidth, (int)wnd.ActualHeight))
                    {
                        wnd.Left = left;
                        wnd.Top = top;
                    }
                }
            }
        }

        private static bool IsOnScreen(int left, int top, int width, int height)
        {
            if (top < SystemParameters.VirtualScreenTop // do not allow top coordinate be out of window
                || left < SystemParameters.VirtualScreenLeft - (int)(width * 0.85) // at least, 15% of window should be visible
                || left + width > SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth + (int)(width * 0.85) // at least, 15% of window should be visible
                || top + height > SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight + (int)(height * 0.65)) // at least, 35% of window should be visible
            {
                return false;
            }

            return true;
        }

        public async void ExitApplication(bool forceExit)
        {
            if (!forceExit)
            {
                if (!await AskAndDisconnectAsync())
                    return;

                DisableFirewallIfRequired(MainViewModel);
            }

            SaveWindowPosition();

            App.IsExiting = true;
            Service.Exit();
            Application.Current.Shutdown();
        }

        private bool AskAndDisconnect(Action<bool> onDisconnected = null)
        {
            if (Service.State == ServiceState.Connected ||
                Service.State == ServiceState.Connecting ||
                Service.State == ServiceState.ReconnectingOnService ||
                Service.State == ServiceState.ReconnectingOnClient)
            {
                if (App.Settings.DoNotShowDialogOnAppClose==false)
                {
                    string message = StringUtils.String("Exiting_will_stop_vpn");
                    if (App.Settings.FirewallType == IVPNFirewallType.Persistent)
                        message = StringUtils.String("Exiting_will_stop_vpn_killswitch_persistent");

                    MessageBoxResult res = MessageBox.Show(
                        message,
                        StringUtils.String("Exiting_will_stop_vpn_Caption"),
                        MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);

                    if (res == MessageBoxResult.No)
                        return false;
                }

                MainViewModel?.DisconnectCommand.Execute(null);
            }

            if (onDisconnected != null)
            {
                Task<bool> waitTask = new Task<bool>
                    (
                        () =>
                        {
                            // Wait 5 seconds for client to terminate.
                            for (int i = 0; i < 50 && Service.IsConnectedToService; i++)
                            {
                                if (MainViewModel.ConnectionState == ServiceState.Disconnected)
                                {
                                    Thread.Sleep(200);
                                    return true;
                                }
                                Thread.Sleep(100);
                            }
                            return false;
                        }
                    );

                waitTask.ContinueWith(t => onDisconnected(t.Result));
                waitTask.Start();
            }

            return true;
        }
 
        private async Task<bool> AskAndDisconnectAsync()
        {
            if (Service.State == ServiceState.Connected ||
                Service.State == ServiceState.Connecting ||
                Service.State == ServiceState.ReconnectingOnService ||
                Service.State == ServiceState.ReconnectingOnClient)
            {
                if (AskAndDisconnect() == false)
                    return false;
                
                await Task.Run<bool>(() =>
                {
                    // Wait 5 seconds for client to terminate.
                    for (int i = 0; i < 500; i++)
                    {
                        if (MainViewModel.ConnectionState == ServiceState.Disconnected)
                        {
                            Thread.Sleep(200);
                            return true;
                        }

                        Thread.Sleep(100);
                    }

                    return false;
                });
            }

            return true;
        }

        private static void DisableFirewallIfRequired(MainViewModel viewModel)
        {
            viewModel?.DisableFirewallOnExitIfRequired();
        }

        public void AnimatedNavigate(Frame targetFrame,
                                      NavigationAnimation navigationAnimation = NavigationAnimation.FadeToLeft, Action onComplete = null)
        {
            if (!Dispatcher.CheckAccess())
            {
                GuiUtils.InvokeInGuiThread(this, () => { AnimatedNavigate(targetFrame, navigationAnimation, onComplete); }) ;
                return;
            }

            if (__NavigationInProgress)
            {
                __NavigationQueue.Enqueue(new NavigationRequest(targetFrame, navigationAnimation, onComplete));
                return;
            }
                        
            /*
            bool shouldNavigate = __CurrentFrame != null &&
                                  __CurrentFrame != targetFrame;

            targetFrame.Visibility = Visibility.Visible;

            __TransitionFrame = __CurrentFrame;
            __CurrentFrame = targetFrame;

            if (!shouldNavigate ||
                navigationAnimation == NavigationAnimation.None)
            {
                NavigationComplete();
                return;
            }

            if (targetFrame != DocumentFrame &&
                targetFrame.Content != null &&
                DocumentFrame.Content != null)
            {
                ((Page)targetFrame.Content).MinHeight = ((Page)DocumentFrame.Content).ActualHeight;
            }

            __NavigationInProgress = true;

            Frame documentFrame = targetFrame;
            
            TranslateTransform transitionFrameTransform = new TranslateTransform();
            DoubleAnimation transitionFrameAnimation = new DoubleAnimation();
            transitionFrameAnimation.Duration = new Duration(TimeSpan.FromSeconds(TRANSITION_TIME));
            transitionFrameAnimation.Completed += (sender, args) =>
            {
                NavigationComplete();
                onComplete?.Invoke();
            };
            transitionFrameAnimation.DecelerationRatio = 1;

            TranslateTransform documentFrameTransform = new TranslateTransform();
            DoubleAnimation documentFrameAnimation = new DoubleAnimation();
            documentFrameAnimation.Duration = new Duration(TimeSpan.FromSeconds(TRANSITION_TIME));
            documentFrameAnimation.DecelerationRatio = 1;

            int animationLength = (int)DocumentFrame.ActualWidth;
            if (animationLength == 0)
                animationLength = (int) ActualWidth;

            if (navigationAnimation == NavigationAnimation.FadeToLeft || navigationAnimation == NavigationAnimation.FadeUp)
            {
                transitionFrameAnimation.From = 1;
                transitionFrameAnimation.To = -animationLength + 1; // small overlapping is required for smooth transition
                documentFrameAnimation.From = animationLength;
                documentFrameAnimation.To = 0;
            }
            else
            {
                transitionFrameAnimation.From = 0;
                transitionFrameAnimation.To = animationLength;

                documentFrameAnimation.From = -animationLength + 1; // small overlapping is required for smooth transition
                documentFrameAnimation.To = 0;
            }
            
            DoubleAnimation opacityAnimation = new DoubleAnimation
                {
                    Duration = new Duration(TimeSpan.FromSeconds(TRANSITION_TIME)),
                    From = 1,
                    To = 0.5
                };

            DoubleAnimation opacityAnimation2 = new DoubleAnimation
                {
                    Duration = new Duration(TimeSpan.FromSeconds(TRANSITION_TIME)),
                    From = 0.9,
                    To = 1
                };
            
            documentFrame.RenderTransform = documentFrameTransform;
            __TransitionFrame.RenderTransform = transitionFrameTransform;

            // opacity
            __TransitionFrame.BeginAnimation(Frame.OpacityProperty, opacityAnimation);
            documentFrame.BeginAnimation(Frame.OpacityProperty, opacityAnimation2);
            
            DependencyProperty translationProperty;

            if (navigationAnimation == NavigationAnimation.FadeToLeft || navigationAnimation == NavigationAnimation.FadeToRight)
                translationProperty = TranslateTransform.XProperty;
            else
                translationProperty = TranslateTransform.YProperty;

            // move
            transitionFrameTransform.BeginAnimation(translationProperty, transitionFrameAnimation);
            documentFrameTransform.BeginAnimation(translationProperty, documentFrameAnimation);
            */
            bool shouldNavigate = __CurrentFrame != null && __CurrentFrame != targetFrame;

            targetFrame.Visibility = Visibility.Visible;

            __TransitionFrame = __CurrentFrame;
            __CurrentFrame = targetFrame;

            if (!shouldNavigate || navigationAnimation == NavigationAnimation.None)
            {
                NavigationComplete();
                return;
            }

            __NavigationInProgress = true;

            Frame documentFrame = targetFrame;
            
            TranslateTransform transitionFrameTransform = new TranslateTransform();
            TranslateTransform documentFrameTransform = new TranslateTransform();
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(0.2)),
                From = 1,
                To = 0,
                FillBehavior = FillBehavior.HoldEnd
            };

            DoubleAnimation opacityAnimation2 = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(0.2)),
                BeginTime = TimeSpan.FromSeconds(0.2),
                From = 0,
                To = 1,
                FillBehavior = FillBehavior.HoldEnd
            };
            documentFrame.RenderTransform = documentFrameTransform;
            __TransitionFrame.RenderTransform = transitionFrameTransform;

            opacityAnimation2.Completed += (sender, args) =>
            {
                NavigationComplete();
                onComplete?.Invoke();
            };

            // opacity
            targetFrame.Opacity = 0;
            __TransitionFrame.BeginAnimation(Frame.OpacityProperty, opacityAnimation);
            documentFrame.BeginAnimation(Frame.OpacityProperty, opacityAnimation2);
        }

        private void NavigationComplete()
        {
            if (__TransitionFrame != null)
            {
                if (__TransitionFrame != __CurrentFrame)
                    __TransitionFrame.Visibility = Visibility.Collapsed;

                __TransitionFrame = null;
            }

            __NavigationInProgress = false;

            DequeueNextNavigationRequest();
        }

        private void DequeueNextNavigationRequest()
        {
            if (__NavigationQueue.Count > 0)
            {
                var request = __NavigationQueue.Dequeue();
                AnimatedNavigate(request.Frame, request.Animation, request.OnComplete);
            }
        }

        public Page CurrentPage => DocumentFrame.Content as Page;

        public Button GuiBackButtonOnTitle => Template.FindName("GuiTemplateButtonBackInTitle", this) as Button;

        private void ShowBackButton()
        {
            if (GuiBackButtonOnTitle!=null)
                GuiBackButtonOnTitle.Visibility = Visibility.Visible;
        }

        private void HideBackButton()
        {
            if (GuiBackButtonOnTitle != null)
                GuiBackButtonOnTitle.Visibility = Visibility.Collapsed;
        }

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

        public void ShowPreferencesWindow()
        {
            SettingsWindow.ShowSettingsWindow(Service, WireguardSettingsViewModel);
        }
        
        public void ShowLogInPage(NavigationAnimation animation, bool doLogIn = false, bool doForceLogin = false)
        {
            // update account menu item
            App.UpdateTrayMenuItems(AppState, MainViewModel);

            Action onComplete = null;
            if (doLogIn)
            {
                onComplete = new Action(() => {
                    if (doForceLogin)
                        LogInViewModel.LogInAndDeleteAllSessionsCommand.Execute(null);
                    else
                        LogInViewModel.LogInCommand.Execute(null);                    
                });                
            }

            AnimatedNavigate(LoginFrame, animation, onComplete);
        }

        public void ShowLogOutPage(NavigationAnimation animation, bool showSessionLimit = false)
        {
            AnimatedNavigate(LogOutFrame, animation, async () => { await LogOutViewModel.DoLogOut(showSessionLimit); });
        }

        public void ShowSessionLimitPage(NavigationAnimation animation)
        {
            AnimatedNavigate(SessionLimitFrame, animation);
        }

        public void ShowSingUpPage(NavigationAnimation animation)
        {
            OpenUrl(Constants.GetSignUpUrl());
        }

        public void OpenUrl(string url)
        {
            if (string.IsNullOrEmpty(url)
                || !Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                return;

            System.Diagnostics.Process.Start(url);
        }
        
        public void ShowMainPage(NavigationAnimation animation)
        {
            // run only on main UI thread
            if (Application.Current.Dispatcher.Thread != Thread.CurrentThread)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowMainPage(animation);
                });
                return;
            }

            // update account menu item
            App.UpdateTrayMenuItems(AppState, MainViewModel);
            // Hide 'Back' button on window title
            HideBackButton();
            // Show title
            Title = StringUtils.String("Title", "IVPN");

            AnimatedNavigate(DocumentFrame, animation);
        }

        public void ShowInitPage(NavigationAnimation animation)
        {
            AnimatedNavigate(InitializationFrame, animation);
        }

        public void ShowServersPage(NavigationAnimation animation)
        {
            double height = DocumentFrame.ActualHeight;
            if (height <= 0)
                height = FastestServerConfigFrame.ActualHeight;
            ServersFrame.Height = height;

            AnimatedNavigate(ServersFrame, animation, () =>
            {
                ShowBackButton();
                ServerListViewModel.Navigated();
            });
        }

        public void ShowFastestServerConfiguration(NavigationAnimation animation)
        {
            FastestServerConfigFrame.Height = ServersFrame.ActualHeight;
            AnimatedNavigate(FastestServerConfigFrame, animation, ShowBackButton);
        }

        #region Private email

        public void PrivateEmailGenerateNew()
        {
            PrivateEmailGenerateWindow.GenerateEmail(PrivateEmailsViewModel);
        }

        public void PrivateEmailManage()
        {
            PrivateEmailManager.Show(PrivateEmailsViewModel);
        }
        #endregion // Private email
    }
}
