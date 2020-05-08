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

﻿
using System.Collections.Generic;
using AppKit;

namespace IVPN
{
    public class EnableView
    {
        public static void Enable(NSView view, IList<NSControl> ignoreControls = null)
        {
            SetEnableAllControls (true, view, ignoreControls);
        }

        public static void Disable (NSView view, IList <NSControl> ignoreControls = null)
        {
            SetEnableAllControls (false, view, ignoreControls);
        }

        private static void SetEnableAllControls (bool isEnable, NSView view, IList<NSControl> ignoreControls = null)
        {
            foreach (var subview in view.Subviews) 
            {
                NSControl ctrl = subview as NSControl;
                if (ctrl != null) 
                {
                    if (ignoreControls != null && ignoreControls.Contains (ctrl))
                        continue;
                    ctrl.Enabled = isEnable;
                }
            }
        }

    }
}
