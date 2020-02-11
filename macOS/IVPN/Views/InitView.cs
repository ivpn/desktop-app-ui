
using System;
using Foundation;

namespace IVPN
{
    public partial class InitView : BaseView
    {
        #region Constructors

        // Called when created from unmanaged code
        public InitView(IntPtr handle) : base(handle)
        {
            Initialize();
        }
        
        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public InitView(NSCoder coder) : base(coder)
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

