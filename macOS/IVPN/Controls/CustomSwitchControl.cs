using System;
using AppKit;
using CoreGraphics;
using Foundation;

namespace IVPN
{
    /// <summary>
    /// Styled switch-control
    /// </summary>
    [Register("CustomSwitchControl")]
    public class CustomSwitchControl : NSControl
    {
        #region Private Variables
        private bool _value = false;
        #endregion

        #region Computed Properties
        /// <summary>
        /// Gets or sets a value indicating whether On or Off.
        /// </summary>
        /// <value><c>true</c> if value; otherwise, <c>false</c>.</value>
        [Export ("Value")]
        public bool Value
        {
            get { return _value; }
            set
            {
                // Save value and force a redraw
                _value = value;
                NeedsDisplay = true;
            }
        }

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public CustomSwitchControl()
        {
            // Init
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="handle">Handle.</param>
        public CustomSwitchControl(IntPtr handle) : base(handle)
        {
            // Init
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="frameRect">Frame rect.</param>
        [Export("initWithFrame:")]
        public CustomSwitchControl(CGRect frameRect) : base(frameRect)
        {
            // Init
            Initialize();
        }

        /// <summary>
        /// Initialize this instance.
        /// </summary>
        private void Initialize()
        {
            this.WantsLayer = true;
            this.LayerContentsRedrawPolicy = NSViewLayerContentsRedrawPolicy.OnSetNeedsDisplay;            
        }

        DateTime __MouseDownTime = DateTime.MinValue;
        public override void MouseDown(NSEvent theEvent)
        {
            base.MouseDown(theEvent);
            __MouseDownTime = DateTime.Now;
        }
        public override void MouseUp(NSEvent theEvent)
        {
            base.MouseUp(theEvent);

            if ((DateTime.Now - __MouseDownTime).TotalMilliseconds <= 500)
            {
                if (Enabled)
                    FlipSwitchState();
            }
        }

        #endregion

        #region Draw Methods
        /// <summary>
        /// Draws the User Interface for this custom control
        /// </summary>
        /// <param name="dirtyRect">Dirty rect.</param>
        public override void DrawRect(CGRect dirtyRect)
        {
            base.DrawRect(dirtyRect);

            CustomSwitchControlDrawer.DrawUISwitch(dirtyRect, Enabled, Value,
            BorderWidth,
            SwitchOnBackgroundColor,
            Colors.SwitcherOffBackgroundColor,
            Colors.SwitcherOffBorderColor,
            Colors.SwitcherCircleColor,
            Colors.SwitcherCircleShadowColor);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Flips the state of the switch between On and Off
        /// </summary>
        private void FlipSwitchState()
        {
            // Update state
            Value = !Value;
            RaiseValueChanged();
        }
        #endregion


        #region Events
        /// <summary>
        /// Occurs when value of the switch is changed.
        /// </summary>
        public event EventHandler ValueChanged;

        /// <summary>
        /// Raises the value changed event.
        /// </summary>
        internal void RaiseValueChanged()
        {
            if (this.ValueChanged != null)
                this.ValueChanged(this, EventArgs.Empty);

            // Perform any action bound to the control from Interface Builder via an Action.
            if (this.Action != null)
                NSApplication.SharedApplication.SendAction(this.Action, this.Target, this);
        }
        #endregion

        public int BorderWidth { get; set; } = 1;
        public NSColor SwitchOnBackgroundColor { get; set; } = NSColor.FromRgb(33, 208, 116);
    }

    internal static class CustomSwitchControlDrawer
    {
        public static void DrawUISwitch(CGRect dirtyRect, bool isEnabled, bool isSwitchOn,
            int BorderWidth,
            NSColor SwitchOnBackgroundColor,
            NSColor SwitchOffBackgroundColor,
            NSColor SwitchOffBorderColor,
            NSColor InternalSwitcherColor,
            NSColor InternalSwitcherShadowColor
            )
        {
            NSGraphicsContext context = NSGraphicsContext.CurrentContext;
            context.CGContext.SaveState();


            NSColor switchOnBackgroundColor = SwitchOnBackgroundColor;
            NSColor switchOffBackgroundColor = SwitchOffBackgroundColor;
            NSColor switchOffBorderColor = SwitchOffBorderColor;
            NSColor internalSwitcherColor = InternalSwitcherColor;
            NSColor internalSwitcherShadowColor = InternalSwitcherShadowColor;
            if (isEnabled == false) 
            {
                // if disabled - set background color darker
                switchOnBackgroundColor = SetColorDarker (switchOnBackgroundColor);
                switchOffBackgroundColor = SetColorDarker (switchOffBackgroundColor);
                switchOffBorderColor = SetColorDarker (switchOffBorderColor);
                internalSwitcherColor = SetColorDarker (internalSwitcherColor);
                internalSwitcherShadowColor = SetColorDarker (internalSwitcherShadowColor);
            }

            nfloat offset = dirtyRect.Height * 0.1f;
            dirtyRect = new CGRect(dirtyRect.X + offset, 
                                   dirtyRect.Y + offset,  
                                   dirtyRect.Width - offset*2,  
                                   dirtyRect.Height - offset*2);

            // set backgrund color
            NSColor bodyColor = (isSwitchOn)? switchOnBackgroundColor : switchOffBackgroundColor;
            bodyColor.SetFill();

            // draw background
            NSBezierPath bodyPath = NSBezierPath.FromRoundedRect(dirtyRect, dirtyRect.Height/2, dirtyRect.Height / 2);
            bodyPath.Fill();

            // draw border
            if (!isSwitchOn)
            {
                bodyPath.AddClip();
                bodyPath.LineWidth = BorderWidth;

                if (switchOffBorderColor != null)
                    switchOffBorderColor.SetStroke();
                bodyPath.Stroke();
            }

            //restore \ save context status
            context.CGContext.RestoreState();
            context.CGContext.SaveState();

            // DRAW CIRCLE
            CGRect circleRect;

            if (!isSwitchOn)
                circleRect = new CGRect(dirtyRect.X, 
                                        dirtyRect.Y, 
                                        dirtyRect.Height, 
                                        dirtyRect.Height);
            else
                circleRect = new CGRect((dirtyRect.Width - dirtyRect.Height + dirtyRect.X), 
                                        dirtyRect.Y,
                                        dirtyRect.Height, 
                                        dirtyRect.Height);

            // draw circle with shadow (no shadow for dark mode)
            if (!Colors.IsDarkMode)
            { 
                CGRect circleShadowRect = new CGRect(circleRect.X + offset / 3, circleRect.Y + offset / 3, circleRect.Height - 2 * offset / 3, circleRect.Height - 2 * offset / 3);

                NSBezierPath circleShadowPath = NSBezierPath.FromRoundedRect(circleShadowRect, circleShadowRect.Height / 2, circleShadowRect.Height / 2);
                context.CGContext.SetShadow (new CGSize (offset / 3, -offset),
                                            offset * 1f,
                                             new CGColor(internalSwitcherShadowColor.RedComponent, 
                                                         internalSwitcherShadowColor.GreenComponent, 
                                                         internalSwitcherShadowColor.BlueComponent));
                circleShadowPath.Fill();
            }

            // restore context state
            context.CGContext.RestoreState();

            // set circle color
            NSColor circleColor = internalSwitcherColor;
            circleColor.SetFill();

            // draw circle without shadow to fill internal area filled by shadow
            NSBezierPath circlePath = NSBezierPath.FromRoundedRect(circleRect, circleRect.Height / 2, circleRect.Height / 2);
            circlePath.Fill();
              
            // circle border
            circlePath.AddClip();
            circlePath.LineWidth = 1;

            if (Colors.IsDarkMode)
            {
                // no border for Dark mode
                internalSwitcherColor.SetStroke();
            }
            else
            {
                if (switchOffBorderColor != null)
                    switchOffBorderColor.SetStroke();
            }
            circlePath.Stroke();
            
        }

        public static NSColor SetColorDarker (NSColor color)
        {
            if (color == null)
                return null;

            if (Colors.IsDarkMode)
                return NSColor.FromRgb(color.RedComponent * 0.8f, color.GreenComponent * 0.8f, color.BlueComponent * 0.8f);
            return NSColor.FromRgb (color.RedComponent * 0.9f, color.GreenComponent * 0.9f, color.BlueComponent * 0.9f);
        }
    }
}
