using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;

namespace IVPN
{
    public partial class FastestServerConfigView : AppKit.NSView
    {
        #region Constructors

        // Called when created from unmanaged code
        public FastestServerConfigView(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public FastestServerConfigView(NSCoder coder) : base(coder)
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
