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
    public class ServerCheckButton : ServerSelectionButtonBase
    {
        public delegate void OnCheckedDelegate();
        public event OnCheckedDelegate OnChecked = delegate { };

        private NSTextField __ServerName;
        NSButton __CheckBtnElement;
        public VpnProtocols.VpnServerInfoBase ServerInfo { get; }

        public bool IsChecked
        {
            get => __CheckBtnElement.State == NSCellStateValue.On;
            set
            {
                bool currentValue = __CheckBtnElement.State == NSCellStateValue.On;
                if (currentValue == value)
                    return;

                __CheckBtnElement.State = (value) ? NSCellStateValue.On : NSCellStateValue.Off;
                OnChecked();
            }
        }

        public ServerCheckButton(VpnProtocols.VpnServerInfoBase serverInfo, bool isChecked = false)
        {
            ServerInfo = serverInfo;

            const int constButtonHeight = 61;
            const int constFlagHeight = 24;

            Bordered = false;
            Title = "";
            Frame = new CGRect(0, 0, 320, constButtonHeight);

            // check Box
            __CheckBtnElement = new NSButton();
            __CheckBtnElement.Title = "";
            __CheckBtnElement.Frame = new CGRect(20, (constButtonHeight - constFlagHeight) / 2, constFlagHeight, constFlagHeight);
            __CheckBtnElement.SetButtonType(NSButtonType.Switch);
            AddSubview(__CheckBtnElement);
            int checkOffset = 33;

            // flag icon
            var flagView = new NSImageView();
            flagView.Frame = new CGRect(checkOffset + 20, (constButtonHeight - constFlagHeight) / 2, constFlagHeight, constFlagHeight);
            flagView.Image = GuiHelpers.CountryCodeToImage.GetCountryFlag(ServerInfo.CountryCode);
            AddSubview(flagView);

            // server name
            __ServerName = UIUtils.NewLabel(ServerInfo.FullName);
            __ServerName.Frame = new CGRect(checkOffset + 49, flagView.Frame.Y + 1, 200, 18);
            __ServerName.Font = UIUtils.GetSystemFontOfSize(14.0f, NSFontWeight.Semibold);
            __ServerName.SizeToFit();
            AddSubview(__ServerName);

            // check if server name is too long
            const int maxXforSelectedIcon = 230;
            nfloat serverNameOverlapWidth = (__ServerName.Frame.X + __ServerName.Frame.Width) - maxXforSelectedIcon;
            if (serverNameOverlapWidth > 0)
            {
                CGRect oldFrame = __ServerName.Frame;
                __ServerName.Frame = new CGRect(oldFrame.X, oldFrame.Y, oldFrame.Width - serverNameOverlapWidth, oldFrame.Height);
            }

            Activated += (sender, e) =>
            {
                IsChecked = !IsChecked;
            };

            __CheckBtnElement.Activated += (sender, e) =>
            {
                OnChecked();
            };

            IsChecked = isChecked;
        }
    }
}
