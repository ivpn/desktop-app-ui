using System;
using Foundation;
using AppKit;
using IVPN.ViewModels;
using IVPN.GuiHelpers;
using System.Collections.Generic;
using CoreGraphics;
using IVPN.Models.Session;

namespace IVPN
{
    public partial class LogInViewController : AppKit.NSViewController
    {
        private ViewModelLogIn __LogInViewModel;
        SubscriptionWillExpireWindowController __SubscriptionExpireWindowCrl;

        #region Constructors

        // Called when created from unmanaged code
        public LogInViewController(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public LogInViewController(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        // Call to load from the XIB/NIB file
        public LogInViewController() : base("LogInView", NSBundle.MainBundle)
        {
            Initialize();
        }

        // Shared initialization code
        void Initialize()
        {

        }

        #endregion

        public nfloat InitialHeight { get; private set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            if (View != null && View.Frame.IsEmpty == false)
                InitialHeight = View.Frame.Height;

            CustomButtonStyles.ApplyStyleMainButton(GuiButtonLogIn, LocalizedStrings.Instance.LocalizedString("Button_LogIn"));

            CustomButtonStyles.ApplyStyleNavigationButtonV2(GuiButtonStartFreeTrial, LocalizedStrings.Instance.LocalizedString("Button_StartFreeTrial"));

            GuiTextViewUser.PlaceholderString = LocalizedStrings.Instance.LocalizedString("Placeholder_Username");
            GuiTextViewUser.Cell.Title = __LogInViewModel.UserName ?? "";
            GuiTextViewUser.LineBreakMode = NSLineBreakMode.TruncatingHead;

            // ACCOUNT ID DESCRIPTION ...
            // Initialize Account ID description text with link to a Client Area
            NSData descriptionData = NSData.FromString("Your account ID can be found in the <a style=\"text-decoration:none\" href=\"https://www.ivpn.net/clientarea/login\">Client Area</a> of the website");
            NSDictionary resultDocumentAttributes;
            NSAttributedString nSAttributed = NSAttributedString.CreateWithHTML(descriptionData, out resultDocumentAttributes);

            NSStringAttributes descTextAttributes = new NSStringAttributes();
            descTextAttributes.Font = GuiTextAccountIdDescription.Font;                 // keep using preconfigured TextField font
            descTextAttributes.ForegroundColor = GuiTextAccountIdDescription.TextColor; // keep using preconfigured TextField color
            descTextAttributes.ParagraphStyle = new NSMutableParagraphStyle { Alignment = NSTextAlignment.Center };

            NSMutableAttributedString descriptionString = new NSMutableAttributedString(nSAttributed);
            descriptionString.AddAttributes(descTextAttributes, new NSRange(0, nSAttributed.Length));

            GuiTextAccountIdDescription.AllowsEditingTextAttributes = true; // it is important
            GuiTextAccountIdDescription.Selectable = true;
            GuiTextAccountIdDescription.AttributedStringValue = descriptionString;// nSAttributed;
            // ... ACCOUNT ID DESCRIPTION

            GuiButtonLogIn.Hidden = false;

            View.OnApperianceChanged += () =>
            {
                CustomButtonStyles.ApplyStyleNavigationButtonV2(GuiButtonStartFreeTrial, LocalizedStrings.Instance.LocalizedString("Button_StartFreeTrial"));
            };
        }

        //strongly typed view accessor
        public new LogInView View
        {
            get
            {
                return (LogInView)base.View;
            }
        }

        public override void ViewDidAppear()
        {
            base.ViewDidAppear();
        }

        public void Navigated()
        {
            GuiTextViewUser.BecomeFirstResponder();
        }

        void __LogInViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => __LogInViewModel_PropertyChanged(sender, e));
                return;
            }

            if (e.PropertyName.Equals(nameof(__LogInViewModel.UserName)))
                GuiTextViewUser.Cell.Title = __LogInViewModel.UserName ?? "";
        }

        NSPopover __LoginPopoverErrorInfo;
        private void ShowLoginErrorPopover(PopoverContentView contentView, NSView positioningView = null)
        {
            if (__LoginPopoverErrorInfo != null)
            {
                __LoginPopoverErrorInfo.Close();
                __LoginPopoverErrorInfo = null;
            }

            // create and show popover
            __LoginPopoverErrorInfo = new NSPopover();

            NSViewController popoverControllerConnectionError = new NSViewController();

            contentView.BackgroundColor = NSColor.FromRgb(251, 56, 65);
            popoverControllerConnectionError.View = contentView;

            __LoginPopoverErrorInfo.ContentViewController = popoverControllerConnectionError;
            __LoginPopoverErrorInfo.Behavior = NSPopoverBehavior.Transient;

            if (positioningView == null)
                positioningView = GuiTextViewUser;
            __LoginPopoverErrorInfo.Show(GuiTextViewUser.Bounds, positioningView, NSRectEdge.MinYEdge);
        }

