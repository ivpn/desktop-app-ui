using System;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using IVPN.Exceptions;
using IVPN.Models;
using IVPN.ViewModels;
using IVPN.GuiHelpers;
using IVPN.WiFi;
using static IVPN.Models.Configuration.NetworkActionsConfig;
using IVPN.Models.Session;

namespace IVPN
{
    public partial class ConnectButtonViewController : AppKit.NSViewController
    {
        private MainViewModel __MainViewModel;
        private ProofsViewModel __ProofsViewModel;

        SubscriptionWillExpireWindowController __SubscriptionExpireWindowCrl;

        private readonly CAShapeLayer __animationLayer = new CAShapeLayer();

        #region Constructors

        // Called when created from unmanaged code
        public ConnectButtonViewController(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public ConnectButtonViewController(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        // Call to load from the XIB/NIB file
        public ConnectButtonViewController() : base("ConnectButtonView", NSBundle.MainBundle)
        {
            Initialize();
        }

        // Shared initialization code
        void Initialize()
        {
            __ProofsViewModel = new ProofsViewModel(AppDelegate.GetLocalizedStrings());
        }

        #endregion

        #region Constants
        private static readonly NSColor __ToDoDescriptionTextColor = NSColor.FromRgb(122, 138, 153);
        private static readonly NSColor __ConnectedButtonTextColor = NSColor.FromRgb(255, 255, 255);
        private static readonly NSColor __PopoverTextColor = NSColor.White;
        private static readonly NSColor __PauseTimeLeftTextColor = __ToDoDescriptionTextColor;
        private static readonly NSColor __PopoverConstTextColor = NSColor.FromRgb(155, 164, 174);
        #endregion // Constants

        #region Properties
        //strongly typed view accessor
        public new ConnectButtonView View
        {
            get
            {
                return (ConnectButtonView)base.View;
            }
        }

        [Export("MainViewModelAdapter")]
        public MainViewModelAdapter MainViewModelAdapter { get; private set; }

        [Export("ProofsViewModelAdapter")]
        public ProofsViewModelAdapter ProofsViewModelAdapter { get; private set; }

        [Export("PopoverTextColor")]
        public NSColor PopoverTextColor { get { return __PopoverTextColor; } }

        [Export("PauseTimeLeftTextColor")]
        public NSColor PauseTimeLeftTextColor { get { return __PauseTimeLeftTextColor; } }

        [Export("PopoverConstTextColor")]
        public NSColor PopoverConstTextColor { get { return __PopoverConstTextColor; } }

        #endregion //Properties

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // stylyze FreTrial button
            GuiNotificationButtonBottom.Hidden = true;
            CustomButtonStyles.ApplyStyleInfoButton(GuiNotificationButtonBottom, "", NSImage.ImageNamed("iconStatusModerate"));

            View.WantsLayer = true;
            View.LayerContentsRedrawPolicy = NSViewLayerContentsRedrawPolicy.OnSetNeedsDisplay;

            View.Layer.AddSublayer(__animationLayer);

            View.SetConnectButtonRect(GuiConnectButtonImage);
            View.SetPauseButton(GuiPauseButton);
            View.SetPauseLeftTimeTextField(GuiPauseLeftTimeText);

            View.OnButtonPressed += View_OnButtonPressed;
            View.OnButtonPausePressed += View_OnButtonPausePressed;
            View.OnPauseTimeLeftTextPressed += View_OnPauseTimeLeftTextPressed;

            UpdateWiFiInfoGuiData();
            UpdateGuiData();
            InitializeInformationPopover();

            __MainViewModel.AppState.OnAccountStatusChanged += UpdateSessionStatusInfo;
            UpdateSessionStatusInfo(__MainViewModel.AppState.AccountStatus);

            GuiWiFiButton.Activated += GuiWiFiButton_Activated;

            View.OnApperianceChanged += () =>
            {
                UpdateGuiData();
            };
        }

        bool __ToDoLabelIsHiddenLastStatus;
        private void UpdateToDoLabelHiddenStatus(bool? isHidden = null)
        {
            if (isHidden != null)
                __ToDoLabelIsHiddenLastStatus = (bool)isHidden;

            if (__MainViewModel.Settings.IsNetworkActionsEnabled == false)
                GuiLabelToDoDescription.Hidden = __ToDoLabelIsHiddenLastStatus;
            else
                GuiLabelToDoDescription.Hidden = true;
        }

        public CGRect GetConnectButtonRect()
        {
            int radius = View.CirclesRadiusIncrement;
            CGRect r = GuiConnectButtonImage.Frame;

            r = new CGRect(
                    r.X - radius,
                    r.Y - radius,
                    r.Width + 2 * radius,
                    r.Height + 2 * radius
                );

            return r;
        }

        public void SetViewModel(MainViewModel viewModel)
        {
            MainViewModelAdapter = new MainViewModelAdapter(viewModel);
            ProofsViewModelAdapter = new ProofsViewModelAdapter(__ProofsViewModel);

            __MainViewModel = viewModel ?? throw new IVPNInternalException("ViewModel is not defined");
            __MainViewModel.PropertyChanged += __MainViewModel_PropertyChanged;

            __MainViewModel.Settings.NetworkActions.PropertyChanged += NetworkActionsConfig_PropertyChanged;
            UpdateGuiData();

            __ProofsViewModel.SetMainViewModel(__MainViewModel);
            __ProofsViewModel.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName.Equals(nameof(ProofsViewModel.State)))
                    UpdateInformationPopover();
            };
        }

