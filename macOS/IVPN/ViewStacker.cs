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
using System.Collections.Generic;

using CoreGraphics;

using AppKit;

namespace IVPN
{
    public class ViewStacker
    {
        private List<NSView> __Controls;

        public ViewStacker()
        {
            __Controls = new List<NSView>();
        }

        public void Add(NSView view)
        {
            view.AutoresizingMask = NSViewResizingMask.NotSizable | NSViewResizingMask.MinYMargin;
            __Controls.Add(view);
        }

        public void Replace(NSView view, NSView newView)
        {
            var index = __Controls.IndexOf(view);
            __Controls.Remove(view);
            __Controls.Insert(index, newView);
        }

        private CGSize GetBounds(float minHeight = 0)
        {
            nfloat totalHeight = 0;
            nfloat maxWidth = 0;

            foreach (var view in __Controls) 
            {
                if (view.Hidden)
                    continue;

                if (view.Frame.Height == 0) 
                { 
                    var control = view as NSControl;
                    if (control != null)
                        control.SizeToFit();
                }

                if (view.Frame.Width > maxWidth)
                    maxWidth = view.Frame.Width;

                totalHeight += view.Frame.Height;
            }

            if (totalHeight < minHeight)
                totalHeight = minHeight;

            return new CGSize(maxWidth, totalHeight);
        }

        public NSView CreateView(float minHeight = 0)
        {            
            CGSize bounds = GetBounds(minHeight);

            NSView view = new NSView(new CGRect(new CGPoint(0, 0), bounds));
            view.AutoresizingMask = NSViewResizingMask.NotSizable | NSViewResizingMask.MinYMargin;

            var yCoord = view.Frame.Bottom;

            foreach (var control in __Controls) 
            {
                if (control.Hidden)
                    continue;

                control.Frame = new CGRect(control.Frame.X, yCoord - control.Frame.Height, control.Frame.Width, control.Frame.Height);

                if (! (control is MarginControl))
                    view.AddSubview(control);

                yCoord -= control.Frame.Height;                
            }


            return view;
        }
    }
}