        void __LogInViewModel_OnAccountCredentailsError(string errorText, string errorDescription = "")
        {
            string fullString = (string.IsNullOrEmpty(errorText) ? "" : errorText)
                + (string.IsNullOrEmpty(errorDescription) ? "" : errorDescription);

            // Due to we do not have solution for auto-resizing Popover according to text size
            // we are created separate view for each possible eror-message.
            // TODO: required universal implementation with text auto-resizing

            if (fullString.Contains(LocalizedStrings.Instance.LocalizedString("Error_UserNameIsEmpty")))
                ShowLoginErrorPopover(GuiPopoverContent_EnterUserrname);
            else if (fullString.Contains(LocalizedStrings.Instance.LocalizedString("Error_Authentication")))
                ShowLoginErrorPopover(GuiPopoverContent_CredentialsError);
            else if (fullString.Contains(LocalizedStrings.Instance.LocalizedString("Message_InvalidUsername")))
                ShowLoginErrorPopover(GuiPopoverContent_InvalidUserrname);
            else
                __LogInViewModel_OnError(errorText, errorDescription);
        }

        void __LogInViewModel_OnError(string errorText, string errorDescription = "")
        {
            if (string.IsNullOrEmpty(errorDescription))
                IVPNAlert.Show(errorText);
            else
                IVPNAlert.Show(errorText, errorDescription);
        }

        public void SetViewModel(ViewModelLogIn viewModel)         {             __LogInViewModel = viewModel;

            __LogInViewModel.OnAccountCredentailsError += __LogInViewModel_OnAccountCredentailsError;

            __LogInViewModel.OnWillExecute += (sender) =>
            {
                EnableView.Disable(View, ignoreControls: new List<NSControl> { GuiButtonLogIn });
                CustomButtonStyles.ApplyStyleMainButton(GuiButtonLogIn, LocalizedStrings.Instance.LocalizedString("Button_Cancel"));
                GuiProgressIndicator.Hidden = false;
                GuiProgressIndicator.StartAnimation(this);
            };

            __LogInViewModel.OnDidExecute += (sender) =>
            {
                GuiButtonLogIn.Hidden = false;

                EnableView.Enable(View, ignoreControls: new List<NSControl> { GuiButtonLogIn });
                CustomButtonStyles.ApplyStyleMainButton(GuiButtonLogIn, LocalizedStrings.Instance.LocalizedString("Button_LogIn"));
                GuiProgressIndicator.Hidden = true;
                GuiProgressIndicator.StopAnimation(this);
            };

            __LogInViewModel.OnError += __LogInViewModel_OnError;

            __LogInViewModel.OnAccountSuspended += (AccountStatus session) =>
            {
                ShowAccountExpireDialog(session);
            };

            __LogInViewModel.PropertyChanged += (sender, e) =>
            {
                __LogInViewModel_PropertyChanged(sender, e);
            };
        }

        private void PrepareForLogin()
        {
            if (__SubscriptionExpireWindowCrl != null)
                __SubscriptionExpireWindowCrl.Close();

            __LogInViewModel.UserName = GuiTextViewUser.Cell.Title;
        }
         partial void OnLogInPressed(Foundation.NSObject sender)
        {
            PrepareForLogin();
            __LogInViewModel.LogInCommand.Execute(null);
        }

        partial void OnStartFreeTrialPressed(Foundation.NSObject sender)
        {
            __LogInViewModel.StartFreeTrialCommand.Execute(null);
        }

        private void ShowAccountExpireDialog(AccountStatus sessionStatus)
        {
            AccountStatus acc = sessionStatus;
            if (acc == null)
                return;

            if (__SubscriptionExpireWindowCrl != null)
                __SubscriptionExpireWindowCrl.Close();

            __SubscriptionExpireWindowCrl = new SubscriptionWillExpireWindowController(acc, __LogInViewModel.UserName);

            NSWindow mainWindow = AppDelegate.GetMainWindowController()?.Window;
            if (mainWindow != null)
            {
                // Set window position centered to the main window
                CGRect mainWindowRect = mainWindow.Frame;
                CGRect infoWindowRect = __SubscriptionExpireWindowCrl.Window.Frame;
                CGPoint wndNewPos = new CGPoint(mainWindowRect.X + mainWindowRect.Width / 2 - infoWindowRect.Width / 2,
                                                 mainWindowRect.Y + mainWindowRect.Height / 2 - infoWindowRect.Height / 2);
                __SubscriptionExpireWindowCrl.Window.SetFrameOrigin(wndNewPos);
            }

            __SubscriptionExpireWindowCrl.ShowWindow(this);
        }
    }
}
