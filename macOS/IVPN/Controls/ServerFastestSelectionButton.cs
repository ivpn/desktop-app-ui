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

﻿using AppKit;
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
