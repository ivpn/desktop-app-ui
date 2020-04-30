using System;
using System.IO;
using System.Timers;

using Foundation;
using AppKit;
using CoreAnimation;
using CoreGraphics;

using System.Threading.Tasks;
using System.ComponentModel;

using IVPN.Models;
using IVPN.ViewModels;
using IVPN.Interfaces;
using IVPN.Models.Configuration;
using System.Collections.Concurrent;

namespace IVPN
{

    public partial class MainWindowController : AppKit.NSWindowController, ISynchronizeInvoke, IMainWindow, ILayerDrawer
    {
        CustomButton __backButton;
        #region Old Fields need to be refactored and then removed

        private NSStatusItem statusItem;
        private NSMenuItem statusMenuItemConnectToLastServer;
        private NSMenuItem statusMenuItemPause;
        private NSMenuItem statusMenuItemResume;
        private NSMenuItem statusMenuItemDisconnect;
        private NSMenuItem statusMenuItemPreferences;
        private NSMenuItem statusMenuItemDiagnosticLogs;

        private NSMenuItem __accountMenuItem;
        private NSMenuItem __accountNameMenuItem;
        private NSMenuItem __privateEmailMenuItem;
        private NSMutableDictionary savedPreferences = new NSMutableDictionary ();

        #endregion

        private AppState __AppState;
        private ApplicationServices __AppServices;
        private IAppNavigationService __NavigationService;
        private IAppNotifications __AppNotifications;

        private Service __Service;
        private Timer __IconAnimationTimer;

        private PageView __CurrentView;

        private ViewModelLogIn __LogInViewModel;
        private ViewModelLogOut __LogOutViewModel;
        private ViewModelSessionLimit __SessionLimitModel;
        private InitViewModel __InitViewModel;
        private MainViewModel __MainViewModel;
        private ServerListViewModel __ServerListViewModel;
        private ViewModelFastestServerSettings __FastestServerConfigViewModel;
        private PrivateEmailsManagerViewModel __PrivateEmailsViewModel;

        private LogInViewController __LogInViewController;
        private LogOutViewController __LogOutViewController;
        private SessionLimitViewController __SessionLimitViewController;
        private InitViewController __InitViewController;
        private MainPageViewController __MainViewController;
        private ServersViewController __ServersViewController;
        private FastestServerConfigViewController __FastestServerConfigViewController;

        private readonly AppSettings __Settings;

        PreferencesWindowController __PreferencesWindowController;
        PrivateEmailManageWindowController __PrivateEmailWindowController;

        FirewallNotificationWindowController __FirewallNotificationWindowController;

        private bool __SubmittingLogs = false;

        #region GUI controls

        #endregion // GUI controls

        #region Old Properties need to remove


        private bool _IsUpdateButtonVisible;

        [Export ("isUpdateButtonVisible")]
        public bool IsUpdateButtonVisible {
            get {
                return _IsUpdateButtonVisible;
            }
            set {
                WillChangeValue ("isUpdateButtonVisible");
                _IsUpdateButtonVisible = value;
                DidChangeValue ("isUpdateButtonVisible");
            }
        }

        private NSString _diagnosticsPreviewText = new NSString ("");

        [Export ("diagnosticsPreviewText")]
        public NSString DiagnosticsPreviewText {
            get {
                return _diagnosticsPreviewText;
            }
            set {
                WillChangeValue ("diagnosticsPreviewText");
                _diagnosticsPreviewText = value;
                DidChangeValue ("diagnosticsPreviewText");
            }
        }

        [Export ("preferences")]
        public NSMutableDictionary preferences {
            get {
                return savedPreferences;
            }
        }

        #endregion

        // Called when created from unmanaged code
        public MainWindowController (IntPtr handle) : base (handle)
        {
            Initialize ();
        }
        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public MainWindowController (NSCoder coder) : base (coder)
        {
            Initialize ();
        }
        // Call to load from the XIB/NIB file
        public MainWindowController (AppSettings settings) : base ("MainWindow")
        {
            __Settings = settings;

            Initialize ();
        }

        // Shared initialization code
        void Initialize ()
        {
            __Service = new Service(this, new Servers(AppSettings.Instance()));
            __Service.ServiceInitialized += (object sender, EventArgs e) =>
            {
                UpdateMenuItems();
            };

            __AppState = AppState.Initialize(__Service);

            // Initialize API service object
            System.Net.IPAddress.TryParse(__Settings.AlternateAPIHost, out System.Net.IPAddress alternateAPIHost);
            IVPNCommon.Api.ApiServices.Instance.Initialize(alternateAPIHost);
            // save into settings when alternate host changed
            IVPNCommon.Api.ApiServices.Instance.AlternateHostChanged += (System.Net.IPAddress ip) =>
            {
                __Settings.AlternateAPIHost = (ip == null) ? "" : ip.ToString();
            };

            __AppServices = new ApplicationServices( LocalizedStrings.Instance );//.GetInstance ();
            __AppServices.HelperMethodInstalled += AppServices_HelperMethodInstalled;
            __NavigationService = new NavigationService (this);
            __AppNotifications = new AppNotifications (this);

            __IconAnimationTimer = new Timer (200);
            __IconAnimationTimer.AutoReset = true;
            __IconAnimationTimer.Elapsed += IconAnimationTimer_Elapsed;

            

            MacWiFiWrapper.Create(); // initialize WiFi wrapper

            __AppState.OnAccountStatusChanged += (session) => UpdateMenuItems(); 

            __MainViewModel = new MainViewModel(__AppState, this, __NavigationService, __AppNotifications, __AppServices, __Service);
            __MainViewModel.PropertyChanged += MainViewModel_PropertyChanged;

            __LogInViewModel = new ViewModelLogIn(__AppState, __AppServices, __NavigationService, __MainViewModel.WireguardKeysManager);
            __LogOutViewModel = new ViewModelLogOut(__AppState, __AppServices, __NavigationService, __MainViewModel);
            __SessionLimitModel = new ViewModelSessionLimit(__LogInViewModel, __AppState, __AppServices, __NavigationService);

            __ServerListViewModel = new ServerListViewModel (__AppServices, __NavigationService, __Service, __MainViewModel);

            __FastestServerConfigViewModel = new ViewModelFastestServerSettings(__AppServices, __NavigationService, __Service, __MainViewModel);
            __PrivateEmailsViewModel = new PrivateEmailsManagerViewModel(__AppState, __AppServices);

            // Check for update - show 'update information' if new release available
            Task checkTask = new Task (() => {
                // Normally, update-check should be performed after application loaded (MainView shown)
                // But there is a chance not to show MainView due to some problems (E.g.: helper was not started... etc.)
                // Here we are waiting some time... and if the update check still was not performed - check update anyway
                System.Threading.Thread.Sleep (30000);
                IVPN.IVPNUpdater.InitializeUpdater ();
            });

            checkTask.Start ();
        }

