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
    /// <summary>
    /// Custom secure text field.
    /// </summary>
    [Register ("CustomTextFieldSecure")]
    public class CustomTextFieldSecure : NSTextField
    {
        #region Constructors
        public CustomTextFieldSecure ()
        {
            // Init
            Initialize ();
        }

        public CustomTextFieldSecure (IntPtr handle) : base (handle)
        {
            // Init
            Initialize ();
        }

        [Export ("initWithFrame:")]
        public CustomTextFieldSecure (CGRect frameRect) : base (frameRect)
        {
            // Init
            Initialize ();
        }

        private void Initialize ()
        {
            CornerRadius = 4;
            BorderLineWidth = 1;
            BorderColor = NSColor.FromRgb (222, 238, 253);
            //BorderColor = NSColor.FromRgb (122, 138, 153);

            Cell = new CustomTextFieldCellSecure ();
            Bordered = true;
            Bezeled = true;
            Enabled = true;
            Editable = true;
            Selectable = true;
        }
        #endregion

        public override bool PerformKeyEquivalent(NSEvent theEvent)
        {
            try
            {
                if (theEvent.Type == NSEventType.KeyDown)
                {
                    if (theEvent.ModifierFlags.HasFlag(NSEventModifierMask.CommandKeyMask))
                    {
                        switch (theEvent.CharactersIgnoringModifiers.ToLower())
                        {
                            // // DO NOT ALLOW COPYING for SECURE field!
                            //case "x":
                            //    if (NSApplication.SharedApplication.SendAction(new ObjCRuntime.Selector("cut:"), null, this)) return true;
                            //    break;
                            //case "c": 
                            //    if (NSApplication.SharedApplication.SendAction(new ObjCRuntime.Selector("copy:"), null, this)) return true;
                            //    break;
                            case "v":
                                if (NSApplication.SharedApplication.SendAction(new ObjCRuntime.Selector("paste:"), null, this)) return true;
                                break;
                            case "z":
                                if (NSApplication.SharedApplication.SendAction(new ObjCRuntime.Selector("undo:"), null, this)) return true;
                                break;
                        }
                    }
                }
            }
            catch
            {
                // ignore everything
            }

            return base.PerformKeyEquivalent(theEvent);
        }

        public override void DrawRect (CGRect dirtyRect)
        {
            base.DrawRect (dirtyRect);

            if (!Colors.IsDarkMode)
            {
                NSBezierPath betterBounds = new NSBezierPath();
                betterBounds.AppendPathWithRoundedRect(dirtyRect, CornerRadius, CornerRadius);
                betterBounds.AddClip();

                betterBounds.LineWidth = BorderLineWidth;

                if (BorderColor != null)
                    BorderColor.SetStroke();

                betterBounds.Stroke();
            }
        }

        private void SaveChanges ()
        {
            NeedsDisplay = true;
        }

        /// <summary>
        /// Corner angle radius
        /// </summary>
        public nfloat CornerRadius {
            set {
                Bordered = false;
                WantsLayer = true;
                Layer.CornerRadius = value;

                SaveChanges ();
            }
            get { return Layer.CornerRadius; }
        }

        /// <summary>
        /// Gets or sets the color of the border.
        /// </summary>
        public NSColor BorderColor {
            get { return __BorderColor; }
            set { __BorderColor = value; SaveChanges (); }
        }
        NSColor __BorderColor;

        /// <summary>
        /// Gets or sets the width of the border line.
        /// </summary>
        public nfloat BorderLineWidth {
            get { return __BorderLineWidth; }
            set { __BorderLineWidth = value; SaveChanges (); }
        }
        private nfloat __BorderLineWidth;
    }
}
