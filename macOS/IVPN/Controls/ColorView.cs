using System;

using Foundation;
using AppKit;

using CoreGraphics;

namespace IVPN
{

    [Register("ColorView")]
    public class ColorView: NSView
    {
        private CGColor __BackgroundColor;

        public ColorView(): base()
        {
            
        }

        public ColorView(IntPtr handle): base(handle)
        {
            BackgroundColor = new CGColor(1.0f, 1.0f, 1.0f);
        }

        [Export("initWithCoder:")]
        public ColorView(NSCoder coder) : base(coder)
        {
            
        }

        public override void DrawRect(CGRect dirtyRect)
        {
            CGContext context = NSGraphicsContext.CurrentContext.GraphicsPort;
            context.SetFillColor(__BackgroundColor);
            context.FillRect(dirtyRect);

            base.DrawRect(dirtyRect);

            if (BorderLineWidth > 0) 
            {
                NSBezierPath bounds = new NSBezierPath ();
                bounds.AppendPathWithRect (dirtyRect);
                bounds.AddClip ();

                bounds.LineWidth = BorderLineWidth;

                if (BorderColor != null)
                    BorderColor.SetStroke ();

                bounds.Stroke ();
            }
        }

        public CGColor BackgroundColor
        {
            get { return __BackgroundColor; }

            set 
            {
                __BackgroundColor = value;
                NeedsDisplay = true;
            }
        }

        /// <summary>
        /// Gets or sets the width of the border line.
        /// </summary>
        public nfloat BorderLineWidth 
        {
            get { return __BorderLineWidth; }
            set 
            { 
                __BorderLineWidth = value; 
                NeedsDisplay = true; 
            }
        }
        private nfloat __BorderLineWidth;

        /// <summary>
        /// Gets or sets the color of the border.
        /// </summary>
        public NSColor BorderColor 
        {
            get { return __BorderColor; }
            set 
            { 
                __BorderColor = value; 
                NeedsDisplay = true; 
            }
        }
        NSColor __BorderColor;
    }
}

