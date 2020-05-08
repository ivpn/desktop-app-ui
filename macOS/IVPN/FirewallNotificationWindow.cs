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

using Foundation;
using AppKit;

namespace IVPN
{
    /// <summary>
    /// Firewall notification window.
    /// </summary>
    public partial class FirewallNotificationWindow : NSWindow, GuiHelpers.IClickDetectable
    {
        private readonly GuiHelpers.ClickDetection _clickDetector = new GuiHelpers.ClickDetection ();

		public event GuiHelpers.OnClickDelegate OnClick;
		public event GuiHelpers.OnDoubleClickDelegate OnDoubleClick;

        public FirewallNotificationWindow (IntPtr handle) : base (handle)
        {
            Initialize ();
        }

        [Export ("initWithCoder:")]
        public FirewallNotificationWindow (NSCoder coder) : base (coder)
        {
            Initialize ();
        }

        private void Initialize()
        {
            // possibility to drag by window body (we have no title bar)
            this.MovableByWindowBackground = true;
            // topmost window
            this.Level = NSWindowLevel.Floating;

            // clicks detection/notification
            _clickDetector.OnClick += () => 
            {
                OnClick?.Invoke ();
            };
            _clickDetector.OnDoubleClick += () => 
            {
                OnDoubleClick?.Invoke ();
            };
        }

        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();
        }

        public override void MouseUp (NSEvent theEvent)
        {
            base.MouseUp (theEvent);
            _clickDetector.MouseUp ();
        }

        public override void MouseDown (NSEvent theEvent)
        {
            base.MouseDown (theEvent);
            _clickDetector.MouseDown ();
        }
    }
}
