using AppKit;
using CoreGraphics;

namespace IVPN
{
    public class ServerFastestSelectionButton : ServerSelectionButtonBase
    {
        private NSTextField __Title;
        private NSImageView __Image;

        public ServerFastestSelectionButton(bool isSelected) : base()
        {
            const int constButtonHeight = 61;
            const int constImgHeight = 24;
            const string title = "Fastest server";

            Title = "";

            // flag icon
            var flagView = new NSImageView();
            flagView.Frame = new CGRect(20, (constButtonHeight - constImgHeight) / 2, constImgHeight, constImgHeight);
            flagView.Image = NSImage.ImageNamed("iconAutomaticServerSelection");
            AddSubview(flagView);

            // title
            __Title = UIUtils.NewLabel(title);
            __Title.Frame = new CGRect(49, flagView.Frame.Y + 3, 200, 18);
            __Title.Font = UIUtils.GetSystemFontOfSize(14.0f, NSFontWeight.Semibold);
            __Title.SizeToFit();
            AddSubview(__Title);

            // image
            __Image = new NSImageView();
            __Image.Frame = new CGRect(__Title.Frame.X + __Title.Frame.Width, flagView.Frame.Y , 25, 25);
            __Image.Image = NSImage.ImageNamed("iconSelected");
            __Image.Hidden = true;
            AddSubview(__Image);

            IsSelected = isSelected;
        }

        public bool IsSelected
        {
            get { return !__Image.Hidden; }
            set { __Image.Hidden = !value; }
        }
    }
}
