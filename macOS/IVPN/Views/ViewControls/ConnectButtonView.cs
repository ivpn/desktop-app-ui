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
using CoreGraphics;

namespace IVPN
{
    public partial class ConnectButtonView : BaseView
    {
        private NSImageView __ConnectButton;
        private NSImageView __PauseButton;

        private CGRect __PauseLeftTimeTextRect;

        private NSTrackingArea __TrackingAreaConnectBtn;
        private NSTrackingArea __TrackingAreaPauseBtn;
        private NSTrackingArea __TrackingAreaPauseLeftTimeText;

        private bool __IsMouseDownPressedConnectBtn;
        private bool __IsMouseDownPressedPauseBtn;
        private bool __IsMouseDownPressedPauseLeftTimeText;

        private NSTextField __PauseLeftTimeText;
        private NSColor __PauseLeftTimeTextOrigColor;

        public EventHandler OnButtonPressed  = delegate {}; 
        public EventHandler OnButtonPausePressed  = delegate {}; 
        public EventHandler OnPauseTimeLeftTextPressed  = delegate {}; 

        #region Constructors

        // Called when created from unmanaged code
        public ConnectButtonView (IntPtr handle) : base (handle)
        {
            Initialize ();
        }

        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public ConnectButtonView (NSCoder coder) : base (coder)
        {
            Initialize ();
        }

        // Shared initialization code
        void Initialize ()
        {
        }

        public override void ViewDidChangeEffectiveAppearance()
        {
            base.ViewDidChangeEffectiveAppearance();
            NeedsDisplay = true;
        }

        #endregion
        public readonly int CirclesRadiusIncrement = 21;

        public bool IsVisibleCircles
        {
            get { return __IsVisibleCircles; }
            set
            {
                __IsVisibleCircles = value;
                NeedsDisplay = true;
            }
        }
        private bool __IsVisibleCircles = true;

        public void SetConnectButtonRect(NSImageView btn)
        {
            __ConnectButton = btn;
            __TrackingAreaConnectBtn = new NSTrackingArea (__ConnectButton.Frame, NSTrackingAreaOptions.ActiveInKeyWindow 
                                                 | NSTrackingAreaOptions.MouseEnteredAndExited 
                                                 | NSTrackingAreaOptions.MouseMoved, this, null);
            AddTrackingArea (__TrackingAreaConnectBtn);

            NeedsDisplay = true;
        }

        
        public void SetPauseButton(NSImageView btn)
        {
            __PauseButton = btn;
            __TrackingAreaPauseBtn = new NSTrackingArea(__PauseButton.Frame, NSTrackingAreaOptions.ActiveInKeyWindow
                                                 | NSTrackingAreaOptions.MouseEnteredAndExited
                                                 | NSTrackingAreaOptions.MouseMoved, this, null);
            AddTrackingArea(__TrackingAreaPauseBtn);

            NeedsDisplay = true;
        }

        public void SetPauseLeftTimeTextField(NSTextField tf)
        {
            __PauseLeftTimeText = tf;
            __PauseLeftTimeTextOrigColor = tf.TextColor;

            CGRect buttonRect = tf.Frame;

            __PauseLeftTimeTextRect = buttonRect;
            __TrackingAreaPauseLeftTimeText = new NSTrackingArea(buttonRect, NSTrackingAreaOptions.ActiveInKeyWindow
                                                 | NSTrackingAreaOptions.MouseEnteredAndExited
                                                 | NSTrackingAreaOptions.MouseMoved, this, null);
            AddTrackingArea(__TrackingAreaPauseLeftTimeText);

            NeedsDisplay = true;
        }

        public override void DrawRect (CGRect dirtyRect)
        {
            base.DrawRect (dirtyRect);
                        
            if (__IsMouseDownPressedConnectBtn)
            {
                NSGraphicsContext c = NSGraphicsContext.CurrentContext;
                var clrChange = 10 / 255f;
                var bgClr = Colors.WindowBackground;

                c.CGContext.SetStrokeColor(NSColor.FromRgb(bgClr.RedComponent - clrChange, bgClr.GreenComponent - clrChange, bgClr.BlueComponent - clrChange).CGColor);

                c.CGContext.SetLineWidth (10);
                c.CGContext.AddEllipseInRect (__ConnectButton.Frame); 
                c.CGContext.StrokePath ();
            }

            if (__IsMouseDownPressedPauseBtn && __PauseButton != null)
            {
                NSGraphicsContext c = NSGraphicsContext.CurrentContext;
                var clrChange = 10 / 255f;
                var bgClr = Colors.WindowBackground;

                c.CGContext.SetStrokeColor(NSColor.FromRgb(bgClr.RedComponent - clrChange, bgClr.GreenComponent - clrChange, bgClr.BlueComponent - clrChange).CGColor);

                c.CGContext.SetLineWidth(5);
                c.CGContext.AddEllipseInRect(__PauseButton.Frame);
                c.CGContext.StrokePath();
            }

            if (__IsVisibleCircles == false || Colors.IsDarkMode)
                return;
            
            if (__ConnectButton == null)
                return;
            
            NSGraphicsContext context = NSGraphicsContext.CurrentContext;

            context.CGContext.SetStrokeColor(new CGColor(238 / 255f, 242 / 255f, 247 / 255f));
            context.CGContext.SetLineWidth (1);
            if (__IsMouseDownPressedConnectBtn)
                context.CGContext.SetLineWidth (2);

            var frame = __ConnectButton.Frame;
            for (int i=1; i<=4; i++)
            {
                int radius = CirclesRadiusIncrement * i;                
                context.CGContext.AddEllipseInRect (new CGRect (
                    frame.X - radius,
                    frame.Y - radius,
                    frame.Width + 2*radius,
                    frame.Height + 2*radius
                ));             
            }

            context.CGContext.StrokePath ();
        }