        void __MainViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Update connection status information
            if (e.PropertyName.Equals(nameof(__MainViewModel.ConnectionState)) || e.PropertyName.Equals(nameof(__MainViewModel.PauseStatus)))
            {
                UpdateGuiData();
                UpdateInformationPopover(false);
            }
            else if (e.PropertyName.Equals(nameof(__MainViewModel.WiFiState))
                     || e.PropertyName.Equals(nameof(__MainViewModel.WiFiActionType)))
            {
                UpdateWiFiInfoGuiData();
            }
            else if (e.PropertyName.Equals(nameof(__MainViewModel.TimeToResumeLeft)))
            {
                Action action = new Action(() => GuiPauseLeftTimeText.StringValue = (string.IsNullOrEmpty(__MainViewModel.TimeToResumeLeft)) ? "0:00:00" : __MainViewModel.TimeToResumeLeft);
                if (!NSThread.IsMain)
                    InvokeOnMainThread(action);
                else
                    action();
            }
        }

        public override void ViewDidAppear()
        {
            base.ViewDidAppear();
        }

        enum AnimationType
        {
            Connected,
            Disconnected,
            Connecting,
            Disconnecting
        }
        private AnimationType? __LastAnimation = null;

        /// <summary>
        /// Update GUI data according to ModelView
        /// </summary>
        private void UpdateGuiData()
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => UpdateGuiData());
                return;
            }

            if (ViewLoaded != true || __MainViewModel == null)
                return;

            UpdateSessionStatusInfo(__MainViewModel.AppState.AccountStatus);

            GuiPauseButton.Hidden = true;
            GuiPauseLeftTimeText.Hidden = true;

            switch (__MainViewModel.ConnectionState)
            {
                case ServiceState.Connected:

                    if (__MainViewModel.PauseStatus != MainViewModel.PauseStatusEnum.Resumed)
                    {
                        switch (__MainViewModel.PauseStatus)
                        {
                            case MainViewModel.PauseStatusEnum.Pausing:
                                StopAllAnimations();
                                ShowDisconnectingAnimation(true);
                                __LastAnimation = AnimationType.Disconnecting;
                                break;
                            case MainViewModel.PauseStatusEnum.Resuming:
                                if (__LastAnimation == null || __LastAnimation != AnimationType.Connecting)
                                {
                                    StopAllAnimations();
                                    ShowConnectingAnimation(true);
                                    __LastAnimation = AnimationType.Connecting;
                                }
                                break;

                            case MainViewModel.PauseStatusEnum.Paused:
                                GuiPauseButton.Image = Colors.IsDarkMode ? NSImage.ImageNamed("buttonStopDark") : NSImage.ImageNamed("buttonStopLight");
                                GuiPauseButton.ToolTip = "Stop";
                                GuiPauseButton.Hidden = false;
                                GuiPauseLeftTimeText.Hidden = false;

                                StopAllAnimations();
                                ShowDisconnectedAnimation(true);
                                __LastAnimation = AnimationType.Disconnected;
                                break;
                        }
                    }
                    else
                    {
                        GuiPauseButton.Image = Colors.IsDarkMode ? NSImage.ImageNamed("buttonPauseDark") : NSImage.ImageNamed("buttonPauseLight");
                        GuiPauseButton.ToolTip = LocalizedStrings.Instance.LocalizedString("ToolTip_PauseBtn");
                        GuiPauseButton.Hidden = false;

                        StopAllAnimations();
                        ShowConnectedAnimation();
                        __LastAnimation = AnimationType.Connected;
                    }
                    break;

                case ServiceState.Disconnected:
                    if (__LastAnimation == null || __LastAnimation != AnimationType.Disconnected)
                    {
                        StopAllAnimations();
                        ShowDisconnectedAnimation(false);
                        __LastAnimation = AnimationType.Disconnected;
                    }
                    else
                        UpdateDisconnectedUITheme(false);
                    break;

                case ServiceState.Connecting:
                case ServiceState.ReconnectingOnService:
                case ServiceState.ReconnectingOnClient:
                    if (__LastAnimation == null || __LastAnimation != AnimationType.Connecting)
                    {
                        StopAllAnimations();
                        ShowConnectingAnimation();
                        __LastAnimation = AnimationType.Connecting;
                    }
                    break;

                case ServiceState.CancellingConnection:
                case ServiceState.Disconnecting:
                    if (__LastAnimation == null || __LastAnimation != AnimationType.Disconnecting)
                    {
                        StopAllAnimations();
                        ShowDisconnectingAnimation();
                        __LastAnimation = AnimationType.Disconnecting;
                    }
                    break;

                case ServiceState.Uninitialized:
                    StopAllAnimations();
                    // TODO: how to process this stage ???
                    GuiConnectButtonImage.Hidden = true;
                    __LastAnimation = null;
                    return;
            }
        }

        void View_OnButtonPressed(object sender, EventArgs e)
        {
            if (__MainViewModel == null)
                return;

            if (__MainViewModel.ConnectionState == Models.ServiceState.CancellingConnection
                || __MainViewModel.ConnectionState == Models.ServiceState.Disconnecting
                || __MainViewModel.ConnectionState == Models.ServiceState.Uninitialized
               )
                return;

            try
            {
                if (__MainViewModel.ConnectionState == ServiceState.Connected && __MainViewModel.PauseStatus == MainViewModel.PauseStatusEnum.Paused)
                    __MainViewModel.ResumeCommand.Execute(null);
                else if (__MainViewModel.ConnectionState == Models.ServiceState.Disconnected)
                    __MainViewModel.ConnectCommand.Execute(null);
                else
                    __MainViewModel.DisconnectCommand.Execute(null);
            }
            catch (Exception ex)
            {
                Logging.Info(string.Format("{0}", ex));
                GuiHelpers.IVPNAlert.Show(LocalizedStrings.Instance.LocalizedString("Title_Error"), ex.Message);
            }
        }

        #region Account status 
        private void UpdateSessionStatusInfo(AccountStatus sessionStatus)
        {
            InvokeOnMainThread(() =>
            {
                try
                {
                    GuiNotificationButtonBottom.Hidden = true;
                    if (sessionStatus == null)
                        return;

                    if (!sessionStatus.IsActive)
                    {
                        string part1 = LocalizedStrings.Instance.LocalizedString("Label_SubscriptionExpired");
                        if (sessionStatus.IsOnFreeTrial)
                            part1 = LocalizedStrings.Instance.LocalizedString("Label_FreeTrialExpired");
                        string part2 = LocalizedStrings.Instance.LocalizedString("Label_AccountExpiredUpgradeNow");

                        string title = part1 + " " + part2;
                        CustomButtonStyles.ApplyStyleInfoButton(GuiNotificationButtonBottom, title, NSImage.ImageNamed("iconStatusBad"));

                        NSMutableAttributedString attrTitle = new NSMutableAttributedString(title);

                        NSStringAttributes stringAttributes0 = new NSStringAttributes();
                        stringAttributes0.Font = GuiNotificationButtonBottom.TitleFont;
                        stringAttributes0.ForegroundColor = GuiNotificationButtonBottom.TitleForegroundColor;
                        stringAttributes0.ParagraphStyle = new NSMutableParagraphStyle { Alignment = NSTextAlignment.Center };

                        NSStringAttributes stringAttributes1 = new NSStringAttributes();
                        stringAttributes1.ForegroundColor = NSColor.FromRgb(59, 159, 230);

                        attrTitle.AddAttributes(stringAttributes0, new NSRange(0, title.Length));
                        attrTitle.AddAttributes(stringAttributes1, new NSRange(title.Length - part2.Length, part2.Length));

                        GuiNotificationButtonBottom.TitleTextAttributedString = attrTitle;

                        GuiNotificationButtonBottom.Hidden = false;
                    }
                    else
                    {
                        if (sessionStatus.WillAutoRebill)
                            return;

                        if ((sessionStatus.ActiveUtil - DateTime.Now).TotalMilliseconds < TimeSpan.FromDays(4).TotalMilliseconds)
                        {
                            int daysLeft = (int)(sessionStatus.ActiveUtil - DateTime.Now).TotalDays;
                            if (daysLeft < 0)
                                daysLeft = 0;

                            string notificationString;

                            if (daysLeft == 0)
                            {
                                notificationString = LocalizedStrings.Instance.LocalizedString("Label_AccountDaysLeft_LastDay");
                                if (sessionStatus.IsOnFreeTrial)
                                    notificationString = LocalizedStrings.Instance.LocalizedString("Label_FreeTrialDaysLeft_LastDay");
                            }
                            else if (daysLeft == 1)
                            {
                                notificationString = LocalizedStrings.Instance.LocalizedString("Label_AccountDaysLeft_OneDay");
                                if (sessionStatus.IsOnFreeTrial)
                                    notificationString = LocalizedStrings.Instance.LocalizedString("Label_FreeTrialDaysLeft_OneDay");
                            }
                            else
                            {
                                notificationString = LocalizedStrings.Instance.LocalizedString("Label_AccountDaysLeft_PARAMETRIZED");
                                if (sessionStatus.IsOnFreeTrial)
                                    notificationString = LocalizedStrings.Instance.LocalizedString("Label_FreeTrialDaysLeft_PARAMETRIZED");

                                notificationString = string.Format(notificationString, daysLeft);
                            }
                            CustomButtonStyles.ApplyStyleInfoButton(GuiNotificationButtonBottom, notificationString, NSImage.ImageNamed("iconStatusModerate"));

                            GuiNotificationButtonBottom.Hidden = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.Info(string.Format("{0}", ex));
                    GuiNotificationButtonBottom.Hidden = true;
                }
            });
        }

        /// <summary>
        /// Notification button pressed (frea trial button)
        /// </summary>
        partial void GuiNotificationButtonBottomPressed(NSObject sender)
        {
            ShowAccountExpireDialog(__MainViewModel.AppState.AccountStatus);
        }

        private void ShowAccountExpireDialog(AccountStatus sessionStatus)
        {
            AccountStatus acc = sessionStatus;
            if (acc == null)
                return;

            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => ShowAccountExpireDialog(sessionStatus));
                return;
            }

            try
            {
                if (__SubscriptionExpireWindowCrl != null)
                    __SubscriptionExpireWindowCrl.Close();

                __SubscriptionExpireWindowCrl = new SubscriptionWillExpireWindowController(acc, __MainViewModel?.AppState?.Session?.AccountID);

                MainWindowController wndController = AppDelegate.GetMainWindowController();
                if (wndController != null && wndController.Window != null)
                {
                    NSWindow mainWindow = wndController.Window;

                    // Set window position centered to the main window
                    CGRect mainWindowRect = mainWindow.Frame;
                    CGRect infoWindowRect = __SubscriptionExpireWindowCrl.Window.Frame;
                    CGPoint wndNewPos = new CGPoint(mainWindowRect.X + mainWindowRect.Width / 2 - infoWindowRect.Width / 2,
                                                     mainWindowRect.Y + mainWindowRect.Height / 2 - infoWindowRect.Height / 2);
                    __SubscriptionExpireWindowCrl.Window.SetFrameOrigin(wndNewPos);
                }

                __SubscriptionExpireWindowCrl.ShowWindow(this);
            }
            catch (Exception ex)
            {
                Logging.Info(string.Format("{0}", ex));
            }
        }
        #endregion //Account status

        #region Information Popover
        readonly ViewStacker __InformationPopoverStacker = new ViewStacker();
        private NSPopover __GuiPopoverConnectionInfo;

        private void InitializeInformationPopover()
        {
            __InformationPopoverStacker.Add(GuiGeoLookupView);
            __InformationPopoverStacker.Add(GuiGeoLookupCityView);
            __InformationPopoverStacker.Add(GuiGeoLookupPublicIpView);
            __InformationPopoverStacker.Add(GuiGeoLookupUpdateView);
            __InformationPopoverStacker.Add(GuiGeoLookupErrorView);
            __InformationPopoverStacker.Add(GuiGeoLookupDurationView);
        }

        private void UpdateInformationPopover(bool isReopenIfShown = true)
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => UpdateInformationPopover(isReopenIfShown));
                return;
            }

            if (__GuiPopoverConnectionInfo == null)
                return;

            GuiGeoLookupView.Hidden = __ProofsViewModel.State != ProofsViewModel.StateEnum.Ok;
            GuiGeoLookupCityView.Hidden = __ProofsViewModel.State != ProofsViewModel.StateEnum.Ok || __ProofsViewModel.GeoLookup == null || string.IsNullOrEmpty(__ProofsViewModel.GeoLookup.City);
            GuiGeoLookupPublicIpView.Hidden = __ProofsViewModel.State != ProofsViewModel.StateEnum.Ok;

            GuiGeoLookupUpdateView.Hidden = __ProofsViewModel.State != ProofsViewModel.StateEnum.Updating;
            GuiGeoLookupErrorView.Hidden = __ProofsViewModel.State != ProofsViewModel.StateEnum.Error;
            GuiGeoLookupDurationView.Hidden = __MainViewModel.ConnectionState != ServiceState.Connected || __MainViewModel.PauseStatus != MainViewModel.PauseStatusEnum.Resumed;

            NSViewController informationPopoverController = new NSViewController();
            informationPopoverController.View = __InformationPopoverStacker.CreateView();

            __GuiPopoverConnectionInfo.ContentViewController = informationPopoverController;

            if (isReopenIfShown && __GuiPopoverConnectionInfo.Shown)
                __GuiPopoverConnectionInfo.Show(GuiInformationButton.Bounds, GuiInformationButton, NSRectEdge.MinYEdge);
        }

        /// <summary>
        /// Show popover (pressed Information button)
        /// </summary>
        partial void GuiInformationButtonPressed(Foundation.NSObject sender)
        {
            __GuiPopoverConnectionInfo = new NSPopover() { Behavior = NSPopoverBehavior.Transient, Appearance = NSPopoverAppearance.HUD };

            __ProofsViewModel.UpdateCommand.Execute(null);
            UpdateInformationPopover();

            if (__GuiPopoverConnectionInfo.Shown)
                __GuiPopoverConnectionInfo.Close();
            else
                __GuiPopoverConnectionInfo.Show(GuiInformationButton.Bounds, GuiInformationButton, NSRectEdge.MinYEdge);
        }
        #endregion Information Popover
        
        #region Animations

        private void StopAllAnimations()
        {
            View.Layer.RemoveAllAnimations();
            __animationLayer.RemoveAllAnimations();
            __LastAnimation = null;
        }

        private void ShowDisconnectingAnimation(bool isPausing = false)
        {
            View.IsVisibleCircles = true;

            if (isPausing == false)
            {
                GuiConnectButtonText.AttributedStringValue = AttributedString.Create(
                    LocalizedStrings.Instance.LocalizedString("Button_Disconnecting"),
                    Colors.ConnectButtonTextColor,
                    NSTextAlignment.Center);
            }
            else
            {
                GuiConnectButtonText.AttributedStringValue = AttributedString.Create(
                    LocalizedStrings.Instance.LocalizedString("Button_Pausing"),
                    Colors.ConnectButtonTextColor,
                    NSTextAlignment.Center);
            }
            GuiConnectButtonText.AlphaValue = 1f;

            UpdateToDoLabelHiddenStatus(true);

            var frame = GuiConnectButtonImage.Frame;
            CGRect oldBounds = new CGRect(0, 0, frame.Width, frame.Height);
            nfloat offset = frame.Width * 0.4f;
            CGRect newBounds = new CGRect(-offset, -offset, oldBounds.Width + offset * 2, oldBounds.Height + offset * 2);
            GuiConnectButtonImage.Bounds = oldBounds;

            NSAnimationContext.RunAnimation((NSAnimationContext context) =>
            {
                context.Duration = 0.5f;

                ((NSView)GuiConnectButtonImage.Animator).Bounds = newBounds;
                ((NSView)GuiConnectButtonImage.Animator).AlphaValue = 0f;
            },
            () =>
            {
                GuiConnectButtonImage.Bounds = oldBounds;
            });

            AnimateButtonTextBlinking();
        }

        private void ShowDisconnectedAnimation(bool isPaused)
        {
            View.IsVisibleCircles = true;

            UpdateDisconnectedUITheme(isPaused);

            GuiConnectButtonImage.AlphaValue = 0f;
            GuiConnectButtonText.AlphaValue = 0f;
            GuiLabelToDoDescription.AlphaValue = 0f;

            GuiConnectButtonImage.Hidden = false;
            GuiConnectButtonText.Hidden = false;
            UpdateToDoLabelHiddenStatus(false);

            NSAnimationContext.RunAnimation((NSAnimationContext context) =>
            {
                context.Duration = 0.5f;

                ((NSView)GuiConnectButtonImage.Animator).AlphaValue = 1f;
                ((NSTextField)GuiConnectButtonText.Animator).AlphaValue = 1f;
                ((NSTextField)GuiLabelToDoDescription.Animator).AlphaValue = 1f;
            },
            () =>
            {

            });
        }

        private void UpdateDisconnectedUITheme(bool isPaused)
        {
            GuiConnectButtonImage.Image = Colors.IsDarkMode ? NSImage.ImageNamed("buttonConnectDark") : NSImage.ImageNamed("buttonConnect");

            if (isPaused == false)
            {
                GuiConnectButtonText.AttributedStringValue = AttributedString.Create(
                    LocalizedStrings.Instance.LocalizedString("Button_Connect"),
                    Colors.ConnectButtonTextColor,
                    NSTextAlignment.Center);

                GuiLabelToDoDescription.AttributedStringValue = AttributedString.Create(
                    LocalizedStrings.Instance.LocalizedString("Label_ClickToConnect"),
                    __ToDoDescriptionTextColor,
                    NSTextAlignment.Center);
            }
            else
            {
                GuiConnectButtonText.AttributedStringValue = AttributedString.Create(
                    LocalizedStrings.Instance.LocalizedString("Button_Resume"),
                    Colors.ConnectButtonTextColor,
                    NSTextAlignment.Center);

                GuiLabelToDoDescription.AttributedStringValue = AttributedString.Create(
                    LocalizedStrings.Instance.LocalizedString("Label_ClickToResume"),
                    __ToDoDescriptionTextColor,
                    NSTextAlignment.Center);
            }
        }

        private void ShowConnectedAnimation()
        {
            View.IsVisibleCircles = false;

            GuiConnectButtonImage.Hidden = false;
            GuiConnectButtonText.Hidden = false;
            UpdateToDoLabelHiddenStatus(false);

            GuiConnectButtonImage.AlphaValue = 0f;
            GuiLabelToDoDescription.AlphaValue = 0f;

            GuiConnectButtonText.AttributedStringValue = AttributedString.Create(
                LocalizedStrings.Instance.LocalizedString("Button_Disconnect"),
                __ConnectedButtonTextColor,
                NSTextAlignment.Center);
            GuiConnectButtonText.AlphaValue = 1f;

            GuiLabelToDoDescription.AttributedStringValue = AttributedString.Create(
                LocalizedStrings.Instance.LocalizedString("Label_ClickToDisconnect"),
                __ToDoDescriptionTextColor,
                NSTextAlignment.Center);

            GuiConnectButtonImage.Image = NSImage.ImageNamed("buttonConnected");

            var frame = GuiConnectButtonImage.Frame;
            CGRect oldBounds = new CGRect(0, 0, frame.Width, frame.Height);
            nfloat offset = frame.Width * 0.1f;
            CGRect newBounds = new CGRect(-offset, -offset, oldBounds.Width + offset * 2, oldBounds.Height + offset * 2);
            GuiConnectButtonImage.Bounds = newBounds;

            NSAnimationContext.RunAnimation((NSAnimationContext context) =>
            {
                context.Duration = 0.5f;

                ((NSView)GuiConnectButtonImage.Animator).Bounds = oldBounds;
                ((NSView)GuiConnectButtonImage.Animator).AlphaValue = 1f;
                ((NSView)GuiLabelToDoDescription.Animator).AlphaValue = 1f;
            },
            () =>
            {
                GuiConnectButtonImage.Bounds = oldBounds;
                ShowConnectedStatusAtimation();
            });
        }

        /// <summary>
        /// Show blue circle which changing radius from button to View bounds
        /// </summary>
        private void ShowConnectedStatusAtimation()
        {
            if (__MainViewModel.ConnectionState != ServiceState.Connected)
                return;

            NSColor startCircleColor = Colors.ConnectiongAnimationCircleColor;
            NSColor endCircleColor = NSColor.Clear;

            __animationLayer.FillColor = NSColor.Clear.CGColor;

            var frame = GuiConnectButtonImage.Frame;
            var startCircle =
                CGPath.EllipseFromRect(
                    new CGRect(frame.X + 10,
                                frame.Y + 10,
                                frame.Width - 20,
                                frame.Height - 20)
                );

            nfloat radiusOffset = View.Frame.Height / 2f - frame.Height / 2f;

            var endCircle = CGPath.EllipseFromRect(new CGRect(
                frame.X - radiusOffset,
                frame.Y - radiusOffset,
                frame.Height + radiusOffset * 2,
                frame.Width + radiusOffset * 2
            ));

            CABasicAnimation circleRadiusAnimation = CABasicAnimation.FromKeyPath("path");
            circleRadiusAnimation.From = FromObject(startCircle);
            circleRadiusAnimation.To = FromObject(endCircle);

            CABasicAnimation strokeColorAnimation = CABasicAnimation.FromKeyPath("strokeColor");
            strokeColorAnimation.From = FromObject(startCircleColor.CGColor);
            strokeColorAnimation.To = FromObject(endCircleColor.CGColor);

            CABasicAnimation lineWidthAnimation = CABasicAnimation.FromKeyPath("lineWidth");
            lineWidthAnimation.From = FromObject(7);
            lineWidthAnimation.To = FromObject(1);

            CAAnimationGroup animationGroup = new CAAnimationGroup();
            animationGroup.Animations = new CAAnimation[] { circleRadiusAnimation, strokeColorAnimation, lineWidthAnimation };
            animationGroup.Duration = 2.25f;
            animationGroup.RepeatCount = float.PositiveInfinity;

            __animationLayer.AddAnimation(animationGroup, null);
        }

        private void ShowConnectingAnimation(bool isResuming = false)
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => ShowConnectingAnimation(isResuming));
                return;
            }
            // prepare GUI controls
            GuiConnectButtonImage.Hidden = false;
            GuiConnectButtonImage.Image = Colors.IsDarkMode ? NSImage.ImageNamed("buttonConnectDark") : NSImage.ImageNamed("buttonConnecting");

            if (isResuming == false)
            {
                GuiConnectButtonText.AttributedStringValue = AttributedString.Create(
                    LocalizedStrings.Instance.LocalizedString("Button_Connecting"),
                    Colors.ConnectButtonTextColor,
                    NSTextAlignment.Center);
            }
            else
            {
                GuiConnectButtonText.AttributedStringValue = AttributedString.Create(
                    LocalizedStrings.Instance.LocalizedString("Button_Resuming"),
                    Colors.ConnectButtonTextColor,
                    NSTextAlignment.Center);
            }

            GuiConnectButtonText.AlphaValue = 1f;
            View.IsVisibleCircles = false;

            // initialize animation
            NSColor startCircleColor = Colors.ConnectiongAnimationCircleColor;

            UpdateToDoLabelHiddenStatus(true);

            __animationLayer.FillColor = NSColor.Clear.CGColor;
            __animationLayer.StrokeColor = startCircleColor.CGColor;

            var frame = GuiConnectButtonImage.Frame;
            var startCircle = CGPath.EllipseFromRect(
                new CGRect(frame.X + 6,
                            frame.Y + 6,
                            frame.Width - 12,
                            frame.Height - 12)
                );

            var endCircle = CGPath.EllipseFromRect(
                new CGRect(frame.X + 3,
                            frame.Y + 3,
                            frame.Width - 6,
                            frame.Height - 6)
                );

            CABasicAnimation circleRadiusAnimation = CABasicAnimation.FromKeyPath("path");
            circleRadiusAnimation.From = FromObject(startCircle);
            circleRadiusAnimation.To = FromObject(endCircle);

            CABasicAnimation lineWidthAnimation = CABasicAnimation.FromKeyPath("lineWidth");
            lineWidthAnimation.From = FromObject(1);
            lineWidthAnimation.To = FromObject(8);

            CAAnimationGroup animationGroup = new CAAnimationGroup();
            animationGroup.Animations = new CAAnimation[] { circleRadiusAnimation, lineWidthAnimation };
            animationGroup.Duration = 1f;
            animationGroup.RepeatCount = float.PositiveInfinity;
            animationGroup.AutoReverses = true;
            __animationLayer.AddAnimation(animationGroup, null);

            AnimateButtonTextBlinking();
        }

        private void AnimateButtonTextBlinking()
        {
            nfloat alpha = 0.3f;
            if (GuiConnectButtonText.AlphaValue < 1f)
                alpha = 1f;

            NSAnimationContext.RunAnimation((NSAnimationContext context) =>
            {
                context.Duration = 1f;
                ((NSView)GuiConnectButtonText.Animator).AlphaValue = alpha;
            },
            () =>
            {
                if (__MainViewModel.ConnectionState != ServiceState.Connecting
                    && __MainViewModel.ConnectionState != ServiceState.Disconnecting)
                {
                    GuiConnectButtonText.AlphaValue = 1f;
                }
                else
                    AnimateButtonTextBlinking();
            });
        }
        #endregion //Animations


        #region Trusted\Untrusted networks
        void NetworkActionsConfig_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateWiFiInfoGuiData();
        }

        private void RecreateNetworkActionsButtonItems()
        {
            NSMenuItem menuUntrusted = new NSMenuItem("", NetworkActionButton_Changed)
            {
                AttributedTitle = AttributedString.Create(GetActionName(WiFiActionTypeEnum.Untrusted), NSColor.SystemRedColor, NSTextAlignment.Center),
                Tag = (int)WiFiActionTypeEnum.Untrusted
            };

            NSMenuItem menuTrusted = new NSMenuItem("", NetworkActionButton_Changed)
            {
                AttributedTitle = AttributedString.Create(GetActionName(WiFiActionTypeEnum.Trusted), NSColor.SystemBlueColor, NSTextAlignment.Center),
                Tag = (int)WiFiActionTypeEnum.Trusted
            };

            NSMenuItem menuNoAction = new NSMenuItem("", NetworkActionButton_Changed)
            {
                AttributedTitle = AttributedString.Create(GetActionName(WiFiActionTypeEnum.None), NSColor.Black, NSTextAlignment.Center),
                Tag = (int)WiFiActionTypeEnum.None
            };

            NSMenuItem menuDefaultAction = new NSMenuItem("", NetworkActionButton_Changed)
            {
                AttributedTitle =
                    (__MainViewModel.WiFiActionType == WiFiActionTypeEnum.Default && __MainViewModel.Settings.NetworkActions.DefaultActionType == WiFiActionTypeEnum.None)
                    ? AttributedString.Create(LocalizedStrings.Instance.LocalizedString("NetworkConfig_ActionNotSet"), NSColor.SystemGrayColor, NSTextAlignment.Center)
                    : AttributedString.Create(GetActionName(WiFiActionTypeEnum.Default) + ": " + GetActionName(__MainViewModel.Settings.NetworkActions.DefaultActionType), NSColor.SystemGrayColor, NSTextAlignment.Center),
                Tag = (int)WiFiActionTypeEnum.Default
            };

            GuiNetworkActionPopUpBtn.Menu.RemoveAllItems();
            GuiNetworkActionPopUpBtn.Menu.AddItem(menuUntrusted);
            GuiNetworkActionPopUpBtn.Menu.AddItem(menuTrusted);
            //GuiNetworkActionPopUpBtn.Menu.AddItem(menuNoAction); // 'No action' available only for 'Default' action
            GuiNetworkActionPopUpBtn.Menu.AddItem(menuDefaultAction);
            GuiNetworkActionPopUpBtn.SelectItemWithTag((int)__MainViewModel.WiFiActionType);

            if (GuiNetworkActionPopUpBtn.Menu.Delegate == null)
                GuiNetworkActionPopUpBtn.Menu.Delegate = new MenuDelegateInvertHighlitedItem();
        }

        public static string GetActionName(WiFiActionTypeEnum action)
        {
            switch (action)
            {
                case WiFiActionTypeEnum.None:
                    return LocalizedStrings.Instance.LocalizedString("NetworkConfig_NoAction");

                case WiFiActionTypeEnum.Untrusted:
                    return LocalizedStrings.Instance.LocalizedString("NetworkConfig_Untrusted");

                case WiFiActionTypeEnum.Trusted:
                    return LocalizedStrings.Instance.LocalizedString("NetworkConfig_Trusted");
            }
            return LocalizedStrings.Instance.LocalizedString("NetworkConfig_Default");
        }

        void NetworkActionButton_Changed(object sender, System.EventArgs e)
        {
            NSMenuItem menuItem = sender as NSMenuItem;
            if (menuItem == null)
                return;

            __MainViewModel.SetActionForCurrentWiFi((WiFiActionTypeEnum)(int)menuItem.Tag);
        }

        private void UpdateWiFiInfoGuiData()
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => UpdateWiFiInfoGuiData());
                return;
            }

            try
            {
                UpdateToDoLabelHiddenStatus();

                WifiState state = __MainViewModel.WiFiState;
                if (__MainViewModel.Settings.IsNetworkActionsEnabled == false)
                {
                    GuiWiFiButton.Hidden = true;
                    GuiNetworkActionPopUpBtn.Hidden = true;
                    return;
                }

                NSFont wifiLabelFont = UIUtils.GetSystemFontOfSize(14, NSFontWeight.Thin);

                if (state == null || string.IsNullOrEmpty(state.Network.SSID))
                {
                    GuiWiFiButton.AttributedTitle = AttributedString.Create(LocalizedStrings.Instance.LocalizedString("Label_NoWiFiConnection"), NSColor.SystemGrayColor, NSTextAlignment.Center, wifiLabelFont);
                    GuiWiFiButton.Image = null;

                    GuiWiFiButton.Enabled = false;
                    GuiWiFiButton.Hidden = false;
                    GuiNetworkActionPopUpBtn.Hidden = true;
                }
                else
                {
                    if (state.ConnectedToInsecureNetwork)
                    {
                        GuiWiFiButton.Image = NSImage.ImageNamed("iconWiFiSmallRed");

                        string networkName = " " + state.Network.SSID + " ";
                        string fullText = networkName + "(" + LocalizedStrings.Instance.LocalizedString("Label_InsecureWiFiConnection") + ") ";

                        NSMutableAttributedString attrTitle = new NSMutableAttributedString(fullText);

                        NSStringAttributes stringAttributes0 = new NSStringAttributes();
                        stringAttributes0.ForegroundColor = __ToDoDescriptionTextColor;
                        stringAttributes0.Font = wifiLabelFont;

                        NSStringAttributes stringAttributes1 = new NSStringAttributes();
                        stringAttributes1.ForegroundColor = NSColor.SystemRedColor;
                        stringAttributes1.Font = wifiLabelFont;

                        attrTitle.AddAttributes(stringAttributes0, new NSRange(0, networkName.Length));
                        attrTitle.AddAttributes(stringAttributes1, new NSRange(networkName.Length, fullText.Length - networkName.Length));
                        attrTitle.SetAlignment(NSTextAlignment.Center, new NSRange(0, fullText.Length));

                        GuiWiFiButton.AttributedTitle = attrTitle;
                    }
                    else
                    {

                        GuiWiFiButton.Image = NSImage.ImageNamed("iconWiFiSmallBlue");
                        GuiWiFiButton.AttributedTitle = AttributedString.Create(" " + state.Network.SSID, __ToDoDescriptionTextColor, NSTextAlignment.Center, wifiLabelFont);
                    }

                    RecreateNetworkActionsButtonItems();

                    GuiWiFiButton.Enabled = true;
                    GuiWiFiButton.Hidden = false;
                    GuiNetworkActionPopUpBtn.Hidden = false;
                }
            }
            catch (Exception ex)
            {
                GuiWiFiButton.Hidden = true;
                GuiNetworkActionPopUpBtn.Hidden = true;

                Logging.Info($"{ex}");
            }
        }

        void GuiWiFiButton_Activated(object sender, EventArgs e)
        {
            GuiNetworkActionPopUpBtn.PerformClick(this);
        }

        #endregion // Trusted\Untrusted networks

        #region Pause\Resume

        NSPopover __PausePopoverMenu;

        void View_OnButtonPausePressed(object sender, EventArgs e)
        {
            if (__MainViewModel.ConnectionState != ServiceState.Connected)
                return;

            if (__MainViewModel.PauseStatus == MainViewModel.PauseStatusEnum.Resumed)
            {
                if (__PausePopoverMenu == null)
                {
                    __PausePopoverMenu = new NSPopover();
                    NSViewController pausePopoverMenuController = new NSViewController();
                    pausePopoverMenuController.View = GuiPausePopoverView;

                    __PausePopoverMenu.ContentViewController = pausePopoverMenuController;
                    __PausePopoverMenu.Behavior = NSPopoverBehavior.Transient;
                }

                if (__PausePopoverMenu.Shown)
                    __PausePopoverMenu.Close();
                else
                    __PausePopoverMenu.Show(GuiPauseButton.Bounds, GuiPauseButton, NSRectEdge.MinYEdge);
            }
            else
            {
                if (__MainViewModel.PauseStatus == MainViewModel.PauseStatusEnum.Paused)
                {
                    __MainViewModel.DisconnectCommand.Execute(null);
                }
            }
        }

        partial void PauseMenuItemButtonPressed(Foundation.NSObject sender)
        {
            if (__PausePopoverMenu != null && __PausePopoverMenu.Shown)
                __PausePopoverMenu.Close();

            NSButton btn = sender as NSButton;
            if (btn == null)
                return;

            double pauseSec = (double)btn.Tag;
            if (pauseSec <= 0)
                ShowPauseTimeDialog();
            else
            {
                if (pauseSec > 0)
                    __MainViewModel.PauseCommand.Execute(pauseSec);
            }
        }

        void View_OnPauseTimeLeftTextPressed(object sender, EventArgs e)
        {
            if (__MainViewModel.PauseStatus == MainViewModel.PauseStatusEnum.Paused)
                ShowPauseTimeDialog();
        }

        #region PauseTimeIntervalDialog
        bool __IsPauseIntervalDialogInitialized = false;

        private void InitializePauseIntervalDialog()
        {
            if (__IsPauseIntervalDialogInitialized)
                return;
            __IsPauseIntervalDialogInitialized = true;

            GuiPauseDlgHoursTextBlock.Formatter = new NumberFormatterForTextField(4, 99);
            GuiPauseDlgMinutesTextBlock.Formatter = new NumberFormatterForTextField(4, 59);
            GuiPauseDlgHoursTextBlock.PlaceholderString = "hours";
            GuiPauseDlgMinutesTextBlock.PlaceholderString = "minutes";
            GuiPauseDlgHoursTextBlock.StringValue = "1";
            GuiPauseDlgMinutesTextBlock.StringValue = "45";
            GuiPauseDlgHoursTextBlock.Alignment = NSTextAlignment.Right;
            GuiPauseDlgMinutesTextBlock.Alignment = NSTextAlignment.Left;

            CustomButtonStyles.ApplyStyleMainButtonV2(GuiPauseDlgOkBtn, "Ok");
            CustomButtonStyles.ApplyStyleSecondaryButton(GuiPauseDlgCancelBtn, "Cancel");

            GuiSetPauseIntervalWindow.WillClose += (object sender, EventArgs e) => { NSApplication.SharedApplication.EndSheet(GuiSetPauseIntervalWindow); };
        }

        public void ShowPauseTimeDialog()
        {
            MainWindowController mainWndController = AppDelegate.GetMainWindowController();
            if (mainWndController == null || mainWndController.Window == null)
                return;

            InitializePauseIntervalDialog();
            mainWndController.MakeFront();
            NSApplication.SharedApplication.BeginSheet(GuiSetPauseIntervalWindow, mainWndController.Window);
        }

        partial void OnGuiPauseDlgCancelBtnPressed(Foundation.NSObject sender)
        {
            GuiSetPauseIntervalWindow.Close();
        }

        partial void OnGuiPauseDlgOkBtnPressed(Foundation.NSObject sender)
        {
            double seconds = 0;
            try
            {
                string sh = string.IsNullOrEmpty(GuiPauseDlgHoursTextBlock.StringValue) ? "0" : GuiPauseDlgHoursTextBlock.StringValue;
                string sm = string.IsNullOrEmpty(GuiPauseDlgMinutesTextBlock.StringValue) ? "0" : GuiPauseDlgMinutesTextBlock.StringValue;
                double hours = double.Parse(sh);
                double minutes = double.Parse(sm);
                seconds = hours * 60 * 60 + minutes * 60;
            }
            catch
            {
                IVPNAlert.Show(AppDelegate.GetMainWindowController()?.Window, "Please, enter correct values");
                return;
            }

            GuiSetPauseIntervalWindow.Close();

            if (__MainViewModel.PauseStatus == MainViewModel.PauseStatusEnum.Paused)
                __MainViewModel.SetPauseTime(seconds);
            else
                __MainViewModel.PauseCommand.Execute(seconds);
        }

        partial void OnGuiPauseDlgHoursTextBlockEnter(Foundation.NSObject sender)
        {
            OnGuiPauseDlgOkBtnPressed(sender);
        }

        partial void OnGuiPauseDlgMinutesTextBlockEnter(Foundation.NSObject sender)
        {
            OnGuiPauseDlgOkBtnPressed(sender);
        }

        #endregion //PauseTimeIntervalDialog

        #endregion //Pause\Resume

    }
}
