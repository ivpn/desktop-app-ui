using System;

using Foundation;
using AppKit;

namespace IVPN
{
    public partial class PrivateEmailManageWindow : NSWindow
    {
        public PrivateEmailManageWindow (IntPtr handle) : base (handle)
        {
        }

        [Export ("initWithCoder:")]
        public PrivateEmailManageWindow (NSCoder coder) : base (coder)
        {
        }

        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();
        }
    }
}
