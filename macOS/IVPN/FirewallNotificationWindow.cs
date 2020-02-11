using System;

using Foundation;
using AppKit;

namespace IVPN
{
    /// <summary>
    /// Firewall notification window.
    /// </summary>
    public partial class FirewallNotificationWindow : NSWindow, GuiHelpers.IClickDetectable
    {
        private readonly GuiHelpers.ClickDetection _clickDetector = new GuiHelpers.ClickDetection ();

		public event GuiHelpers.OnClickDelegate OnClick;
		public event GuiHelpers.OnDoubleClickDelegate OnDoubleClick;

        public FirewallNotificationWindow (IntPtr handle) : base (handle)
        {
            Initialize ();
        }

        [Export ("initWithCoder:")]
        public FirewallNotificationWindow (NSCoder coder) : base (coder)
        {
            Initialize ();
        }

        private void Initialize()
        {
            // possibility to drag by window body (we have no title bar)
            this.MovableByWindowBackground = true;
            // topmost window
            this.Level = NSWindowLevel.Floating;

            // clicks detection/notification
            _clickDetector.OnClick += () => 
            {
                OnClick?.Invoke ();
            };
            _clickDetector.OnDoubleClick += () => 
            {
                OnDoubleClick?.Invoke ();
            };
        }

        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();
        }

        public override void MouseUp (NSEvent theEvent)
        {
            base.MouseUp (theEvent);
            _clickDetector.MouseUp ();
        }

        public override void MouseDown (NSEvent theEvent)
        {
            base.MouseDown (theEvent);
            _clickDetector.MouseDown ();
        }
    }
}
