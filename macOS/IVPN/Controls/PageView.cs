using System;

using CoreGraphics;

using Foundation;
using AppKit;
using CoreAnimation;

namespace IVPN
{
    [Register("PageView")]
    public class PageView: NSView
    {
        private bool __WasDrawn;
        private ILayerDrawer __drawer;

        [Export("initWithCoder:")]
        public PageView(NSCoder coder) : base(coder)
        {

        }

        public PageView(IntPtr handle):base(handle)
        {
            
        }

        public override void DrawRect(CGRect dirtyRect)
        {
            base.DrawRect(dirtyRect);

            try 
            {
                ILayerDrawer drawer = __drawer;
                if (drawer != null)
                    __drawer.DrawLayer (this, dirtyRect);
            }
            catch {}

            __WasDrawn = true;
        }

        public void SetDrawer(ILayerDrawer drawer)
        {
            if (!this.WantsLayer) 
            {
                this.WantsLayer = true;
                this.LayerContentsRedrawPolicy = NSViewLayerContentsRedrawPolicy.OnSetNeedsDisplay;
            }

            __drawer = drawer;
            NeedsDisplay = true;
        }

        public bool WasDrawn
        {
            get { return __WasDrawn; }
            set { __WasDrawn = value;}
        }
    }

    public interface ILayerDrawer
    {
        void DrawLayer (NSView view, CGRect dirtyRect);    
    }
}

