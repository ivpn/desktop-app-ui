using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;

namespace IVPN
{
    public partial class ServersView : AppKit.NSView
    {
        #region Constructors

        // Called when created from unmanaged code
        public ServersView (IntPtr handle) : base (handle)
        {
            Initialize ();
        }

        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public ServersView (NSCoder coder) : base (coder)
        {
            Initialize ();
        }

        // Shared initialization code
        void Initialize ()
        {
        }

        #endregion
    }
}