        void AppServices_HelperMethodInstalled (object sender, EventArgs e)
        {
            MakeFront ();
        }

        private void MainViewModel_PropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            // Start GUI update in separate task
            // Return from function immediately (avoid GUI freeze)
            System.Threading.Tasks.Task.Run (() => ProcessModelPropertyChange (e.PropertyName));
        }

        private void ProcessModelPropertyChange(string propertyName)
        {
            if (!NSThread.IsMain) 
            {
                InvokeOnMainThread (() => ProcessModelPropertyChange (propertyName));
                return;
            }

            if (propertyName.Equals(nameof(__MainViewModel.ConnectionState)))
            {
                UpdateMenuItems();

                if (AwaitingDisconnect && __MainViewModel.ConnectionState == ServiceState.Disconnected)
                {
                    NSApplication.SharedApplication.ReplyToApplicationShouldTerminate(true);
                }
            }

            else if (propertyName.Equals(nameof(__MainViewModel.SelectedServer))
                || propertyName.Equals(nameof(__MainViewModel.PauseStatus)))
                UpdateMenuItems();
        }

        override public void AwakeFromNib ()
        {
            base.AwakeFromNib ();

            MakeFront ();

            GuiInitialize ();
        }

        public override void WindowDidLoad ()
        {
            base.WindowDidLoad ();

            ShowInitPage ();

            InitStatusMenu ();

            diagnosticsTextView.Font = NSFont.UserFixedPitchFontOfSize (12);
            WindowView.WantsLayer = true;

            // Check for update - show 'update information' if new release available (if still not checked)
            // Update check should be performed after successfull application start
            // (on a curent moment everything is initialized (e.g.: firewall ... etc.))
            IVPN.IVPNUpdater.InitializeUpdater();
        }

        private void View_OnApperianceChanged()
        {
			Logging.Info($"UI Apperiance chnaged: {Window.EffectiveAppearance.Name}");

			// Dark mode support 
			Colors.SetAppearance(Window.EffectiveAppearance);

            CustomButtonStyles.ApplyStyleTitleNavigationButton(__backButton);
            // changing common background color for all transparent views
            Window.BackgroundColor = Colors.WindowBackground;

            // Update apperiance of introduction views
            UpdateIntroductionApperiance();
        }

        private void GuiInitialize ()
        {
            // Disable title-bar (but keep close/minimize/expand buttons on content-view)
            // IMPORTANT! 'FullSizeContentView' implemented since OS X 10.10 !!!
            Window.TitleVisibility = NSWindowTitleVisibility.Hidden;
            Window.TitlebarAppearsTransparent = true;
            Window.StyleMask |= NSWindowStyle.FullSizeContentView;

            // set correct backfround color (White/Dark mode)
            View_OnApperianceChanged();

            InitializeToolbar ();
        }

        #region ToolBar methods
        private void InitializeToolbar ()
        {
            // Hide ToolBar separator line
            GuiToolBar.ShowsBaselineSeparator = false;

            // Due to a known bug in Cocoa InterfaceBuilder, it is not possible to add CustomView into ToolBar from IB.
            // Therefore, we should manually add CustomViews from code

            // 'Back' button
            __backButton = new CustomButton ();
            __backButton.Activated += OnBackTitleButtonPressed;
            CustomButtonStyles.ApplyStyleTitleNavigationButton(__backButton, "\u27E8  " + __AppServices.LocalizedString("Button_Back"));
            GuiToolBarItemBackButton.View = __backButton;

            /*
             * Implementation of 'Config' button (will be in use after implementation configuration for each server)
             * 
            // 'Configure servers' button
            CustomButton configureButton = new CustomButton(); // action detined in ServersViewController
            CustomButtonStyles.ApplyStyleTitleConfigureButton(configureButton, "Configure");
            GuiToolbarItemConfigureButton.View = configureButton;
            */

            GuiClearToolBar ();
        }

        private void GuiClearToolBar ()
        {
            while (GuiToolBar.VisibleItems.Length > 0)
                GuiToolBar.RemoveItem (0);
        }

        private void GuiShowLogInToolBarItems ()
        {
            GuiClearToolBar ();
            GuiToolBar.ShowsBaselineSeparator = false;
        }

