
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
