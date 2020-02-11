using System;

using Foundation;
using AppKit;

namespace IVPN
{
    public partial class PrivateEmailGeneratedWindow : NSWindow
    {
        public PrivateEmailGeneratedWindow (IntPtr handle) : base (handle)
        {
        }

        [Export ("initWithCoder:")]
        public PrivateEmailGeneratedWindow (NSCoder coder) : base (coder)
        {
        }

        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();
        }
    }
}