        public override void MouseDown (NSEvent theEvent)
        {
            base.MouseDown (theEvent);

            if (IsPointInButtonCircle(__ConnectButton.Frame, ConvertWindowLocationToViewCoordinates(theEvent.LocationInWindow)))
            {
                __IsMouseDownPressedConnectBtn = true;
                NeedsDisplay = true;
            }

            if (IsPointInButtonCircle(__PauseButton.Frame, ConvertWindowLocationToViewCoordinates(theEvent.LocationInWindow)))
            {
                __IsMouseDownPressedPauseBtn = true;
                NeedsDisplay = true;
            }

            if (IsPointInRect(__PauseLeftTimeTextRect, ConvertWindowLocationToViewCoordinates(theEvent.LocationInWindow)))
            {
                __IsMouseDownPressedPauseLeftTimeText = true;

                OnMouseHover(true);

                NeedsDisplay = true;
            }
        }

        public override void MouseUp (NSEvent theEvent)
        {
            base.MouseUp (theEvent);
            
            if (__IsMouseDownPressedConnectBtn && IsPointInButtonCircle(__ConnectButton.Frame, ConvertWindowLocationToViewCoordinates(theEvent.LocationInWindow)))
            {
                __IsMouseDownPressedConnectBtn = false;
                NeedsDisplay = true;

                OnButtonPressed(this, null);
            }
            else if (__IsMouseDownPressedPauseBtn && IsPointInButtonCircle(__PauseButton.Frame, ConvertWindowLocationToViewCoordinates(theEvent.LocationInWindow)))
            {
                __IsMouseDownPressedPauseBtn = false;
                NeedsDisplay = true;

                OnButtonPausePressed(this, null);
            }
            else if (__IsMouseDownPressedPauseLeftTimeText && IsPointInRect(__PauseLeftTimeTextRect, ConvertWindowLocationToViewCoordinates(theEvent.LocationInWindow)))
            {
                __IsMouseDownPressedPauseLeftTimeText = false;
                NeedsDisplay = true;

                OnPauseTimeLeftTextPressed(this, null);
            }
        }

        public override void MouseMoved (NSEvent theEvent)
        {
            base.MouseMoved (theEvent);

            if (__IsMouseDownPressedConnectBtn && !IsPointInButtonCircle (__ConnectButton.Frame, ConvertWindowLocationToViewCoordinates (theEvent.LocationInWindow))) 
            {
                __IsMouseDownPressedConnectBtn = false;
                NeedsDisplay = true;
            }

            if (__IsMouseDownPressedPauseBtn && !IsPointInButtonCircle(__PauseButton.Frame, ConvertWindowLocationToViewCoordinates(theEvent.LocationInWindow)))
            {
                __IsMouseDownPressedPauseBtn = false;
                NeedsDisplay = true;
            }

            bool isMouseUnderTimeLeftText = IsPointInRect(__PauseLeftTimeTextRect, ConvertWindowLocationToViewCoordinates(theEvent.LocationInWindow));
            if (__IsMouseDownPressedPauseLeftTimeText && !isMouseUnderTimeLeftText)
            {
                __IsMouseDownPressedPauseLeftTimeText = false;
                NeedsDisplay = true;
            }

            if (isMouseUnderTimeLeftText)
                OnMouseHover(true);
            else
                OnMouseHover(false);
        }

        public override void MouseExited (NSEvent theEvent)
        {
            base.MouseExited (theEvent);

            __IsMouseDownPressedConnectBtn = false;
            __IsMouseDownPressedPauseBtn = false;
            __IsMouseDownPressedPauseLeftTimeText = false;

            OnMouseHover(false);

            NeedsDisplay = true;
        }

        private CGPoint ConvertWindowLocationToViewCoordinates(CGPoint point)
        {
            return new CGPoint(point.X, point.Y - Frame.Y);
        }

        private void OnMouseHover(bool isHover)
        {
            NSColor requiredColor = (isHover) ?
                __PauseLeftTimeText.TextColor = __PauseLeftTimeTextOrigColor.ColorWithAlphaComponent(0.5f) : __PauseLeftTimeTextOrigColor;


            if (!__PauseLeftTimeText.TextColor.Equals(requiredColor))
            {
                __PauseLeftTimeText.TextColor = requiredColor;
                NeedsDisplay = true;
            }
        }

        private bool IsPointInButtonCircle(CGRect rect, CGPoint p)
        {
            nfloat radius = rect.Width / 2;
            nfloat xCenter = rect.X + rect.Width / 2;
            nfloat yCenter = rect.Y + rect.Height / 2;

            //| AB |² = (y2 - y1)² +(x2 - x1)²
            double diff = Math.Sqrt (Math.Pow (xCenter - p.X, 2) + Math.Pow (yCenter - p.Y, 2));

            if (diff > radius)
                return false;

            return true;
        }

        private bool IsPointInRect(CGRect rect, CGPoint p)
        {
            return rect.IntersectsWith(new CGRect() { X = p.X, Y = p.Y, Height = 1, Width= 1 }); 
        }
    }
}
