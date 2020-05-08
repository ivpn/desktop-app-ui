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
using CoreGraphics;
using Foundation;

namespace IVPN
{
    [Register("CustomHyperLinkButton")]
    public class CustomHyperLinkButton : NSButton// CustomButton
    {
        #region Constructors
        public CustomHyperLinkButton() : base()
        {
            // Init
            Initialize();
        }

        public CustomHyperLinkButton(IntPtr handle) : base(handle)
        {
            // Init
            Initialize();
        }

        [Export("initWithFrame:")]
        public CustomHyperLinkButton(CGRect frameRect) : base(frameRect)
        {
            // Init
            Initialize();
        }
        #endregion 

        public NSUrl Url { get; set; }

        private void Initialize()
        {
            string title = Title;
            string alternateTitle = AlternateTitle;

            AttributedTitle = new NSAttributedString(
                title,
                new NSStringAttributes
                {
                    Font = Font,
                    ForegroundColor = NSColor.FromRgb(58, 135, 253),
                    ParagraphStyle = new NSMutableParagraphStyle { Alignment = NSTextAlignment.Left }
                });
            Bordered = false;

            try
            {
                if (string.IsNullOrEmpty(alternateTitle))
                    Url = new NSUrl(title);
                else
                    Url = new NSUrl(alternateTitle);
            }
            catch
            {
                Url = null;
            }
        }

        public override void ResetCursorRects()
        {
            base.ResetCursorRects();
            AddCursorRect(Bounds, NSCursor.PointingHandCursor);
        }

        public override void PerformClick(NSObject sender)
        {
            base.PerformClick(sender);
            OpenUrl();
        }

        public override void MouseDown(NSEvent theEvent)
        {
            base.MouseDown(theEvent);
            OpenUrl();
        }

        public override void KeyDown(NSEvent theEvent)
        {
            base.KeyDown(theEvent);
            OpenUrl();
        }

        private void OpenUrl()
        {
            if (Url == null)
                return;

            try
            {
                NSWorkspace.SharedWorkspace.OpenUrl(Url);
            }
            catch 
            {
                // ignore all
            }
        }

    }
}
