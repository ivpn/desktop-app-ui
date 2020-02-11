using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;

namespace IVPN
{
    public partial class LogOutView : AppKit.NSView
    {
        #region Constructors

        // Called when created from unmanaged code
        public LogOutView(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public LogOutView(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        // Shared initialization code
        void Initialize()
        {
        }

        #endregion
    }
}
