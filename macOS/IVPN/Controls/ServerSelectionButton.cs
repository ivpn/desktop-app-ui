using System;
using AppKit;
using CoreGraphics;
using Foundation;
using IVPN.Models;

namespace IVPN
{
    public class ServerSelectionButton : ServerSelectionButtonBase
    {
        private ColorView __DisabledLayer;
        private NSTextField __PingView;
        private NSTextField __ServerName;
        private NSImageView __pingStatusImage;
        private NSImageView __selectedServerImage;

        public ServerSelectionButton(ServerLocation serverLocation) : base()
        {
            ServerLocation = serverLocation;

            const int constButtonHeight = 61;
            const int constFlagHeight = 24;

            Bordered = false;
            Title = "";
            Frame = new CGRect(0, 0, 320, constButtonHeight);

            // flag icon
            var flagView = new NSImageView();
            flagView.Frame = new CGRect(20, (constButtonHeight - constFlagHeight) / 2, constFlagHeight, constFlagHeight);
            flagView.Image = GuiHelpers.CountryCodeToImage.GetCountryFlag(serverLocation.CountryCode);
            AddSubview(flagView);

            // server name
            __ServerName = UIUtils.NewLabel(serverLocation.Name);
            __ServerName.Frame = new CGRect(49, flagView.Frame.Y + 1, 200, 18);
            __ServerName.Font = UIUtils.GetSystemFontOfSize(14.0f, NSFontWeight.Semibold);
            __ServerName.SizeToFit();
            AddSubview(__ServerName);

            // check if server name is too long
            const int maxXforSelectedIcon = 218;
            nfloat serverNameOverlapWidth = (__ServerName.Frame.X + __ServerName.Frame.Width) - maxXforSelectedIcon;
            if (serverNameOverlapWidth > 0)
            {
                CGRect oldFrame = __ServerName.Frame;
                __ServerName.Frame = new CGRect(oldFrame.X, oldFrame.Y, oldFrame.Width - serverNameOverlapWidth, oldFrame.Height);
            }

            // selected server image
            __selectedServerImage = new NSImageView();
            __selectedServerImage.Frame = new CGRect(__ServerName.Frame.X + __ServerName.Frame.Width, flagView.Frame.Y - 2, 25, 25);
            __selectedServerImage.Image = NSImage.ImageNamed("iconSelected");
            __selectedServerImage.Hidden = !ServerLocation.IsSelected;
            AddSubview(__selectedServerImage);

            // ping status image
            __pingStatusImage = new NSImageView();
            __pingStatusImage.Frame = new CGRect(238, flagView.Frame.Y, 24, 24);
            __pingStatusImage.Hidden = true;
            AddSubview(__pingStatusImage);
            UpdatePingStatusImage();

            // ping timeout info
            __PingView = UIUtils.NewLabel(GetPingTimeString(ServerLocation.PingTime));
            __PingView.Alignment = NSTextAlignment.Left;
            __PingView.Font = UIUtils.GetSystemFontOfSize(12.0f);
            __PingView.Frame = new CGRect(260, flagView.Frame.Y + 4, 60, 18);
            __PingView.TextColor = NSColor.FromRgb(180, 193, 204);
            if (ServerLocation.PingTime == 0)
                __PingView.Hidden = true;
            __PingView.SizeToFit();
            AddSubview(__PingView);

            // "disabled layer" visible only if button is disabled
            __DisabledLayer = new ColorView();
            __DisabledLayer.Frame = new CGRect(Frame.X, Frame.Y, Frame.Width, Frame.Height - 1);
            var bgClr = Colors.WindowBackground;
            __DisabledLayer.BackgroundColor = Colors.IsDarkMode ? new CGColor(bgClr.RedComponent, bgClr.GreenComponent, bgClr.BlueComponent, 0.6f) : new CGColor(1.0f, 0.6f);
            __DisabledLayer.Hidden = true;
            AddSubview(__DisabledLayer);
        }

        

        private DateTime __LastPingUpdateTime; 
        public void UpdateUI()
        {
            if (!NSThread.IsMain)
                throw new Exception($"Unable to update {this} not from UI thread");

            __selectedServerImage.Hidden = !ServerLocation.IsSelected;

            if (ServerLocation.LastPingUpdateTime != __LastPingUpdateTime && ServerLocation.LastPingUpdateTime != default(DateTime))
            {
                __LastPingUpdateTime = ServerLocation.LastPingUpdateTime;

                UpdatePingTimeText();
                UpdatePingStatusImage();
            }
        }

        public override void DrawRect(CGRect dirtyRect)
        {
            base.DrawRect(dirtyRect);

            var bgClr = Colors.WindowBackground;
            __DisabledLayer.BackgroundColor = Colors.IsDarkMode ? new CGColor(bgClr.RedComponent, bgClr.GreenComponent, bgClr.BlueComponent, 0.6f) : new CGColor(1.0f, 0.6f);
        }

        public bool DisabledForSelection
        {
            get { return __DisabledLayer.Hidden; }
            set { __DisabledLayer.Hidden = !value; }
        }

        public ServerLocation ServerLocation { get; }

        private string GetPingTimeString(int pingTime)
        {
            return String.Format("{0} ms", pingTime);
        }

        private void UpdatePingTimeText()
        {
            if (IsConfigMode)
                return;

            __PingView.StringValue = GetPingTimeString(ServerLocation.PingTime);
            __PingView.Hidden = ServerLocation.PingTime == 0;
            __PingView.SizeToFit();
        }

        private void UpdatePingStatusImage()
        {
            if (IsConfigMode)
                return;

            double pingTimeRelative = ServerLocation.PingTimeRelative;

            if (ServerLocation.PingTime <= 0)
                return;

            if (pingTimeRelative <= 0.5)
                __pingStatusImage.Image = NSImage.ImageNamed("iconStatusGood");
            else if (pingTimeRelative <= 0.8)
                __pingStatusImage.Image = NSImage.ImageNamed("iconStatusModerate");
            else
                __pingStatusImage.Image = NSImage.ImageNamed("iconStatusBad");

            __pingStatusImage.Hidden = false;
        }

        protected override void UpdateModeView()
        {
            base.UpdateModeView();

            if (IsConfigMode)
            {
                __PingView.Hidden = true;
                __pingStatusImage.Hidden = true;
            }
            else
            {
                UpdatePingTimeText();
                UpdatePingStatusImage();
            }
        }
    }
}
