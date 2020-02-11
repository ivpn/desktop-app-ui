using System;

using CoreGraphics;

using AppKit;

namespace IVPN
{

    // The only purpose of this control is to add margings for the ViewStacker class
    // This control should never be added to the view and drawn
    public class MarginControl: NSControl
    {        
        public MarginControl(float margin=0): base(new CGRect(0, 0, 0, margin))
        {
            
        }
    }
}

