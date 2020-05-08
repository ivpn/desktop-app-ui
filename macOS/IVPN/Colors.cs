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

namespace IVPN
{
    public static class Colors
    {
        static private NSAppearance __Appearance;
        public static bool IsDarkMode
        {
            get
            {
                if (__Appearance?.Name == NSAppearance.NameDarkAqua)
                    return true;
                return false;
            }
        }

        public static void SetAppearance(NSAppearance appearance)
        {
            __Appearance = appearance;
        }

        public static NSColor WindowBackground => (IsDarkMode) ? NSColor.FromRgb(28, 28, 30) : NSColor.FromRgb(248, 251, 255);
        public static NSColor ConnectButtonTextColor => (IsDarkMode) ? NSColor.White : NSColor.FromRgb(51, 77, 102);
        public static NSColor ConnectiongAnimationCircleColor => (IsDarkMode) ? NSColor.FromRgb(57, 143, 230) : NSColor.FromRgb(176, 209, 238);

        public static NSColor IntroductionBackground => (IsDarkMode) ? NSColor.FromRgb(67, 68, 70) : NSColor.White;
        public static NSColor IntroductionTextColor => (IsDarkMode) ? NSColor.White : NSColor.FromRgb(38, 57, 77);
        
        public static NSColor SwitcherOffBackgroundColor => (IsDarkMode) ? NSColor.FromRgb(98, 109, 118) : NSColor.FromRgb(249, 250, 252);
        public static NSColor SwitcherOffBorderColor => (IsDarkMode) ? SwitcherOffBackgroundColor : NSColor.FromRgb(225, 231, 237);
        public static NSColor SwitcherCircleColor => (IsDarkMode) ? NSColor.FromRgb(45, 46, 48) : NSColor.FromRgb(255, 255, 255);
        public static NSColor SwitcherCircleShadowColor => (IsDarkMode) ? NSColor.FromRgba(0, 0, 0, 0) : NSColor.FromRgb(200, 206, 214);

        public static NSColor HopBtnBorderColor => (IsDarkMode) ? WindowBackground : NSColor.FromRgb(223, 223, 235);
        public static NSColor HopBtnColor => (IsDarkMode) ? NSColor.FromRgb(45, 46, 48) : WindowBackground;
        public static NSColor HopBtnTextEnabledColor => (IsDarkMode) ? NSColor.White : NSColor.FromRgb(38, 57, 77);
        public static NSColor HopBtnTextDisabledColor => (IsDarkMode) ? NSColor.FromRgb(128, 141, 154) : NSColor.FromRgb(122, 138, 153);
        public static NSColor HopBtnPressedColor => (IsDarkMode)
                                                        ? NSColor.FromRgb(HopBtnColor.RedComponent * 0.90f, HopBtnColor.GreenComponent * 0.90f, HopBtnColor.BlueComponent * 0.90f)
                                                        : NSColor.FromRgb(HopBtnColor.RedComponent * 0.95f, HopBtnColor.GreenComponent * 0.95f, HopBtnColor.BlueComponent * 0.95f);

        public static NSColor HeaderNavigationBtnColor => (IsDarkMode) ? NSColor.FromRgb(58, 58, 60) : NSColor.FromRgb(190, 203, 211);
        public static NSColor HeaderNavigationBtnBorderColor => (IsDarkMode) ? HeaderNavigationBtnColor : NSColor.FromRgb(216, 223, 230);
        public static NSColor HeaderNavigationBtnTextColor => (IsDarkMode) ? NSColor.FromRgba(255, 255, 255, 127) : NSColor.White;


        public static NSColor SecondaryBtnGradient1Color => (IsDarkMode) ? NSColor.FromRgb(45, 46, 48) : NSColor.FromRgb(251, 252, 253);
        public static NSColor SecondaryBtnGradient2Color => (IsDarkMode) ? NSColor.FromRgb(45, 46, 48) : NSColor.FromRgb(240, 244, 247);
        public static NSColor SecondaryBtnTextColor => (IsDarkMode) ? NSColor.FromRgb(59, 152, 252) : NSColor.FromRgb(23, 143, 230);
        public static NSColor SecondaryBtnBorderColor => (IsDarkMode) ? SecondaryBtnGradient1Color : NSColor.FromRgb(186, 202, 212);

        public static NSColor BtnAccountExpireBackground => (IsDarkMode) ? SecondaryBtnGradient1Color : NSColor.White;
        public static NSColor BtnAccountExpireBorder => (IsDarkMode) ? BtnAccountExpireBackground : NSColor.FromRgb(216, 223, 230);
        public static NSColor BtnAccountExpireFont => (IsDarkMode) ? NSColor.White : NSColor.FromRgb(51, 77, 102);


    }
}