        private void GuiShowMainViewToolBarItems ()
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => GuiShowMainViewToolBarItems());
                return;
            }

            GuiClearToolBar ();
            GuiToolBar.ShowsBaselineSeparator = true;

            GuiToolBar.InsertItem (GuiToolBarItemLogo.Identifier, 0);
            GuiToolBar.InsertItem(GuiToolBarItemFlexibleSpace.Identifier, GuiToolBar.VisibleItems.Length);

            GuiToolBar.InsertItem (GuiToolBarItemMenu.Identifier, GuiToolBar.VisibleItems.Length);
        }

        private void GuiShowServersToolBarView ()
        {
            GuiClearToolBar ();
            GuiToolBar.ShowsBaselineSeparator = true;

            GuiToolBar.InsertItem (GuiToolBarItemBackButton.Identifier, 0);
            GuiToolBar.InsertItem(GuiToolBarItemFlexibleSpace.Identifier, GuiToolBar.VisibleItems.Length);
            /*
             * Implementation of 'Config' button (will be in use after implementation configuration for each server) 
             * 
            GuiToolBar.InsertItem(GuiToolbarItemConfigureButton.Identifier, GuiToolBar.VisibleItems.Length);
            */
        }

        private void GuiShowFastestServerConfigToolBarView()
        {
            GuiClearToolBar();
            GuiToolBar.ShowsBaselineSeparator = true;

            GuiToolBar.InsertItem(GuiToolBarItemBackButton.Identifier, 0);
        }

        /// <summary>
        /// Removes the toolbar item.
        /// </summary>
        private void RemoveToolbarItem (NSToolbarItem itemToRemove)
        {
            for (int i = 0; i < GuiToolBar.VisibleItems.Length; i++) {
                NSToolbarItem item = GuiToolBar.VisibleItems [0];

                if (item == itemToRemove) {
                    GuiToolBar.RemoveItem (i);
                    break;
                }
            }
        }

        /// <summary>
        /// 'Back' button pressed - navigate to maing view (from e.g. server-list view)
        /// </summary>
        private void OnBackTitleButtonPressed (object sender, EventArgs e)
        {
            __NavigationService.GoBack();
        }

        /// <summary>
        /// 'Preferences' button pressed 
        /// </summary>
        partial void GuiToolBarPreferencesPressed (Foundation.NSObject sender)
        {
            __MainViewModel.NavigationService.ShowSettingsWindow ();
        }

        #endregion //ToolBar methods

        public void RestoreWindowPositions ()
        {
            if (!__Settings.IsFirstIntroductionDone) 
            {
                // If it is a first run (no introduction was shown to user)
                // onle center window on a screen
                Window.Center ();
                return;
            }
            
            RestoreWindowLastPosition (Window, __Settings.LastWindowPosition);
        }

        public void SaveWindowPositions ()
        {
            try {
                if (__Settings == null)
                    return;

                __Settings.LastWindowPosition = GetWindowPosition (Window);
                __Settings.Save ();
            } catch (Exception e) {
                Logging.Info (string.Format ("{0}", e));
            }
        }

        #region Save\Restore window position
        private static string GetWindowPosition (NSWindow wnd)
        {
            if (wnd == null)
                return null;
            return String.Format ("{0:D},{1:D}",
                                  (int)wnd.Frame.X,
                                  (int)(wnd.Frame.Y + wnd.Frame.Height)
                                 );
        }

        private static void RestoreWindowLastPosition (NSWindow wnd, string position)
        {
            try {
                if (wnd != null && !String.IsNullOrEmpty (position)) {
                    var windowPosition = position.Split (new char [] { ',' });
                    if (windowPosition.Length == 2) {
                        int left, top;

                        if (!int.TryParse (windowPosition [0], out left))
                            return;

                        if (!int.TryParse (windowPosition [1], out top))
                            return;

                        int x = left;
                        int y = (int)(top - wnd.Frame.Height);

                        if (IsOnScreen (wnd, x, y, (int)wnd.Frame.Width, (int)wnd.Frame.Height))
                            wnd.SetFrame (new CGRect (x, y, wnd.Frame.Width, wnd.Frame.Height), false, false);
                    }
                }
            } catch (Exception e) {
                Logging.Info (string.Format ("{0}", e));
            }
        }

        private bool IsOnScreen ()
        {
            return IsOnScreen (Window, (int)Window.Frame.X, (int)Window.Frame.Y, (int)Window.Frame.Width, (int)Window.Frame.Height);
        }

        private static bool IsOnScreen (NSWindow wnd, int x, int y, int width, int height)
        {
            const int minOffset = 30;

            if (wnd == null)
                throw new Exception("IsOnScreen(NSWindow wnd, ...): window is null");
      
            if (wnd.Screen == null)
                return false;
            
            try
            {
                return wnd.Screen.VisibleFrame.IntersectsWith(new CGRect(x + minOffset, y + height - minOffset, 1, 1)) // top left
                          || wnd.Screen.VisibleFrame.IntersectsWith(new CGRect(x + width - minOffset, y + height - minOffset, 1, 1)); // top right
            }
            catch (Exception ex)
            {
                Logging.Info(string.Format("{0}", ex));
                return false;
            }
        }
        #endregion

        private PageView __AnimateOldNSView;
        private PageView __AnimateNewNSView;
        private NavigationAnimation __AnimateAnimation;
        private Action __AnimateAction;

        /// <summary>
        /// Is change-View animation in progress. 
        /// If TRUE - do not start new animation or window resizing
        /// </summary>
        public bool IsAnimationInProgress { get; private set; }
        public readonly object __AnimationLocker = new object();

        class  NavigationRequest
        {
            public NavigationRequest(PageView newView, NavigationAnimation animation, Action whenFinished)
            {
                NewView = newView;
                Animation = animation;
                WhenFinished = whenFinished;
            }

            public PageView NewView { get; }
            public NavigationAnimation Animation { get; }
            public Action WhenFinished { get; }
        }
            
        ConcurrentQueue<NavigationRequest> __NavigationRequestsQueue = new ConcurrentQueue<NavigationRequest>();

        private void AnimateAppearance (PageView newView, NavigationAnimation animation, Action whenFinished = null, int setWindowHeight = 0)
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread (() => AnimateAppearance (newView, animation, whenFinished));
                return;
            }

            lock (__AnimationLocker)
            {
                if (IsAnimationInProgress)
                {
                    __NavigationRequestsQueue.Enqueue(new NavigationRequest(newView, animation, whenFinished));
                    return;
                }
            }

            PageView oldView = __CurrentView;

            // make the new view appears on the side but not outside the visible area 
            // so it can be drawn before animation starts.
            var initialPosisionX = newView.Frame.Width - 20;

            CGRect newPageNewFrame = new CGRect (new CGPoint (0, 0), MainPageView.Frame.Size);
            if (setWindowHeight > 0)
            {
                newPageNewFrame = new CGRect(new CGPoint(0, 0), new CGSize(newView.Frame.Size.Width, setWindowHeight));
                Window.SetFrame(
                UIUtils.UpdateHeight(
                    Window.Frame,
                    newPageNewFrame.Height
                    ),
                true, true);
            }

            if (oldView == null) 
            {
                newView.Frame = newPageNewFrame;
                newView.AlphaValue = 1;
                newView.Hidden = false;

                __CurrentView = newView;
            } 
            else 
            {
                IsAnimationInProgress = true;

                newView.WasDrawn = false;
                newView.Frame = new CGRect (new CGPoint (animation == NavigationAnimation.FadeToLeft ? initialPosisionX : -initialPosisionX, 0), newPageNewFrame.Size);
                newView.AlphaValue = 0.9f;
                newView.Hidden = false;
                newView.NeedsDisplay = true;


                __AnimateNewNSView = newView;
                __AnimateOldNSView = oldView;
                __AnimateAction = whenFinished;
                __AnimateAnimation = animation;

                StartViewAnimation ();
            }
        }

        private void StartViewAnimation ()
        {
            var oldView = __AnimateOldNSView;
            var newView = __AnimateNewNSView;
            var animation = __AnimateAnimation;
            var pageWidth = newView.Frame.Width;

            if (newView.WasDrawn == false) 
            {
                BeginInvokeOnMainThread (() => { StartViewAnimation (); });
                return;
            }

            CGRect newPageNewFrame = new CGRect (new CGPoint (0, 0), MainPageView.Frame.Size);
            CGRect oldPageNewFrame = new CGRect (new CGPoint (animation == NavigationAnimation.FadeToLeft ? -pageWidth : pageWidth, 0), InitPageView.Frame.Size);

            NSAnimationContext.RunAnimation ((NSAnimationContext context) => 
            {
                context.Duration = 0.4f;

                NSView oldPageAnimator = (NSView)oldView.Animator;
                NSView newPageAnimator = (NSView)newView.Animator;

                oldPageAnimator.Frame = oldPageNewFrame;
                oldPageAnimator.AlphaValue = 0.5f;

                newPageAnimator.Frame = newPageNewFrame;
                newPageAnimator.AlphaValue = 1f;

            }, () => 
            {
                try
                {
                    oldView.Hidden = true;
                    oldView.AlphaValue = 1f;

                    newView.Frame = newPageNewFrame;
                    newView.AlphaValue = 1f;
                    newView.Hidden = false;

                    __CurrentView = newView;

                    __AnimateAction?.Invoke();
                }
                finally
                {
                    lock (__AnimationLocker)
                    {
                        IsAnimationInProgress = false;

                        NavigationRequest nextAnimation = null;
                        if (__NavigationRequestsQueue.TryDequeue(out nextAnimation))
                            AnimateAppearance(nextAnimation.NewView, nextAnimation.Animation, nextAnimation.WhenFinished);
                    }
                }                
            });
        }

        private void PrepareViewController (NSViewController viewController, NSView view)
        {
            viewController.View.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;
            viewController.View.Frame = view.Bounds;

            view.AutoresizesSubviews = true;
            view.LayerContentsRedrawPolicy = NSViewLayerContentsRedrawPolicy.OnSetNeedsDisplay;
            view.WantsLayer = true;   // WantsLayer will be set only after the view is fully displayed.

            view.Hidden = true;
            view.AddSubview (viewController.View);
        }

        public void ShowLogInPage (NavigationAnimation animation = NavigationAnimation.FadeToRight, bool doLogIn = false, bool doForceLogin = false)
        {
            if (!NSThread.IsMain) 
            {
                InvokeOnMainThread (() => ShowLogInPage (animation));
                return;
            }

            // Close other windows 
            CloseAllWindowsOnLogOut();
            // Update main menu (Connect to last server item should be disabled)
            UpdateMenuItems();
            // prepare toolbars to show LogIn page
            GuiShowLogInToolBarItems();

            if (__PrivateEmailWindowController!=null)
            {
                __PrivateEmailWindowController.Close ();
                __PrivateEmailWindowController = null;
            }

            if (__LogInViewController == null) 
            {
                __LogInViewController = new LogInViewController ();
                __LogInViewController.SetViewModel (__LogInViewModel);

                PrepareViewController (__LogInViewController, LogInPageView);
            }

            int height = 0;
            if (__LogInViewController.InitialHeight > 1)
                height = (int)__LogInViewController.InitialHeight + 11; 

            AnimateAppearance ((PageView)LogInPageView, animation, () => 
            {
                __LogInViewController.Navigated ();

                if (doLogIn)
                {
                    if (doForceLogin)
                        __LogInViewModel.LogInAndDeleteAllSessionsCommand.Execute(null);
                    else
                        __LogInViewModel.LogInCommand.Execute(null);
                }
            }, setWindowHeight: height);
        }   

        public void ShowSessionLimitPage(NavigationAnimation animation)
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => ShowSessionLimitPage(animation));
                return;
            }

            if (__SessionLimitViewController == null)
            {
                __SessionLimitViewController = new SessionLimitViewController();
                __SessionLimitViewController.SetViewModel(__SessionLimitModel);

                PrepareViewController(__SessionLimitViewController, SessionLimitPageView);
            }

            AnimateAppearance((PageView)SessionLimitPageView, animation, () =>
            {
                __SessionLimitViewController.Navigated();
            });
        }

        public void ShowLogOutPage(NavigationAnimation animation, bool showSessionLimit = false)
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => ShowLogOutPage(animation, showSessionLimit));
                return;
            }

            // Close other windows 
            CloseAllWindowsOnLogOut();
            // Update main menu (Connect to last server item should be disabled)
            UpdateMenuItems();
            // prepare toolbars to show LogIn page
            GuiShowLogInToolBarItems();

            if (__LogOutViewController == null)
            {
                __LogOutViewController = new LogOutViewController();
                __LogOutViewController.SetViewModel(__LogOutViewModel);

                PrepareViewController(__LogOutViewController, LogOutPageView);
            }

            AnimateAppearance((PageView)LogOutPageView, animation, async () =>
            {
                await __LogOutViewController.Navigated(showSessionLimit);
            });
        }

        public void ShowSingUpPage (NavigationAnimation animation = NavigationAnimation.FadeToLeft)
        {
            OpenUrl(Constants.GetSignUpUrl());
            /*
             * Internal implementation of SignUp form is temporary disabled.
             * We are opening SingUp web page instead
             * Do not remove this code. We can use it later.
             * 
            if (!NSThread.IsMain) 
            {
                InvokeOnMainThread (() => ShowSingUpPage (animation));
                return;
            }

            __accountMenuItem.Hidden = true;

            if (__SignUpViewModel == null)
                __SignUpViewModel = new ViewModelSignUp (__AppServices, __NavigationService, __UserAccountChecker);

            if (__SignUpViewController == null) {
                __SignUpViewController = new SignUpViewController ();
                __SignUpViewController.SetViewModel (__SignUpViewModel);

                PrepareViewController (__SignUpViewController, SignUpPageView);
            }

            AnimateAppearance (__CurrentView, (PageView)SignUpPageView, animation, () => 
            {
                __SignUpViewController.Navigated ();
            });
            //*/
        }
        
        public void ShowInitPage (NavigationAnimation animation = NavigationAnimation.FadeToRight)
        {
            if (!NSThread.IsMain) 
            {
                InvokeOnMainThread (() => ShowInitPage (animation));
                return;
            }

            if (__InitViewModel == null)
                __InitViewModel = new InitViewModel (__AppState, __AppServices, __NavigationService, __Service, __Settings);

            if (__InitViewController == null)
            {
                __InitViewController = new InitViewController ();
                __InitViewController.SetViewModel (__InitViewModel);

                // bo be notified of changing to/from Dark Mode
                __InitViewController.View.OnApperianceChanged += View_OnApperianceChanged;

                PrepareViewController (__InitViewController, InitPageView);
            }

            AnimateAppearance ((PageView)InitPageView, animation);
            __InitViewController.StartInitialization ();
        }

        public void ShowMainPage (NavigationAnimation animation = NavigationAnimation.FadeToLeft)
        {
            if (!NSThread.IsMain) 
            {
                InvokeOnMainThread (() => ShowMainPage (animation));
                return;
            }

            GuiClearToolBar ();

            // Update main menu (Account manu item should be enabled + correct username in menu)
            UpdateMenuItems ();

            if (__MainViewController == null) 
            {
                __MainViewController = new MainPageViewController ();
                __MainViewController.SetViewModel (__MainViewModel, Window);

                PrepareViewController (__MainViewController, MainPageView);

                /*
                 * Implementation of 'Config' button (will be in use after implementation configuration for each server)
                 * 
                // disable configuration mode for servers controlled immediately after MainView shown
                if (__ServersViewController != null)
                    __ServersViewController.IsConfigMode = false;
                */

                // Initialize FirewallNotification window
                // (will be shown when no VPN connection and Firewall is blocking all trafic)
                __FirewallNotificationWindowController = new FirewallNotificationWindowController(__AppState, __Service, __AppServices, __MainViewModel, __MainViewController);
                __FirewallNotificationWindowController.OnDoubleClick += () => { MakeFront(); };
            }

            __MainViewController.UpdateWindowSize();
            AnimateAppearance((PageView)MainPageView, animation,
                               () =>
                                {
                                    // We have to start it in separate thread to release current code flow 
                                    Task.Run(() =>
                                    {
                                        GuiShowMainViewToolBarItems();
                                        StartIntroductionIfNecesary();
                                    });
                                }
                              ); 
        }

        public void ShowServersPage (NavigationAnimation animation = NavigationAnimation.FadeToLeft)
        {
            if (!NSThread.IsMain) 
            {
                InvokeOnMainThread (() => ShowServersPage (animation));
                return;
            }

            GuiClearToolBar ();

            if (__ServersViewController == null) 
            {
                __ServersViewController = new ServersViewController (GuiToolbarItemConfigureButton.View as CustomButton);
                __ServersViewController.SetViewModel (__ServerListViewModel);
                PrepareViewController (__ServersViewController, ServersPageView);
            }

            /*
             * Implementation of 'Config' button (will be in use after implementation configuration for each server)
             * 
            __ServersViewController.IsConfigMode = false;
            */

            AnimateAppearance((PageView)ServersPageView, animation, () => 
            {
                GuiShowServersToolBarView ();
                __ServerListViewModel.Navigated ();
            });
        }

        public void ShowFastestServerConfiguration(NavigationAnimation animation)
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => ShowFastestServerConfiguration(animation));
                return;
            }

            GuiClearToolBar();

            if (__FastestServerConfigViewController == null)
            {
                __FastestServerConfigViewController = new FastestServerConfigViewController();
                __FastestServerConfigViewController.SetViewModel(__FastestServerConfigViewModel);
                PrepareViewController(__FastestServerConfigViewController, FastestServerConfigPageView);
            }

            AnimateAppearance((PageView)FastestServerConfigPageView, animation, () =>
            {
                GuiShowFastestServerConfigToolBarView();
            });
        }

        public void OpenUrl(string url)
        {
            NSWorkspace.SharedWorkspace.OpenUrl(new NSUrl(url));
        }

        public void ShowPrivateEmailGenerationWindow ()
        {
            PrivateEmailGeneratedWindowController.Generate(__PrivateEmailsViewModel);
        }

        public void ShowPrivateEmailManageWindow ()
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => ShowPrivateEmailManageWindow());
                return;
            }

            if (__PrivateEmailWindowController == null)
            {
                __PrivateEmailWindowController = new PrivateEmailManageWindowController ( __PrivateEmailsViewModel );
                __PrivateEmailWindowController.Window.Center ();
            }

            MakeFront ();
            __PrivateEmailWindowController.ShowWindow (this);

        }

        public void ShowPreferencesWindow ()
        {
            if (__PreferencesWindowController == null)
            {
                __PreferencesWindowController = new PreferencesWindowController(__Settings,
                                                                                 __Service,
                                                                                 __MainViewModel);
            }
            else
                __PreferencesWindowController.ReloadSettings();

            MakeFront ();
            __PreferencesWindowController.ShowWindow (this);
        }

        /// <summary>
        /// Closes all windows which should be closed if user not logged-in.
        /// </summary>
        private void CloseAllWindowsOnLogOut()
        {
            __PreferencesWindowController?.Close();
            __PrivateEmailWindowController?.Close();
            PrivateEmailGeneratedWindowController.CloseAllWindows();
        }

        private void UpdateMenuItems ()
        {
            if (__MainViewModel == null)
                return;

            if (!NSThread.IsMain)
            {
                InvokeOnMainThread (()=> UpdateMenuItems ());
                return;
            }

            if (__MainViewModel.SelectedServer != null) 
            {
                var server = __MainViewModel.SelectedServer;

                if (server == null)
                    statusMenuItemConnectToLastServer.Hidden = true;
                else {
                    statusMenuItemConnectToLastServer.Title = string.Format ("Connect to {0}, {1}", server.VpnServer.City, server.VpnServer.Country);
                    statusMenuItemConnectToLastServer.Hidden = false;
                }
            }

            statusMenuItemConnectToLastServer.Hidden = __MainViewModel.ConnectionState != ServiceState.Disconnected;

            // if user not logged-in - disable possibility to connect 
            if (!__AppState.IsLoggedIn())
                statusMenuItemConnectToLastServer.Hidden = true;
            
            statusItem.Image = NSImage.ImageNamed ((__MainViewModel.ConnectionState == ServiceState.Connected
                                                    && __MainViewModel.PauseStatus == MainViewModel.PauseStatusEnum.Resumed) ?
                                                   "icon-connected" : "icon-disconnected");
            statusItem.Image.Template = true;

            if (__MainViewModel.ConnectionState == ServiceState.Connecting ||
                __MainViewModel.ConnectionState == ServiceState.ReconnectingOnClient ||
                __MainViewModel.ConnectionState == ServiceState.ReconnectingOnService) {
                __IconAnimationTimer.Start ();
            }

            if (__MainViewModel.ConnectionState == ServiceState.Uninitialized)
            {
                statusMenuItemDisconnect.Hidden = true;                 statusMenuItemPreferences.Hidden = true;
                statusMenuItemDiagnosticLogs.Hidden = true;
            }
            else
            {
                statusMenuItemDisconnect.Hidden = false;
                statusMenuItemPreferences.Hidden = false;
                statusMenuItemDiagnosticLogs.Hidden = false;

                if (__MainViewModel.ConnectionState == ServiceState.Disconnected)
                    statusMenuItemDisconnect.Hidden = true;
            }

            if (!__AppState.IsLoggedIn())
            {
                __accountMenuItem.Hidden = true;
                __privateEmailMenuItem.Hidden = true;
            } 
            else 
            {
                __accountMenuItem.Hidden = false;

                if (__MainViewModel.IsAllowedPrivateEmails)
                    __privateEmailMenuItem.Hidden = false;
                else
                    __privateEmailMenuItem.Hidden = true;
            }

            if (__accountNameMenuItem != null && !string.IsNullOrEmpty (AppState?.Session?.AccountID)) 
            {
                __accountNameMenuItem.Title = string.Format ("{0}{1}",
                                                             LocalizedStrings.Instance.LocalizedString ("MenuItem_AccountUsername"),
                                                             AppState?.Session?.AccountID ?? "");
                __accountNameMenuItem.Hidden = false;
            }
            else
                __accountNameMenuItem.Hidden = true;

            if (__MainViewModel.PauseStatus == MainViewModel.PauseStatusEnum.Resumed && __MainViewModel.ConnectionState == ServiceState.Connected)
                statusMenuItemPause.Hidden = false;
            else
                statusMenuItemPause.Hidden = true;

            if (__MainViewModel.PauseStatus == MainViewModel.PauseStatusEnum.Paused && __MainViewModel.ConnectionState == ServiceState.Connected)
                statusMenuItemResume.Hidden = false;
            else
                statusMenuItemResume.Hidden = true;
        }

        private int __ConnectingIconIndex = 0;
        private readonly string [] __ConnectingIcons = new string [] { "icon-disconnected", "icon-1", "icon-2", "icon-3", "icon-4", "icon-3", "icon-2", "icon-1" };

        void IconAnimationTimer_Elapsed (object sender, ElapsedEventArgs e)
        {
            InvokeOnMainThread (() => {
                if (__MainViewModel.ConnectionState == ServiceState.Connecting ||
                    __MainViewModel.ConnectionState == ServiceState.ReconnectingOnClient ||
                    __MainViewModel.ConnectionState == ServiceState.ReconnectingOnService) {
                    var imageName = __ConnectingIcons [__ConnectingIconIndex % __ConnectingIcons.Length];

                    statusItem.Image = NSImage.ImageNamed (imageName);
                    statusItem.Image.Template = true;

                    __ConnectingIconIndex++;

                } else {
                    __IconAnimationTimer.Stop ();
                    __ConnectingIconIndex = 0;
                }
            });
        }

        public void MakeFront ()
        {
            if (!IsOnScreen ())
                Window.Center ();
                
            this.ShowWindow (this);
            this.Window.MakeKeyAndOrderFront (this);
            NSRunningApplication.CurrentApplication.Activate (
                NSApplicationActivationOptions.ActivateIgnoringOtherWindows);
        }

        private void InitStatusMenu ()
        {
            var statusMenu = new NSMenu ();
            statusMenu.AutoEnablesItems = true;

            statusMenu.AddItem (string.Format ("IVPN {0}", Platform.Version), new ObjCRuntime.Selector ("dummy:"), "");
            statusMenu.AddItem (NSMenuItem.SeparatorItem);

            statusMenu.AddItem (new NSMenuItem (LocalizedStrings.Instance.LocalizedString ("MenuItem_ShowIVPN"), ((sender, e) => {
                MakeFront ();
            })));

            // last server
            statusMenuItemConnectToLastServer = new NSMenuItem("",
            (async (sender, e) => {
                if (__MainViewModel.ConnectionState == ServiceState.Disconnected)
                    await __MainViewModel.ConnectToLastServer();
            }));
            statusMenuItemConnectToLastServer.Hidden = true;
            statusMenu.AddItem(statusMenuItemConnectToLastServer);

            // Pause
            //private NSMenuItem statusMenuItemPause;
            statusMenuItemPause = new NSMenuItem("Pause");
            statusMenuItemPause.Hidden = true;

            NSMenu pauseSubMenu = new NSMenu();
            pauseSubMenu.AddItem(new NSMenuItem("Pause for 5 min.", ((sender, e) => { __MainViewModel.PauseCommand.Execute((double)5 * 60); })));
            pauseSubMenu.AddItem(new NSMenuItem("Pause for 30 min.", ((sender, e) => { __MainViewModel.PauseCommand.Execute((double)30 * 60); })));
            pauseSubMenu.AddItem(new NSMenuItem("Pause for 1 hour", ((sender, e) => { __MainViewModel.PauseCommand.Execute((double)1 * 60 * 60); })));
            pauseSubMenu.AddItem(new NSMenuItem("Pause for 3 hours", ((sender, e) => { __MainViewModel.PauseCommand.Execute((double)3 * 60 * 60); })));
            pauseSubMenu.AddItem(new NSMenuItem("Pause for a custom time...", ((sender, e) => 
                {
                    if (__MainViewController != null)
                        __MainViewController.ShowPauseTimeDialog();
                }
                                                                            )));
            statusMenuItemPause.Submenu = pauseSubMenu;

            statusMenu.AddItem(statusMenuItemPause);

            // Resume
            statusMenuItemResume = new NSMenuItem("Resume", ((sender, e) => { __MainViewModel.ResumeCommand.Execute(null); }));
            statusMenuItemResume.Hidden = true;
            statusMenu.AddItem(statusMenuItemResume);

            // Disconnect
            statusMenuItemDisconnect = new NSMenuItem(LocalizedStrings.Instance.LocalizedString("MenuItem_Disconnect"), ((sender, e) => {
                __MainViewModel.DisconnectCommand.Execute(null);
            }));
            statusMenuItemDisconnect.Hidden = true;
            statusMenu.AddItem(statusMenuItemDisconnect);

            statusMenu.AddItem (NSMenuItem.SeparatorItem);

            __accountMenuItem = new NSMenuItem (LocalizedStrings.Instance.LocalizedString ("MenuItem_Account"));
            NSMenu accountSubMenu = new NSMenu ();

            __accountNameMenuItem = accountSubMenu.AddItem (string.Format ("{0}{1}", 
                                                                           LocalizedStrings.Instance.LocalizedString ("MenuItem_AccountUsername"),
                                                                               AppState?.Session?.AccountID ?? ""), 
                                                                new ObjCRuntime.Selector ("dummy:"), 
                                                                "");
            
            accountSubMenu.AddItem (new NSMenuItem (LocalizedStrings.Instance.LocalizedString ("MenuItem_AccountLogOut"), ((sender, e) => {
                MenuItemLogOutPressed (this);
            })));

            __accountMenuItem.Submenu = accountSubMenu;
            statusMenu.AddItem (__accountMenuItem);

            statusMenu.AddItem (NSMenuItem.SeparatorItem);

            __privateEmailMenuItem = new NSMenuItem (LocalizedStrings.Instance.LocalizedString ("MenuItem_PrivateEmail"));

            NSMenu privateMailSubMenu = new NSMenu ();
            privateMailSubMenu.AddItem (new NSMenuItem (LocalizedStrings.Instance.LocalizedString ("MenuItem_PrivateEmailGenerate"), (sender, e) => {
                ShowPrivateEmailGenerationWindow ();
            }));
            privateMailSubMenu.AddItem (new NSMenuItem (LocalizedStrings.Instance.LocalizedString ("MenuItem_PrivateEmailManage"), (sender, e) => {
                ShowPrivateEmailManageWindow ();
            }));
            __privateEmailMenuItem.Submenu = privateMailSubMenu;

            statusMenu.AddItem (__privateEmailMenuItem);
            __privateEmailMenuItem.Hidden = true;

            statusMenu.AddItem (NSMenuItem.SeparatorItem);
#if DEBUG
            // Menuitem available only in DEBUG mode
            // Required for testing "Introduction" functionality
            statusMenu.AddItem (new NSMenuItem ("Show introduction...", ((sender, e) => 
            {
                __Settings.IsFirstIntroductionDone = false;
                StartIntroductionIfNecesary ();
            })));
#endif

            statusMenuItemPreferences = new NSMenuItem(LocalizedStrings.Instance.LocalizedString("MenuItem_Preferences"), ((sender, e) =>
            {
                ShowPreferencesWindow();
            }));
            statusMenu.AddItem (statusMenuItemPreferences);
            statusMenuItemPreferences.Hidden = true;

            statusMenuItemDiagnosticLogs = new NSMenuItem(LocalizedStrings.Instance.LocalizedString("MenuItem_DiagnosticsLogs"), ((sender, e) =>
            {
                this.ShowWindow(this);
                openDiagnosticsSheet();
            }));
            statusMenu.AddItem (statusMenuItemDiagnosticLogs);
            statusMenuItemDiagnosticLogs.Hidden = true;

            statusMenu.AddItem (new NSMenuItem (LocalizedStrings.Instance.LocalizedString ("MenuItem_CheckForUpdates"), ((sender, e) => {
                IVPN.IVPNUpdater.CheckForUpdates (this);
            })));

            statusMenu.AddItem (NSMenuItem.SeparatorItem);
            statusMenu.AddItem (LocalizedStrings.Instance.LocalizedString ("MenuItem_QuitIVPN"), new ObjCRuntime.Selector ("terminate:"), "");

            statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem (NSStatusItemLength.Square);
            statusItem.Menu = statusMenu;
            statusItem.Image = NSImage.ImageNamed ("icon-disconnected");
            statusItem.Image.Template = true;
            statusItem.HighlightMode = true;
        }

        #region Diagnostic logs
        ErrorReporterEvent __diagReport = null;

        public void openDiagnosticsSheet ()
        {
            if (__NavigationService.CurrentPage == NavigationTarget.InitPage)
                return;

            MakeFront ();
            NSApplication.SharedApplication.BeginSheet (diagnosticsSheet, this.Window);

            SubmittingLogs = false;
            __diagReport = null;
            diagnosticsSendButton.Enabled = false;
            diagnosticsTextViewUserComments.Value = "";

            __Service.Proxy.DiagnosticsGenerated -= Service_Proxy_DiagnosticsGenerated; // ensure that event not subscribed multiple times
            __Service.Proxy.DiagnosticsGenerated += Service_Proxy_DiagnosticsGenerated;

            diagnosticsTextViewUserComments.TextDidChange -= diagnosticsUserCommentsChanged; // ensure that event not subscribed multiple times
            diagnosticsTextViewUserComments.TextDidChange += diagnosticsUserCommentsChanged;

            __Service.Proxy.GenerateDiagnosticLogs(__Settings.VpnProtocolType);

            DiagnosticsPreviewText = new NSString ("Capturing logs, please wait...");
        }

        private void diagnosticsUserCommentsChanged(object sender, EventArgs e)
        {
            if (__diagReport == null)
                diagnosticsSendButton.Enabled = false;
            else
                diagnosticsSendButton.Enabled = !string.IsNullOrEmpty(diagnosticsTextViewUserComments.Value);
        }

        void Service_Proxy_DiagnosticsGenerated(Responses.IVPNDiagnosticsGeneratedResponse diagInfoResponse)
        {
            __diagReport = ErrorReporter.PrepareDiagReportToSend(__Settings,
                diagInfoResponse.EnvironmentLog,
                diagInfoResponse.ServiceLog,
                diagInfoResponse.ServiceLog0,
                diagInfoResponse.OpenvpnLog,
                diagInfoResponse.OpenvpnLog0);

            InvokeOnMainThread(() => {
                DiagnosticsPreviewText = new NSString(__diagReport.ToString());
            });
        }

        partial void submitDiagnosticsLogs (NSObject sender)
        {
            if (__diagReport == null)
                return;

            string userComments = diagnosticsTextViewUserComments.Value;
            if (string.IsNullOrEmpty(userComments))
            {
                GuiHelpers.IVPNAlert.Show("Please write a description of the problem you are experiencing");
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    InvokeOnMainThread(() => { SubmittingLogs = true; });
                    ErrorReporter.SendReport(__diagReport, userComments);
                    InvokeOnMainThread(() => { dismissDiagnosticsSheet(this); });
                }
                catch(Exception ex)
                {
                    InvokeOnMainThread(() => { NSAlert.WithMessage("Error submitting diagnostics Logs", "OK", null, null, ex.Message).RunModal(); });
                }
                finally
                {
                    InvokeOnMainThread(() => { SubmittingLogs = false; });
                }
            }); 
        }

        [Export ("SubmittingLogs")]
        public bool SubmittingLogs {
            get {
                return __SubmittingLogs;
            }
            set {
                WillChangeValue ("SubmittingLogs");
                __SubmittingLogs = value;
                DidChangeValue ("SubmittingLogs");
            }
        }

        partial void dismissDiagnosticsSheet (NSObject sender)
        {
            NSApplication.SharedApplication.EndSheet (diagnosticsSheet);
            diagnosticsSheet.OrderOut (this);
            DiagnosticsPreviewText = new NSString ("");
        }
        #endregion Diagnostic logs

        public void ShakeWindow (Action cb)
        {
            var SHAKE_DURATION = 0.3f;

            CAKeyFrameAnimation shakeAnim = new CAKeyFrameAnimation ();
            CGPath shakePath = new CGPath ();

            CGRect frame = this.Window.Frame;

            shakePath.MoveToPoint (frame.GetMinX (), frame.GetMinY ());
            for (int i = 0; i < 2; i++) {
                shakePath.AddLineToPoint ((float)(frame.GetMinX () - frame.Size.Width * 0.08), frame.GetMinY ());
                shakePath.AddLineToPoint ((float)(frame.GetMinX () + frame.Size.Width * 0.08), frame.GetMinY ());
            }
            shakePath.CloseSubpath ();
            shakeAnim.Path = shakePath;
            shakeAnim.Duration = SHAKE_DURATION;

            (this.Window.Animator as NSWindow).Animations = new NSDictionary ("frameOrigin", shakeAnim);
            (this.Window.Animator as NSWindow).SetFrameOrigin (this.Window.Frame.Location);

            NSTimer.CreateScheduledTimer (TimeSpan.FromSeconds (SHAKE_DURATION), (timer) => {
                InvokeOnMainThread (() => {
                    cb ();
                });
            });
        }

        #region ISynchronizeInvoke implementation

        public IAsyncResult BeginInvoke (Delegate method, object [] args)
        {
            BeginInvokeOnMainThread (() => {
                ((Action)method) ();
            });
            return null;
        }

        public object EndInvoke (IAsyncResult result)
        {
            throw new NotImplementedException ();
        }

        public object Invoke (Delegate method, object [] args)
        {
            InvokeOnMainThread (() => {
                ((Action)method) ();
            });
            return null;
        }

        public bool InvokeRequired => NSThread.IsMain;

        #endregion

        public void UpdateWindowSizeForDelta (float delta)
        {
            Window.SetFrame (
                UIUtils.UpdateHeight (
                    Window.Frame,
                    Window.Frame.Height - delta),
                true, true);
        }

        public AppState AppState { get { return __AppState; } }

        public MainViewModel MainViewModel {
            get {
                return __MainViewModel;
            }
        }

        public ServerListViewModel ServerListViewModel {
            get {
                return __ServerListViewModel;
            }
        }

        public bool AwaitingDisconnect {
            get;
            set;
        }

        //strongly typed window accessor
        public new MainWindow Window {
            get {
                return (MainWindow)base.Window;
            }
        }

        public ILocalizedStrings GetLocalizedStrings()
        {
            return __AppServices;
        }

        #region Menu actions
        partial void MenuItemPreferencesPressed (Foundation.NSObject sender)
        {
            __MainViewModel.SettingsCommand.Execute(null);
        }

        partial void MenuItemCheckForUpdatesPressed (Foundation.NSObject sender)
        {
            IVPN.IVPNUpdater.CheckForUpdates (this);
        }

        partial void MenuItemLogOutPressed (Foundation.NSObject sender)
        {
            string message = LocalizedStrings.Instance.LocalizedString ("Message_LogOut_question") + Environment.NewLine;
            if (__Service.KillSwitchIsEnabled 
                && 
                (__Service.State != ServiceState.Disconnected && __Service.State != ServiceState.Uninitialized)
               )
                message += LocalizedStrings.Instance.LocalizedString ("Message_LogOut_actioninfo_firewall_and_connection");
            else if (__Service.State != ServiceState.Disconnected && __Service.State != ServiceState.Uninitialized)
                message += LocalizedStrings.Instance.LocalizedString ("Message_LogOut_actioninfo_connection");
            else if (__Service.KillSwitchIsEnabled)
                message += LocalizedStrings.Instance.LocalizedString ("Message_LogOut_actioninfo_firewall");
            
            string logoutBtnTitle = LocalizedStrings.Instance.LocalizedString ("Button_Logout");
            string cancelBtnTitle = LocalizedStrings.Instance.LocalizedString ("Button_Cancel");

            // show alert
            NSAlert alert = NSAlert.WithMessage (message, logoutBtnTitle, cancelBtnTitle, "", "");
            NSRunningApplication.CurrentApplication.Activate (NSApplicationActivationOptions.ActivateIgnoringOtherWindows);

            // Return in case if user cancelled operation
            if (alert.RunModal () != 1)
                return;

            __NavigationService.NavigateToLogOutPage(NavigationAnimation.FadeToRight);
        }

        partial void MenuItemQuitPressed (Foundation.NSObject sender)
        {
            NSApplication.SharedApplication.Terminate (this);
        }


        #endregion //Menu actions
    }
}

