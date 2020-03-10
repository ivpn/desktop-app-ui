using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using System.Threading.Tasks;
using Microsoft.Shell;
using IVPN.Models;
using IVPN.ViewModels;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
using ContextMenuStrip = System.Windows.Forms.ContextMenuStrip;
using ToolStripMenuItem = System.Windows.Forms.ToolStripMenuItem;
using ToolStripSeparator = System.Windows.Forms.ToolStripSeparator;

// as described at http://blogs.microsoft.co.il/blogs/arik/archive/2010/05/28/wpf-single-instance-application.aspx

using IVPN.Interfaces;
using IVPN.Models.Configuration;
using IVPN.Windows;
using Exception = System.Exception;

namespace IVPN
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp, IAppNotifications
    {
        public const string PRODUCT_NAME = "IVPN Client";

        private static AppSettings __AppSettings;

        public static NotifyIcon TrayIcon;
        public static bool IsExiting = false;

        private System.ComponentModel.IContainer mComponents;
        private NotifyIcon __NotifyIcon;
        private ContextMenuStrip __ContextMenu;

        private ToolStripMenuItem __MenuItemDisplayForm;
        private ToolStripMenuItem __MenuItemCheckUpdates;
        private ToolStripMenuItem __MenuItemExitApplication;

        private static ToolStripSeparator __AccountSeparatorMenuItem;
        private static ToolStripMenuItem __AccountMenuItem;
        private static ToolStripMenuItem __UsernameMenuItem;

        private static ToolStripSeparator __PrivateEmailSeparatorMenuItem;
        private static ToolStripMenuItem __PrivateEmailsMenuItem;
        
        private static ToolStripMenuItem __MenuItemConnectToLastServer;
        private static ToolStripMenuItem __MenuItemDisconnect;

        private static ToolStripMenuItem __MenuItemPause;
        private static ToolStripMenuItem __MenuItemResume;

        private static bool __ExceptionCought;

        [STAThread]
        public static void Main(string[] args)
        {
            // manually set TLS v1.2 for all HTTP requests in current application domain!
            if (System.Net.ServicePointManager.SecurityProtocol < System.Net.SecurityProtocolType.Tls12)
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            }

            // set Application version globally
            Platform.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            //Logging.SetLogFile("C:\\delme\\ivpnclient.log");
            //Logging.Enable();

            if (!CheckConfig())
                return;

            if (!TryUpgradeSettings())
                return;

#if DEBUG
#warning DEBUG MODE! Platform.InstallationDirectory hardcoded to : "C:/Program Files/IVPN Client"
            Platform.InstallationDirectory = @"C:/Program Files/IVPN Client";
#else
            Platform.InstallationDirectory = Path.GetDirectoryName(GetExecutingAssemblyFile());
#endif

            if (SingleInstance<App>.InitializeAsFirstInstance("IVPNClient"))
            {
                var application = new App();
                application.DispatcherUnhandledException += App_DispatcherUnhandledException;
                application.InitializeComponent();
                application.Run();

                SingleInstance<App>.Cleanup();
            }
        }

        private static string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private static string GetParentDirectory(string path, int parentDepth)
        {
            while (parentDepth > 0)
            {
                path = Path.GetDirectoryName(path);
                parentDepth--;
            }

            return path;
        }

        private static bool CheckConfig()
        {
            try
            {
                IVPN.Properties.Settings.Default.Reload();
            }
            catch (ConfigurationErrorsException e)
            {
                ResetBrokenSettingsFile(e);
                return false;
            }

            return true;
        }

        private static void ResetBrokenSettingsFile(ConfigurationErrorsException settingsBrockenException)
        {
            // getting settings file path
            string settingsFile = string.Empty;
            if (!string.IsNullOrEmpty(settingsBrockenException.Filename))
                settingsFile = settingsBrockenException.Filename;
            else
            {
                if (settingsBrockenException.InnerException is ConfigurationErrorsException innerEx && !string.IsNullOrEmpty(innerEx.Filename))
                    settingsFile = innerEx.Filename;
            }

            if (string.IsNullOrEmpty(settingsFile) || !File.Exists(settingsFile))
                return;
            
            MessageBox.Show(
                "Your configuration file is corrupted. All settings will be reset.\n\n" +
                $"If the problem persists, please delete application configuration files at: \n {GetParentDirectory(settingsFile, 3)}",
                "IVPN Configuration file corrupted",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            try
            {
                File.Delete(settingsFile);
            }
            catch
            {
                // ignored
            }

            System.Windows.Forms.Application.Restart();
        }

        private static bool TryUpgradeSettings()
        {
            try
            {
                if (!IVPN.Properties.Settings.Default.IsUpgraded)
                {
                    IVPN.Properties.Settings.Default.Upgrade();
                }
            }
            catch (ConfigurationErrorsException ex)
            {
                // Seems, the configuration file is corrupted
                // In this case, we must try to completely delete it.
                // This will give us possibility to start the application (next start) without problems
                ResetBrokenSettingsFile(ex);
                return false;
            }
            catch (Exception)
            {
                // ignored
            }

            IVPN.Properties.Settings.Default.IsUpgraded = true;
            IVPN.Properties.Settings.Default.Save();
            return true;
        }

        private static string GetExecutingAssemblyFile()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return path;
        }

        static void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // * Occurance of this exception in most cases is caused by sudden network disconnection 
            //   between service and client (caused by other softwares or network restart).
            // * When this occures, client will be presented the Init page with an exact error message,
            //   where he can retry the connection. 
            // * So we cannot call this a crash, since there is a recovery route for the initial error
            if (e.Exception is IVPNClientProxyNotConnectedException ||
                e.Exception is OperationCanceledException)
            {
                e.Handled = true;
                return;
            }

            // Prevent multiple exceptions
            if (__ExceptionCought)
                return;

            __ExceptionCought = true;

            ExceptionWindow wnd = new ExceptionWindow(e.Exception, false);
            wnd.ShowDialog();

            e.Handled = true;

            // Force the application to exit after  
            // show crash is show to the user
            System.Environment.Exit(120);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Console.WriteLine(@"OnStartup");

            // ensure we're running in the installation directory so relative paths resolve correctly
            Directory.SetCurrentDirectory(Path.GetDirectoryName(
                System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName));

            Console.WriteLine(@"starting IVPNClientProxy");

            Console.WriteLine(@"Invoking DoStartup");
            SetupNotifyIcon();
        }

        private void SetupNotifyIcon()
        {
            mComponents = new System.ComponentModel.Container();
            __NotifyIcon = TrayIcon = new NotifyIcon(mComponents);
            __NotifyIcon.Icon = System.Drawing.SystemIcons.Application;
            __NotifyIcon.Text = PRODUCT_NAME;
            __NotifyIcon.Visible = true;
            __NotifyIcon.DoubleClick += (sender, evt) =>
            {
                RestoreApplicationWindow();
            };

            __NotifyIcon.BalloonTipClicked += (object sender, EventArgs e) =>
            {
                RestoreApplicationWindow();
            };

            TrayIcon.Icon = IVPN.Properties.Resources.disconnected;

            __ContextMenu = new ContextMenuStrip();
            __MenuItemDisplayForm = new ToolStripMenuItem();
            __MenuItemCheckUpdates = new ToolStripMenuItem();
            __MenuItemExitApplication = new ToolStripMenuItem();
            __MenuItemConnectToLastServer = new ToolStripMenuItem();
            __MenuItemDisconnect = new ToolStripMenuItem();

            __MenuItemPause = new ToolStripMenuItem();
            __MenuItemResume = new ToolStripMenuItem();

            __NotifyIcon.ContextMenuStrip = __ContextMenu;

            __ContextMenu.Items.Add(new ToolStripMenuItem { Text = string.Format("{0} {1}", PRODUCT_NAME, Platform.Version), Enabled = false });
            __ContextMenu.Items.Add(new ToolStripSeparator());

            __MenuItemDisplayForm.Text = @"Show " + PRODUCT_NAME;
            __MenuItemDisplayForm.Click += (object sender, EventArgs evt) =>
            {
                RestoreApplicationWindow();
            };

            __ContextMenu.Items.Add(__MenuItemDisplayForm);

            __MenuItemConnectToLastServer.Text = @"Connect to Last Server";
            __MenuItemConnectToLastServer.Visible = false;
            __MenuItemConnectToLastServer.Click += async (object sender, EventArgs evt) =>
            {
                await ConnectToLastServer();
            };
            __ContextMenu.Items.Add(__MenuItemConnectToLastServer);

            // PAUSE
            __MenuItemPause = new ToolStripMenuItem { Text = @"Pause" };
            var menuSubItemPause5Min = new ToolStripMenuItem {Text = @"Pause for 5 minutes", Tag = "300"};
            var menuSubItemPause30Min = new ToolStripMenuItem {Text = @"Pause for 30 minutes", Tag = "1800"};
            var menuSubItemPause1Hour = new ToolStripMenuItem {Text = @"Pause for 1 hour", Tag = "3600"};
            var menuSubItemPause3Hours = new ToolStripMenuItem { Text = @"Pause for 3 hours", Tag = "10800" };
            var menuSubItemPauseCustom = new ToolStripMenuItem { Text = @"Pause for a custom time...", Tag = "0"};

            menuSubItemPause5Min.Click += MenuSubItemPauseOnClick;
            menuSubItemPause30Min.Click += MenuSubItemPauseOnClick;
            menuSubItemPause1Hour.Click += MenuSubItemPauseOnClick;
            menuSubItemPause3Hours.Click += MenuSubItemPauseOnClick;
            menuSubItemPauseCustom.Click += MenuSubItemPauseOnClick;

            __MenuItemPause.DropDownItems.Add(menuSubItemPause5Min);
            __MenuItemPause.DropDownItems.Add(menuSubItemPause30Min);
            __MenuItemPause.DropDownItems.Add(menuSubItemPause1Hour);
            __MenuItemPause.DropDownItems.Add(menuSubItemPause3Hours);
            __MenuItemPause.DropDownItems.Add(menuSubItemPauseCustom);
            __ContextMenu.Items.Add(__MenuItemPause);

            // RESUME
            __MenuItemResume = new ToolStripMenuItem { Text = @"Resume" };
            __MenuItemResume.Click += (sender, args) =>
            {
                GetMainViewModel().ResumeCommand.Execute(null);
            };
            __ContextMenu.Items.Add(__MenuItemResume);

            // DISCONNECT
            __MenuItemDisconnect.Text = @"Disconnect";
            __MenuItemDisconnect.Click += (object sender, EventArgs evt) =>
            {
                MainViewModel mainViewModel = GetMainViewModel();
                if (mainViewModel == null)
                    return;

                if (mainViewModel.ConnectionState == ServiceState.Connected)
                    mainViewModel.DisconnectCommand.Execute(null);
            };
            __MenuItemDisconnect.Visible = false;
            __ContextMenu.Items.Add(__MenuItemDisconnect);

            // ACCOUNT menu item
            __AccountSeparatorMenuItem = new ToolStripSeparator();
            __ContextMenu.Items.Add(__AccountSeparatorMenuItem);
            
            __AccountMenuItem = new ToolStripMenuItem();
            __AccountMenuItem.Text = @"Account";
            __AccountMenuItem.Visible = false;
            __AccountSeparatorMenuItem.Visible = false;

            __UsernameMenuItem = new ToolStripMenuItem();
            __UsernameMenuItem.Enabled = false;

            ToolStripMenuItem logoutMenuItem = new ToolStripMenuItem {Text = @"Logout..."};

            logoutMenuItem.Click += LogoutMenuItemOnClick;

            __AccountMenuItem.DropDownItems.Add(__UsernameMenuItem);
            __AccountMenuItem.DropDownItems.Add(logoutMenuItem);
            __ContextMenu.Items.Add(__AccountMenuItem);

            // PRIVATE EMAILS
            __PrivateEmailSeparatorMenuItem = new ToolStripSeparator {Visible = false};
            __ContextMenu.Items.Add(__PrivateEmailSeparatorMenuItem);
            __PrivateEmailsMenuItem = new ToolStripMenuItem { Text = @"Private e-mail", Visible = false};
            ToolStripMenuItem privEmailGenerate = new ToolStripMenuItem {Text = @"Generate new..."};
            ToolStripMenuItem privEmailManage = new ToolStripMenuItem {Text = @"Manage email..."};
            privEmailGenerate.Click += PrivEmailGenerateOnClick;
            privEmailManage.Click += PrivEmailManageOnClick;
            __PrivateEmailsMenuItem.DropDownItems.Add(privEmailGenerate);
            __PrivateEmailsMenuItem.DropDownItems.Add(privEmailManage);
            __ContextMenu.Items.Add(__PrivateEmailsMenuItem);

            // UPDATES menu item
            __ContextMenu.Items.Add(new ToolStripSeparator());

            __MenuItemCheckUpdates.Text = @"Check for updates...";
            __MenuItemCheckUpdates.Click += (object sender, EventArgs evt) =>
            {
                AutoUpdate.Updater.CheckForUpdatesWithUi();
            };
            __ContextMenu.Items.Add(__MenuItemCheckUpdates);

            __ContextMenu.Items.Add(new ToolStripSeparator());

            __MenuItemExitApplication.Text = @"Exit " + PRODUCT_NAME;
            __MenuItemExitApplication.Click += (object sender, EventArgs evt) =>
            {
                MainWindow window = MainWindow as MainWindow;
                window?.ExitApplication(false);
            };
            __ContextMenu.Items.Add(__MenuItemExitApplication);

            Console.WriteLine(@"DoStartup EXIT");
        }

        private void MenuSubItemPauseOnClick(object sender, EventArgs e)
        {
            ToolStripMenuItem btn = sender as ToolStripMenuItem;
            if (btn == null)
                return;

            string strPauseTimeSec = btn.Tag as string;
            if (string.IsNullOrEmpty(strPauseTimeSec))
                strPauseTimeSec = "0";

            if (double.TryParse(strPauseTimeSec, out var pauseSec) == false)
                return;

            if (pauseSec < 1)
            {
                if (TimeIntervalDialog.ShowInputTimeIntervalDialog(out pauseSec, App.Current.MainWindow) == false)
                    return;
            }

            GetMainViewModel().PauseCommand.Execute(pauseSec);
        }

        private void PrivEmailManageOnClick(object o, EventArgs eventArgs)
        {
            if (this.MainWindow is MainWindow mainWindow) mainWindow.PrivateEmailManage();
        }

        private void PrivEmailGenerateOnClick(object o, EventArgs eventArgs)
        {
            if (this.MainWindow is MainWindow mainWindow) mainWindow.PrivateEmailGenerateNew();
        }

        private void LogoutMenuItemOnClick(object o, EventArgs eventArgs)
        {
            Logout();
        }

        /// <summary>
        /// Perform logout
        /// </summary>
        /// <returns>TRUE if Logout successful</returns>
        public bool Logout()
        {
            MainWindow mainWindow = MainWindow as MainWindow;
            var navService = mainWindow?.NavigationService;
            if (navService == null)
                return false;

            Service service = mainWindow.Service;
            
            string message = mainWindow.AppServices.LocalizedString ("Message_LogOut_question") + Environment.NewLine;
            if (service.KillSwitchIsEnabled 
                && (service.State != ServiceState.Disconnected && service.State != ServiceState.Uninitialized))
                message += mainWindow.AppServices.LocalizedString ("Message_LogOut_actioninfo_firewall_and_connection");
            else if (service.State != ServiceState.Disconnected && service.State != ServiceState.Uninitialized)
                message += mainWindow.AppServices.LocalizedString ("Message_LogOut_actioninfo_connection");
            else if (service.KillSwitchIsEnabled)
                message += mainWindow.AppServices.LocalizedString ("Message_LogOut_actioninfo_firewall");

            if (MessageBox.Show(mainWindow, message, "Log out...", MessageBoxButton.OKCancel, MessageBoxImage.Question) !=
                MessageBoxResult.OK)
                return false;

            SettingsWindow.CloseSettingsWindow();
            SubscriptionExpireWindow.Close();
            PrivateEmailManager.CloseAllWindows();
            PrivateEmailGenerateWindow.CloseAllWindows();

            navService.NavigateToLogOutPage( NavigationAnimation.FadeToRight );
            return true;
        }

        public static void UpdateTrayMenuItems(AppState appState, MainViewModel mainViewModel)
        {
            if (appState == null)
                throw new ArgumentNullException(nameof(appState));

            if (appState.IsLoggedIn())
                __UsernameMenuItem.Text = $@"Account ID: {appState.Session?.AccountID ?? ""}";

            __MenuItemConnectToLastServer.Visible = appState.IsLoggedIn();
            __AccountMenuItem.Visible = appState.IsLoggedIn();
            __AccountSeparatorMenuItem.Visible = appState.IsLoggedIn();

            __PrivateEmailSeparatorMenuItem.Visible = mainViewModel.IsAllowedPrivateEmails;
            __PrivateEmailsMenuItem.Visible = mainViewModel.IsAllowedPrivateEmails;
        }

        private async Task ConnectToLastServer()
        {
            MainViewModel mainViewModel = GetMainViewModel();
            if (mainViewModel == null)
                return;

            await mainViewModel.ConnectToLastServer();
        }

        public MainViewModel GetMainViewModel()
        {
            MainWindow mainWindow = MainWindow as MainWindow;
            return mainWindow?.MainViewModel;
        }

        private static void RestoreApplicationWindow()
        {
            if (Current.MainWindow != null)
            {
                Current.MainWindow.Show();
                Current.MainWindow.WindowState = WindowState.Normal;
                Current.MainWindow.ShowInTaskbar = true;
                Current.MainWindow.Activate();
                Current.MainWindow.Focus();
            }
        }

        public static void RefreshTrayMenuItems(bool isConnected, MainViewModel.PauseStatusEnum pauseStatus)
        {
            __MenuItemConnectToLastServer.Visible = !isConnected;
            __MenuItemDisconnect.Visible = isConnected;

            __MenuItemPause.Visible = false;
            __MenuItemResume.Visible = false;
            if (isConnected)
            {
                switch (pauseStatus)
                {
                    case MainViewModel.PauseStatusEnum.Paused:
                        __MenuItemResume.Visible = true;
                        break;

                    case MainViewModel.PauseStatusEnum.Resumed:
                        __MenuItemPause.Visible = true;
                        break;
                }
            }
        }

        public void UpdateConnectedTimeDuration(string serverName, string connectedTime)
        {
            string toolTipText = string.Format(
                PRODUCT_NAME + "\r\nConnected to: {0}\r\nDuration: {1}",
                serverName, connectedTime);

            SetNotifyIconToolTip(toolTipText);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (__NotifyIcon != null)
                __NotifyIcon.Dispose();

            base.OnExit(e);
        }

        /// <summary>
        /// Application settings object
        /// </summary>
        public static AppSettings Settings
        {
            get
            {
                if (__AppSettings == null)
                    __AppSettings = AppSettings.InitInstance(new Models.SettingsProvider());

                return __AppSettings;
            }
        }

        #region ISingleInstanceApp Members

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            if (args == null || args.Count == 0 || Current.MainWindow == null)
                return true;

            Current.MainWindow.Show();
            Current.MainWindow.WindowState = WindowState.Normal;
            Current.MainWindow.ShowInTaskbar = true;
            Current.MainWindow.Activate();
            Current.MainWindow.Focus();

            return true;
        }

        #endregion        

        // HACK to allow tooltips longer than 63 characters as imposed by NotifyIcon
        public static void SetNotifyIconToolTip(string text)
        {
            if (text.Length >= 128)
                throw new ArgumentOutOfRangeException("Text limited to 127 characters");

            Type t = typeof(NotifyIcon);
            BindingFlags hidden = BindingFlags.NonPublic | BindingFlags.Instance;
            t.GetField("text", hidden).SetValue(TrayIcon, text);
            if ((bool)t.GetField("added", hidden).GetValue(TrayIcon))
                t.GetMethod("UpdateIcon", hidden).Invoke(TrayIcon, new object[] { true });
        }        

        internal static void TrayIconDiagnosticsSubmitted(bool success, string error)
        {
            if (success)
                TrayIcon.ShowBalloonTip(5000, "Diagnostics Submission", "Logs were successfully sent.", System.Windows.Forms.ToolTipIcon.Info);
            else
                TrayIcon.ShowBalloonTip(10000, "Diagnostics Submission", error, System.Windows.Forms.ToolTipIcon.Error);
        }

        internal static void UpdateTrayIcon(ServiceState state, MainViewModel.PauseStatusEnum pauseStatus)
        {
            if (pauseStatus != MainViewModel.PauseStatusEnum.Resumed)
                TrayIcon.Icon = IVPN.Properties.Resources.disconnected;
            else
            {
                switch (state)
                {
                    case ServiceState.Connecting:
                    case ServiceState.ReconnectingOnService:
                    case ServiceState.ReconnectingOnClient:
                        TrayIcon.Icon = IVPN.Properties.Resources.connecting;
                        break;

                    case ServiceState.Connected:
                        TrayIcon.Icon = IVPN.Properties.Resources.connected;
                        UpdateConnectionProgressString("Connected");

                        break;
                    case ServiceState.Uninitialized:
                    case ServiceState.Disconnected:
                        TrayIcon.Icon = IVPN.Properties.Resources.disconnected;
                        UpdateConnectionProgressString("Disconnected");
                        break;
                }
            }

            RefreshTrayMenuItems(state == ServiceState.Connected, pauseStatus);
        }

        internal static void UpdateConnectionProgressString(string progressString)
        {
            SetNotifyIconToolTip($"{PRODUCT_NAME}\n{progressString}");
        }

        void IAppNotifications.ShowConnectedTrayBaloon(string baloonText)
        {
            TrayIcon.ShowBalloonTip(5000,
                PRODUCT_NAME + " Connected",
                baloonText,
                System.Windows.Forms.ToolTipIcon.Info);
        }

        void IAppNotifications.TrayIconNotifyInsecureWiFi(string message)
        {
            TrayIcon.ShowBalloonTip(5000,
                        "Insecure WiFi Network Detected",
                        message,
                        System.Windows.Forms.ToolTipIcon.Warning);
        }


        public void ShowDisconnectedNotification()
        {
            TrayIcon.ShowBalloonTip(5000,
                PRODUCT_NAME + " Disconnected",
                "You have disconnected from the VPN",
                System.Windows.Forms.ToolTipIcon.Info);
        }
    }
}
