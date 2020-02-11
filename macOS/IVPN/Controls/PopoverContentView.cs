using System;
using AppKit;
using Foundation;
using CoreGraphics;

/////////////////////////////////////////////////////////////////////////////// 
// Implementation Content view for Popover:
//     - addidng posibility to set background color for Popover
// Details:
//      https://stackoverflow.com/questions/19978620/how-to-change-nspopover-background-color-include-triangle-part
/////////////////////////////////////////////////////////////////////////////// 
namespace IVPN
{
	[Register("PopoverContentView")]
	public class PopoverContentView : NSView
    {
		//PopoverBackgroundView __BackgroundView;

		#region Constructors
		public PopoverContentView() { }

		public PopoverContentView(IntPtr handle) : base(handle) { }

        [Export("initWithFrame:")]
		public PopoverContentView(CGRect frameRect) : base(frameRect) { }
		#endregion

        public NSColor BackgroundColor { get; set; }

		public override void ViewDidMoveToWindow()
		{
			NSView frameView = Window?.ContentView?.Superview;
			if (frameView == null || BackgroundColor == null)
				return;

			NSView backgroundView = new NSView(frameView.Bounds);

			backgroundView.WantsLayer = true;
			if (backgroundView.Layer != null)
				backgroundView.Layer.BackgroundColor = BackgroundColor.CGColor;
			backgroundView.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;

			frameView.AddSubview(backgroundView, NSWindowOrderingMode.Below, frameView);
		}
    }
}
