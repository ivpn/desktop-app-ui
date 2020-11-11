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

        public override void DrawRect(CGRect dirtyRect)
        {
            base.DrawRect(dirtyRect);

            var bgClr = Colors.WindowBackground;
            base.Layer.BackgroundColor = Colors.IsDarkMode ? bgClr.CGColor : NSColor.White.CGColor;
        }
    }
}
