//
//  IVPN Client Desktop
//  https://github.com/ivpn/desktop-app-ui
//
//  Created by Stelnykovych Alexandr.
//  Copyright (c) 2020 Privatus Limited.
//
//  This file is part of the IVPN Client Desktop.
//
//  The IVPN Client Desktop is free software: you can redistribute it and/or
//  modify it under the terms of the GNU General Public License as published by the Free
//  Software Foundation, either version 3 of the License, or (at your option) any later version.
//
//  The IVPN Client Desktop is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
//  details.
//
//  You should have received a copy of the GNU General Public License
//  along with the IVPN Client Desktop. If not, see <https://www.gnu.org/licenses/>.
//

﻿using System;
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
