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
    public partial class ServerSelectionView : BaseView
    {
        #region Constructors

        // Called when created from unmanaged code
        public ServerSelectionView (IntPtr handle) : base (handle)
        {
            Initialize ();
        }

        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public ServerSelectionView (NSCoder coder) : base (coder)
        {
            Initialize ();
        }

        // Shared initialization code
        void Initialize ()
        {
        }

        #endregion

        public delegate void MouseEventDelegate (NSEvent theEvent);
        public event MouseEventDelegate OnMouseDown = delegate { }; 
        public event MouseEventDelegate OnMouseUp = delegate {}; 
        public event MouseEventDelegate OnMouseMoved = delegate {}; 
        public event MouseEventDelegate OnMouseExited = delegate {};

        private NSTrackingArea __TrackingArea;
        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();

            __TrackingArea = new NSTrackingArea (Frame, NSTrackingAreaOptions.ActiveInKeyWindow 
                                                 | NSTrackingAreaOptions.MouseEnteredAndExited 
                                                 | NSTrackingAreaOptions.MouseMoved, this, null);
            AddTrackingArea (__TrackingArea);
        }

        public override void MouseDown (NSEvent theEvent)
        {
            base.MouseDown (theEvent);
            OnMouseDown(theEvent);
        }

        public override void MouseUp (NSEvent theEvent)
        {
            base.MouseUp (theEvent);
            OnMouseUp(theEvent);
        }

        public override void MouseMoved (NSEvent theEvent)
        {
            base.MouseMoved (theEvent);
            OnMouseMoved (theEvent);
        }

        public override void MouseExited (NSEvent theEvent)
        {
            base.MouseExited (theEvent);
            OnMouseExited(theEvent);
        }
    }
}
