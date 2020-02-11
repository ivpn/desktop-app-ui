using System;
using Foundation;

namespace IVPN
{
    public class BaseView : AppKit.NSView
    {
        public delegate void OnApperianceChangedDelegate();
        public event OnApperianceChangedDelegate OnApperianceChanged = delegate { };

        #region Constructors

        // Called when created from unmanaged code
        public BaseView(IntPtr handle) : base(handle) { }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public BaseView(NSCoder coder) : base(coder) { }
        
        #endregion

        public override void ViewDidChangeEffectiveAppearance()
        {
            base.ViewDidChangeEffectiveAppearance();
            OnApperianceChanged();
        }
    }
}
