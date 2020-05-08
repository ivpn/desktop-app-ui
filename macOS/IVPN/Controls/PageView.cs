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

using CoreGraphics;

using Foundation;
using AppKit;
using CoreAnimation;

namespace IVPN
{
    [Register("PageView")]
    public class PageView: NSView
    {
        private bool __WasDrawn;
        private ILayerDrawer __drawer;

        [Export("initWithCoder:")]
        public PageView(NSCoder coder) : base(coder)
        {

        }

        public PageView(IntPtr handle):base(handle)
        {
            
        }

        public override void DrawRect(CGRect dirtyRect)
        {
            base.DrawRect(dirtyRect);

            try 
            {
                ILayerDrawer drawer = __drawer;
                if (drawer != null)
                    __drawer.DrawLayer (this, dirtyRect);
            }
            catch {}

            __WasDrawn = true;
        }

        public void SetDrawer(ILayerDrawer drawer)
        {
            if (!this.WantsLayer) 
            {
                this.WantsLayer = true;
                this.LayerContentsRedrawPolicy = NSViewLayerContentsRedrawPolicy.OnSetNeedsDisplay;
            }

            __drawer = drawer;
            NeedsDisplay = true;
        }

        public bool WasDrawn
        {
            get { return __WasDrawn; }
            set { __WasDrawn = value;}
        }
    }

    public interface ILayerDrawer
    {
        void DrawLayer (NSView view, CGRect dirtyRect);    
    }
}

