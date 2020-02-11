using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;

namespace IVPN
{
    public partial class SessionLimitView : BaseView
    {
        #region Constructors

        // Called when created from unmanaged code
        public SessionLimitView(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public SessionLimitView(NSCoder coder) : base(coder)
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
