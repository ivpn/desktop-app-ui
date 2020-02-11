using System;
using Foundation;
using AppKit;

namespace IVPN
{
    public partial class ServerSelectionView : BaseView
    {
        #region Constructors

        // Called when created from unmanaged code
        public ServerSelectionView (IntPtr handle) : base (handle)
        {
            Initialize ();
        }

        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public ServerSelectionView (NSCoder coder) : base (coder)
        {
            Initialize ();
        }

        // Shared initialization code
        void Initialize ()
        {
        }

        #endregion

        public delegate void MouseEventDelegate (NSEvent theEvent);
        public event MouseEventDelegate OnMouseDown = delegate { }; 
        public event MouseEventDelegate OnMouseUp = delegate {}; 
        public event MouseEventDelegate OnMouseMoved = delegate {}; 
        public event MouseEventDelegate OnMouseExited = delegate {};

        private NSTrackingArea __TrackingArea;
        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();

            __TrackingArea = new NSTrackingArea (Frame, NSTrackingAreaOptions.ActiveInKeyWindow 
                                                 | NSTrackingAreaOptions.MouseEnteredAndExited 
                                                 | NSTrackingAreaOptions.MouseMoved, this, null);
            AddTrackingArea (__TrackingArea);
        }

        public override void MouseDown (NSEvent theEvent)
        {
            base.MouseDown (theEvent);
            OnMouseDown(theEvent);
        }

        public override void MouseUp (NSEvent theEvent)
        {
            base.MouseUp (theEvent);
            OnMouseUp(theEvent);
        }

        public override void MouseMoved (NSEvent theEvent)
        {
            base.MouseMoved (theEvent);
            OnMouseMoved (theEvent);
        }

        public override void MouseExited (NSEvent theEvent)
        {
            base.MouseExited (theEvent);
            OnMouseExited(theEvent);
        }
    }
}
