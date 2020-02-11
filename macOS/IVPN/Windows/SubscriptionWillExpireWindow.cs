using System;

using Foundation;
using AppKit;

namespace IVPN
{
    public partial class SubscriptionWillExpireWindow : NSWindow
    {
        public SubscriptionWillExpireWindow (IntPtr handle) : base (handle)
        {
        }

        [Export ("initWithCoder:")]
        public SubscriptionWillExpireWindow (NSCoder coder) : base (coder)
        {
        }

        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();
        }
    }
}
