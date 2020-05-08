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

using System;

using Foundation;
using AppKit;

using CoreGraphics;

namespace IVPN
{

    [Register("ColorView")]
    public class ColorView: NSView
    {
        private CGColor __BackgroundColor;

        public ColorView(): base()
        {
            
        }

        public ColorView(IntPtr handle): base(handle)
        {
            BackgroundColor = new CGColor(1.0f, 1.0f, 1.0f);
        }

        [Export("initWithCoder:")]
        public ColorView(NSCoder coder) : base(coder)
        {
            
        }

        public override void DrawRect(CGRect dirtyRect)
        {
            CGContext context = NSGraphicsContext.CurrentContext.GraphicsPort;
            context.SetFillColor(__BackgroundColor);
            context.FillRect(dirtyRect);

            base.DrawRect(dirtyRect);

            if (BorderLineWidth > 0) 
            {
                NSBezierPath bounds = new NSBezierPath ();
                bounds.AppendPathWithRect (dirtyRect);
                bounds.AddClip ();

                bounds.LineWidth = BorderLineWidth;

                if (BorderColor != null)
                    BorderColor.SetStroke ();

                bounds.Stroke ();
            }
        }

        public CGColor BackgroundColor
        {
            get { return __BackgroundColor; }

            set 
            {
                __BackgroundColor = value;
                NeedsDisplay = true;
            }
        }

        /// <summary>
        /// Gets or sets the width of the border line.
        /// </summary>
        public nfloat BorderLineWidth 
        {
            get { return __BorderLineWidth; }
            set 
            { 
                __BorderLineWidth = value; 
                NeedsDisplay = true; 
            }
        }
        private nfloat __BorderLineWidth;

        /// <summary>
        /// Gets or sets the color of the border.
        /// </summary>
        public NSColor BorderColor 
        {
            get { return __BorderColor; }
            set 
            { 
                __BorderColor = value; 
                NeedsDisplay = true; 
            }
        }
        NSColor __BorderColor;
    }
}

