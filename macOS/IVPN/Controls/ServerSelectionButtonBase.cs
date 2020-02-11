using System;
using AppKit;
using CoreGraphics;

namespace IVPN
{
    public abstract class ServerSelectionButtonBase : NSButton
    {
        public event EventHandler OnConfigButtonPressed = delegate { };

        private NSBox __HorisontalLine;
        private NSButton __ConfigButton;

        public ServerSelectionButtonBase() : base()
        {
            const int constButtonHeight = 61;

            Bordered = false;
            Frame = new CGRect(0, 0, 320, constButtonHeight);

            // horizontal line at the buttom
            __HorisontalLine = new NSBox(new CGRect(new CGPoint(0, constButtonHeight - 1), new CGSize(320, 1)));
            __HorisontalLine.BorderType = NSBorderType.LineBorder;
            __HorisontalLine.BorderWidth = 1;
            __HorisontalLine.BoxType = NSBoxType.NSBoxSeparator;
            AddSubview(__HorisontalLine);

            // ============================ ITEMS FOR CONFIG MODE
            NSImage cfgImg = NSImage.ImageNamed("iconPreferences");
            __ConfigButton = new NSButton();
            __ConfigButton.SetButtonType(NSButtonType.MomentaryPushIn);
            __ConfigButton.Bordered = false;
            __ConfigButton.ImageScaling = NSImageScale.None;
            __ConfigButton.ImagePosition = NSCellImagePosition.ImageOnly;
            __ConfigButton.BezelStyle = NSBezelStyle.SmallSquare;
            __ConfigButton.Image = cfgImg;
            __ConfigButton.Frame = new CGRect(320 - 50, (constButtonHeight - cfgImg.Size.Height) / 2, cfgImg.Size.Width, cfgImg.Size.Height);
            __ConfigButton.Activated += (object sender, System.EventArgs e) =>
            {
                OnConfigButtonPressed(this, e);
            };
            AddSubview(__ConfigButton);
            __ConfigButton.Hidden = true;
        }

        public bool IsConfigMode
        {
            get => __IsConfigMode;
            set
            {
                bool isUpdateRequired = __IsConfigMode != value;
                __IsConfigMode = value;

                if (isUpdateRequired)
                    UpdateModeView();
            }
        }
        private bool __IsConfigMode;

        protected virtual void UpdateModeView()
        {
            if (IsConfigMode)
                __ConfigButton.Hidden = false;
            else
                __ConfigButton.Hidden = true;
        }
    }
}
