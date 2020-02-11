using System;
using Foundation;

namespace IVPN
{
    public partial class LogInView : BaseView
    {
        #region Constructors

        // Called when created from unmanaged code
        public LogInView (IntPtr handle) : base (handle)
        {
            Initialize ();
        }

        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public LogInView (NSCoder coder) : base (coder)
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
