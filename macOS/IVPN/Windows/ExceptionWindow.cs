using System;

using Foundation;
using AppKit;

namespace IVPN
{
    public partial class ExceptionWindow : NSWindow
    {
        public ExceptionWindow (IntPtr handle) : base (handle)
        {
        }

        [Export ("initWithCoder:")]
        public ExceptionWindow (NSCoder coder) : base (coder)
        {
        }

        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();
        }
    }
}
