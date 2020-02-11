using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Foundation;
using IVPN.Interfaces;

namespace IVPN
{
    /// <summary>
    /// Introduction functionality are moved to separate file (MainWindowController is partial class)
    /// </summary>
    public partial class MainWindowController : AppKit.NSWindowController, ISynchronizeInvoke, IMainWindow, ILayerDrawer
    {
        /// <summary>
        /// Introduction steps
        /// </summary>
        private enum IntroductionStageEnum
        {
            NotStarted,
            Starting,
            Welcome,
            ConnectBtn,
            Firewall,
            Servers,
            Finished
        }

        /// <summary>
        /// Current intorduction step
        /// </summary>
        private IntroductionStageEnum __InotroductionStage = IntroductionStageEnum.NotStarted;

        

        /// <summary>
        /// Initialize introduction functionality
        /// </summary>
        private void StartIntroductionIfNecesary ()
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => StartIntroductionIfNecesary());
                return;
            }

            if (__Settings.IsFirstIntroductionDone) 
                return;
            
            if (__InotroductionStage != IntroductionStageEnum.NotStarted) 
                return;

            try 
            {
                __Settings.IsFirstIntroductionDone = true;
                __Settings.Save ();
            }
            catch (Exception ex)
            {
                Logging.Info (string.Format ("{0}", ex));
                return;
            }

            GuiIntroductionPanelWelcome.WillClose += OnIntroductionPanel_WillClose;
            GuiIntroductionPanelConnectBtn.WillClose += OnIntroductionPanel_WillClose;
            GuiIntroductionPanelFirewall.WillClose += OnIntroductionPanel_WillClose;
            GuiIntroductionPanelServers.WillClose += OnIntroductionPanel_WillClose;

            UpdateIntroductionApperiance();
            
            __InotroductionStage = IntroductionStageEnum.Starting;

            ShowNextIntoductionStep ();
        }

        private void UpdateIntroductionApperiance()
        {            
            if (Colors.IsDarkMode)
                GuiIntroLogoImage.Image = NSImage.ImageNamed("iconLogoDark");
            else
                GuiIntroLogoImage.Image = NSImage.ImageNamed("iconLogo");

            CustomButtonStyles.ApplyStyleMainButton(GuiIntroBtnWelcomeShowMe, LocalizedStrings.Instance.LocalizedString("Button_Introduction_ShowMe"));
            GuiIntroductionPanelWelcome.BackgroundColor = Colors.IntroductionBackground;
            GuiIntroLabelWelcome.StringValue = LocalizedStrings.Instance.LocalizedString("Label_IntroWelcome");
            GuiIntroLabelWelcome.TextColor = Colors.IntroductionTextColor;

            CustomButtonStyles.ApplyStyleMainButton(GuiIntroBtnConnect, LocalizedStrings.Instance.LocalizedString("Button_Introduction_Continue"));
            GuiIntroductionPanelConnectBtn.BackgroundColor = Colors.IntroductionBackground;
            GuiIntroLabelConnectTitle.StringValue = LocalizedStrings.Instance.LocalizedString("Label_IntroConnectTitle");
            GuiIntroLabelConnectText.StringValue = LocalizedStrings.Instance.LocalizedString("Label_IntroConnectText");
            GuiIntroLabelConnectTitle.TextColor = Colors.IntroductionTextColor;
            GuiIntroLabelConnectText.TextColor = Colors.IntroductionTextColor;

            CustomButtonStyles.ApplyStyleMainButton(GuiIntroBtnFirewallContinue, LocalizedStrings.Instance.LocalizedString("Button_Introduction_Continue"));
            GuiIntroBtnFirewallContinue.TitleFont = UIUtils.GetSystemFontOfSize(16f, NSFontWeight.Medium);

            GuiIntroductionPanelFirewall.BackgroundColor = Colors.IntroductionBackground;
            GuiIntroLabelFirewallTitle.StringValue = LocalizedStrings.Instance.LocalizedString("Label_IntroFirewallTitle");
            GuiIntroLabelFirewallText.StringValue = LocalizedStrings.Instance.LocalizedString("Label_IntroFirewallText");
            GuiIntroLabelFirewallTitle.TextColor = Colors.IntroductionTextColor;
            GuiIntroLabelFirewallText.TextColor = Colors.IntroductionTextColor;

            CustomButtonStyles.ApplyStyleMainButton(GuiIntroBtnServersClose, LocalizedStrings.Instance.LocalizedString("Button_Introduction_Close"));

            GuiIntroductionPanelServers.BackgroundColor = Colors.IntroductionBackground;
            GuiIntroLabelServersTitle.StringValue = LocalizedStrings.Instance.LocalizedString("Label_IntroServersTitle");
            GuiIntroLabelServersText.StringValue = LocalizedStrings.Instance.LocalizedString("Label_IntroServersText");
            GuiIntroLabelServersTitle.TextColor = Colors.IntroductionTextColor;
            GuiIntroLabelServersText.TextColor = Colors.IntroductionTextColor;
        }

        private void InitializeDialogPositions()
        {
            Window.Center ();

            var windowFrame = Window.Frame;
            CGRect connectBtnFrame = __MainViewController.GetConnectButtonViewRect ();
            CGRect serversSelectionFrame = __MainViewController.GetServerSelectionViewRect ();
            CGRect firewallFrame = __MainViewController.GetFirewallControlViewRect();
             
            // Welcome
            var frame = GuiIntroductionPanelWelcome.Frame;
            var x = windowFrame.X + windowFrame.Width / 2 - frame.Width / 2;
            var y = windowFrame.Y + windowFrame.Height / 2 - frame.Height / 2;
            GuiIntroductionPanelWelcome.SetFrameOrigin (new CGPoint (x, y));

            // Connect button
            frame = GuiIntroductionPanelConnectBtn.Frame;
            x = windowFrame.X - frame.Width + connectBtnFrame.X - 10;
            y = windowFrame.Y + connectBtnFrame.Y + connectBtnFrame.Height / 2 - frame.Height /2;
            GuiIntroductionPanelConnectBtn.SetFrameOrigin (new CGPoint (x, y));

            // Firewall 
            frame = GuiIntroductionPanelFirewall.Frame;
            x = windowFrame.X + firewallFrame.X + firewallFrame.Width - 5;
            y = windowFrame.Y + firewallFrame.Y - frame.Height - 10;
            GuiIntroductionPanelFirewall.SetFrameOrigin (new CGPoint (x, y));

            // Servers
            frame = GuiIntroductionPanelServers.Frame;
            x = windowFrame.X + windowFrame.Width - 50;
            y = windowFrame.Y + serversSelectionFrame.Height + 10;
            GuiIntroductionPanelServers.SetFrameOrigin (new CGPoint (x, y));
        }

        /// <summary>
        /// All introduction dialogs are modal
        /// Here we catching all close events of introduction dialogs
        /// </summary>
        void OnIntroductionPanel_WillClose (object sender, System.EventArgs e)
        {
            // continue closing window AND immediately show next introduction step
            System.Threading.Tasks.Task.Run (() => 
            {
                InvokeOnMainThread (() => {
                    ShowNextIntoductionStep ();
                });
            });
        }

        /// <summary>
        /// Show next introduction step
        /// </summary>
        private void ShowNextIntoductionStep ()
        {

            NSPanel panelToShow = null;
            switch (__InotroductionStage) 
            {
            case IntroductionStageEnum.Starting:
                panelToShow = GuiIntroductionPanelWelcome;
                __InotroductionStage = IntroductionStageEnum.Welcome;
                break;
            case IntroductionStageEnum.Welcome:
                panelToShow = GuiIntroductionPanelConnectBtn;
                __InotroductionStage = IntroductionStageEnum.ConnectBtn;
                break;
            case IntroductionStageEnum.ConnectBtn:
                panelToShow = GuiIntroductionPanelFirewall;
                __InotroductionStage = IntroductionStageEnum.Firewall;
                break;
            case IntroductionStageEnum.Firewall:
                panelToShow = GuiIntroductionPanelServers;
                __InotroductionStage = IntroductionStageEnum.Servers;
                break;
            case IntroductionStageEnum.Servers:
                __InotroductionStage = IntroductionStageEnum.Finished;
                break;
            }
            
            // fit transparent view to window size
            GuiTopTransparentView.Frame = MainPageView.Frame;
            // request View to redraw
            ((PageView)GuiTopTransparentView).SetDrawer (this);

            NSApplication.SharedApplication.StopModal ();

            if (panelToShow != null) 
            {
                InitializeDialogPositions ();

                panelToShow.ParentWindow = Window;
                panelToShow.MakeKeyAndOrderFront (this);
                NSApplication.SharedApplication.RunModalForWindow (panelToShow);
            }
        }

        /// <summary>
        /// We are using separate layer for higliting elements on a main view
        /// This method is highliting GUI element depends on current introduction step
        /// </summary>
        public void DrawLayer (NSView view, CGRect dirtyRect)
        {
            var bg = Colors.WindowBackground;
            NSColor fillColor = NSColor.FromRgba(bg.RedComponent, bg.GreenComponent, bg.BlueComponent, 0.4f);

            NSColor strokeColor = NSColor.FromRgb (50, 158, 230);
            nfloat strokeWidth = 2;

            if (__InotroductionStage != IntroductionStageEnum.Finished)
            {
                NSBezierPath fillRect = NSBezierPath.FromRect (dirtyRect);
                fillColor.SetFill ();
                fillRect.Fill ();
            }

            if (__InotroductionStage == IntroductionStageEnum.Firewall) {
                CGRect fwRect = __MainViewController.GetFirewallControlViewRect();

                NSBezierPath firewallPath = NSBezierPath.FromRoundedRect (fwRect, fwRect.Height / 2, fwRect.Height / 2);
                firewallPath.LineWidth = strokeWidth;
                strokeColor.SetStroke ();
                firewallPath.Stroke ();
            }

            if (__InotroductionStage == IntroductionStageEnum.ConnectBtn) {
                // CONNECT BUTTON
                CGRect circleRect = __MainViewController.GetConnectButtonViewRect ();
                NSBezierPath connectBthPth = NSBezierPath.FromRoundedRect (circleRect, circleRect.Width / 2, circleRect.Height / 2);
                connectBthPth.LineWidth = strokeWidth;
                strokeColor.SetStroke ();
                connectBthPth.Stroke ();
            }

            if (__InotroductionStage == IntroductionStageEnum.Servers) {
                // SERVERS SELECTION
                CGRect serversRect = __MainViewController.GetServerSelectionViewRect ();
                serversRect = new CGRect (serversRect.X + 3, serversRect.Y + 3, serversRect.Width - 6, serversRect.Height - 6);
                NSBezierPath serversViewPath = NSBezierPath.FromRoundedRect (serversRect, 4, 4);
                serversViewPath.LineWidth = strokeWidth;
                strokeColor.SetStroke ();
                serversViewPath.Stroke ();
            }
        }

        #region Processing buton actions
        partial void OnIntroBtnWelcomePressed (Foundation.NSObject sender)
        {
            GuiIntroductionPanelWelcome.Close ();
        }

        partial void OnIntroBtnConnectPressed(Foundation.NSObject sender)
        {
            GuiIntroductionPanelConnectBtn.Close ();
        }

        partial void OnIntroBtnFirewallContinuePressed (Foundation.NSObject sender)
        {
            GuiIntroductionPanelFirewall.Close ();
        }

        partial void OnIntroBtnFirewallClosePressed (Foundation.NSObject sender)
        {
            __InotroductionStage = IntroductionStageEnum.Finished;
            GuiIntroductionPanelFirewall.Close ();
        }

        partial void OnIntroBtnServersClosePressed (Foundation.NSObject sender)
        {
            __InotroductionStage = IntroductionStageEnum.Finished;
            GuiIntroductionPanelServers.Close ();
        }

        partial void OnIntroBtnServersConnectPressed (Foundation.NSObject sender)
        {
            __InotroductionStage = IntroductionStageEnum.Finished;
            GuiIntroductionPanelServers.Close ();

            __MainViewModel.ConnectCommand.Execute (null);
        }

        #endregion //Processing buton action

    }
}