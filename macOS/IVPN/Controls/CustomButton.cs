using System;
using AppKit;
using CoreGraphics;
using Foundation;

namespace IVPN
{
    /// <summary>
    /// Static class - helper.
    /// Customize CustomButton to required button style.
    /// </summary>
    public class CustomButtonStyles
    {
        private static void SetBaseStyle(CustomButton button, string title = null, NSImage image = null)
        {
            if (button == null)
                return;

            if (title != null)
                button.TitleText = ((image != null) ? " " : "") + title;
            
            if (image != null)
                button.Image = image;
        }

        /// <summary>
        /// Applies the style secondary button.
        /// </summary>
        public static void ApplyStyleSecondaryButton (CustomButton button, string title = null, NSImage image = null)
        {
            if (button == null)
                return;
            
            SetBaseStyle (button, title, image);

            button.Gradient = new NSGradient(Colors.SecondaryBtnGradient1Color, Colors.SecondaryBtnGradient2Color);
            button.TitleForegroundColor = Colors.SecondaryBtnTextColor;
            button.BorderColor = Colors.SecondaryBtnBorderColor;
            button.BorderShadow = new NSShadow
            {
                ShadowOffset = new CGSize(0f, 1f),
                ShadowColor = NSColor.FromRgba(0, 0, 0, 0.12f)
            };
            button.HighlitedColorOverlay = NSColor.FromRgba(0, 0, 0, 0.05f);
        }

        /// <summary>
        /// Applies the style main button.
        /// </summary>
        public static void ApplyStyleMainButton (CustomButton button, string title, NSImage image = null)
        {
            if (button == null)
                return;
            
            SetBaseStyle (button, title, image);

            if (title != null)
                button.TitleText = title;

            button.BorderShadow = new NSShadow 
            {
                ShadowOffset = new CGSize (0f, 1f),
                ShadowColor = NSColor.FromRgba (0, 0, 0, 0.18f)
            };
        }

        /// <summary>
        /// Applies the style secondary button.
        /// </summary>
        public static void ApplyStyleSecondaryButtonV2 (CustomButton button, string title = null, NSImage image = null)
        {
            if (button == null)
                return;

            SetBaseStyle (button, title, image);

            button.CornerRadius = 3.5f;
            button.BackgroundColor = NSColor.FromRgb (255, 255, 255);
            button.TitleForegroundColor = NSColor.FromRgb (39, 39, 39);
            button.BorderColor = NSColor.FromRgb (231, 231, 231);

            button.BorderShadow = new NSShadow 
            {
                ShadowOffset = new CGSize (0f, 1f),
                ShadowColor = NSColor.FromRgba (0, 0, 0, 0.12f)
            };
            button.HighlitedColorOverlay = NSColor.FromRgba (0, 0, 0, 0.05f);
        }

        /// <summary>
        /// Applies the style main button.
        /// </summary>
        public static void ApplyStyleMainButtonV2 (CustomButton button, string title, NSImage image = null)
        {
            if (button == null)
                return;

            SetBaseStyle (button, title, image);

            if (title != null)
                button.TitleText = title;

            button.Gradient = new NSGradient (NSColor.FromRgb (128, 187, 249), NSColor.FromRgb (17, 130, 254));

            button.BorderShadow = new NSShadow {
                ShadowOffset = new CGSize (0f, 1f),
                ShadowColor = NSColor.FromRgba (0, 0, 0, 0.18f)
            };
        }

        /// <summary>
        /// Applies the style navigation button.
        /// </summary>
        public static void ApplyStyleNavigationButton(CustomButton button, string title, NSImage image = null)
        {
            if (button == null)
                return;
            
            SetBaseStyle (button, title, image);

            button.TitleForegroundColor = NSColor.FromRgb (122, 138, 153);
            button.CornerRadius = 0;
            button.BorderColor = NSColor.FromRgb (122, 138, 153);
            button.Gradient = null;
        }

        /// <summary>
        /// Applies the style navigation button.
        /// </summary>
        public static void ApplyStyleNavigationButtonV2(CustomButton button, string title, NSImage image = null)
        {
            if (button == null)
                return;

            SetBaseStyle(button, title, image);

            button.TitleForegroundColor = NSColor.FromRgb(152, 170, 186);
            button.CornerRadius = 0;
            button.BorderColor = NSColor.FromRgb(122, 138, 153);
            button.Gradient = null;
    
            button.TitleFont = UIUtils.GetSystemFontOfSize(14f, NSFontWeight.Regular);//   Thin);

            button.DoNotChangeColorWhenDisabled = true;
        }

        /// <summary>
        /// Applies the style title navigation button.
        /// </summary>
        public static void ApplyStyleTitleNavigationButton (CustomButton button, string title = null, NSImage image = null)
        {
            if (button == null)
                return;

            SetBaseStyle (button, title, image);

            if (title != null)
                button.TitleText = title;

            button.CornerRadius = 4;
            button.TitleForegroundColor = Colors.HeaderNavigationBtnTextColor;
            button.TitleFont = UIUtils.GetSystemFontOfSize(12f, NSFontWeight.Medium);

            button.BackgroundColor = Colors.HeaderNavigationBtnColor;
            button.BorderColor = Colors.HeaderNavigationBtnBorderColor;
            button.IconLocation = CustomButton.IconLocationEnum.Left_BeforeCenteredText;
        }

        public static void ApplyStyleTitleConfigureButton(CustomButton button, string title = null, NSImage image = null)
        {
            if (button == null)
                return;

            SetBaseStyle(button, title, image);

            if (title != null)
                button.TitleText = title;

            button.CornerRadius = 4;

            button.TitleForegroundColor = NSColor.FromRgb(109, 109, 109);
            button.TitleFont = UIUtils.GetSystemFontOfSize(12f, NSFontWeight.Thin);

            button.Gradient = new NSGradient(NSColor.FromRgb(227, 229, 231), NSColor.FromRgb(227, 229, 231));
            button.BorderColor = NSColor.FromRgb(217, 217, 217);
            button.IconLocation = CustomButton.IconLocationEnum.Left_BeforeCenteredText;
        }

        public static void ApplyStyleTitleConfigureButtonPressed(CustomButton button, string title = null, NSImage image = null)
        {
            if (button == null)
                return;

            SetBaseStyle(button, title, image);

            if (title != null)
                button.TitleText = title;

            button.CornerRadius = 4;

            button.TitleForegroundColor = NSColor.FromRgb(255, 255, 255);
            button.TitleFont = UIUtils.GetSystemFontOfSize(12f, NSFontWeight.Thin);

            button.Gradient = new NSGradient(NSColor.FromRgb(197, 208, 217), NSColor.FromRgb(197, 208, 217));
            button.BorderColor = NSColor.FromRgb(217, 217, 217);
            button.IconLocation = CustomButton.IconLocationEnum.Left_BeforeCenteredText;
        }

        /// <summary>
        /// Applies the style info button.
        /// </summary>
        public static void ApplyStyleInfoButton (CustomButton button, string title = null, NSImage image = null)
        {
            if (button == null)
                return;

            SetBaseStyle (button, title, image);

            if (title != null)
                button.TitleText = title;

            button.TitleForegroundColor = Colors.BtnAccountExpireFont;
            button.TitleFont = UIUtils.GetSystemFontOfSize (12f, NSFontWeight.Regular);

            button.CornerRadius = 12;

            button.BackgroundColor = Colors.BtnAccountExpireBackground;
            button.BorderColor = Colors.BtnAccountExpireBorder;

            button.BorderShadow = new NSShadow 
            {
                ShadowOffset = new CGSize (0, 1),
                ShadowColor = NSColor.FromRgba (0, 0, 0, 0.25f)
            };

            button.IconLocation = CustomButton.IconLocationEnum.Left_BeforeCenteredText;
            button.HighlitedColorOverlay = NSColor.FromRgba (0, 0, 0, 0.05f);
        }

        /// <summary>
        /// Applies the style grey button.
        /// </summary>
        public static void ApplyStyleGreyButton(CustomButton btn, string title)
        {
            NSColor ButtonBorderColor = NSColor.FromRgb(223, 223, 235);
            NSColor ButtonColor = NSColor.FromRgb(250, 252, 255);
            NSColor ButtonTextColor = NSColor.FromRgb(38, 57, 77);

            btn.BorderLineWidth = 1f;
            btn.BorderColor = ButtonBorderColor;
            btn.TitleFont = UIUtils.GetSystemFontOfSize(14f, NSFontWeight.Semibold);
            btn.TitleText = title;
            btn.BackgroundColor = ButtonColor;
            btn.TitleForegroundColor = ButtonTextColor;
        }

        public static void ApplyStyleGreyButtonV2(CustomButton btn, string title)
        {
            ApplyStyleGreyButton(btn, title);
            btn.BorderColor = NSColor.LightGray;
        }
    }

    /// <summary>
    /// Custom button.
    /// - rounded corners
    /// - gradient fill
    /// - shadow
    /// - title font / color
    /// - border color / width
    /// </summary>
    [Register ("CustomButton")]
    public class CustomButton : NSButton
    {
        #region Constructors
        public CustomButton ()
        {
            // Init
            Initialize ();
        }

        public CustomButton (IntPtr handle) : base (handle)
        {
            // Init
            Initialize ();
        }

        [Export ("initWithFrame:")]
        public CustomButton (CGRect frameRect) : base (frameRect)
        {
            // Init
            Initialize ();
        }
        #endregion
        private void Initialize ()
        {
            // initialize default values
            CornerRadius = 4;

            GradientAngle = 90;
            ImagePosition = NSCellImagePosition.ImageLeft;

            TitleText = "Button";
            TitleForegroundColor = NSColor.FromRgb (255, 255, 255);
            TitleFont = UIUtils.GetSystemFontOfSize (16f, NSFontWeight.Semibold);

            Gradient = new NSGradient (NSColor.FromRgb (58, 156, 217), NSColor.FromRgb (20, 130, 222));

            BorderColor = NSColor.FromRgb (48, 147, 209);
            BorderLineWidth = 0.5f;

            IconLocation = IconLocationEnum.Right_BeforeCenteredText;
        }

        public override void DrawRect (CGRect dirtyRect)
        {
            if (Gradient != null)
                Gradient.DrawInRect (dirtyRect, GradientAngle);

            // -----------------------------------------
            nfloat textOffset = 0f;
            nfloat imageXOffset = 0f;
            nfloat imageWidth = 0f;
            if (Image != null) 
            {
                if (Image.Size.Height > dirtyRect.Height)
                    Image.Size = new CGSize (Image.Size.Height, dirtyRect.Height);
                if (Image.Size.Width > dirtyRect.Width)
                    Image.Size = new CGSize (dirtyRect.Width, Image.Size.Width);

                imageWidth = Image.Size.Width;

                nfloat minXOffset = (dirtyRect.Height - Image.Size.Height) / 2f;

                imageXOffset = CornerRadius;
                if (imageXOffset < minXOffset)
                    imageXOffset = minXOffset;

                if (AttributedTitle == null)
                    imageXOffset = dirtyRect.Width / 2f - Image.Size.Width / 2f;
                else if (IconLocation == IconLocationEnum.Right_BeforeCenteredText) 
                    imageXOffset = dirtyRect.Width / 2f - AttributedTitle.Size.Width / 2f - Image.Size.Width - minXOffset / 2f;
                else if (IconLocation == IconLocationEnum.Left_AfterCenteredText)
                    imageXOffset = dirtyRect.Width / 2f + AttributedTitle.Size.Width / 2f + minXOffset / 2f;
                else if (IconLocation == IconLocationEnum.Right_AfterCenteredText)
                    imageXOffset = dirtyRect.Width - minXOffset / 2f - Image.Size.Width;
                else if (IconLocation == IconLocationEnum.Right)
                {
                    nfloat space = (dirtyRect.Width - AttributedTitle.Size.Width - Image.Size.Width) / 3;
                    textOffset = space;
                    imageXOffset = space + AttributedTitle.Size.Width + space;
                }
                else if (IconLocation == IconLocationEnum.Left)
                {
                    nfloat space = (dirtyRect.Width - AttributedTitle.Size.Width - Image.Size.Width) / 3;
                    imageXOffset = space;
                    textOffset = space + Image.Size.Width + space;
                }
                
                CGRect imgRect = new CGRect (dirtyRect.X + imageXOffset,
                                             dirtyRect.Y + dirtyRect.Height / 2f - Image.Size.Height / 2f,
                                             Image.Size.Width,
                                             Image.Size.Height);
                Image.Draw (imgRect);
            }

            if (AttributedTitle != null) 
            {
                if (IconLocation == IconLocationEnum.Right_BeforeCenteredText || IconLocation == IconLocationEnum.Left_BeforeCenteredText)
                {
                    if ((imageXOffset + imageWidth) > (dirtyRect.Width - AttributedTitle.Size.Width) / 2f)
                        textOffset = imageXOffset + imageWidth;
                }
                    
                CGRect titleRect = new CGRect (dirtyRect.X + textOffset,
                                               dirtyRect.Y + dirtyRect.Height / 2f - AttributedTitle.Size.Height / 2f - 1f,
                                               dirtyRect.Width - textOffset,
                                               AttributedTitle.Size.Height);

                this.AttributedTitle.DrawInRect (titleRect);
            }

            if (Highlighted) 
            {
                NSColor fillColor = HighlitedColorOverlay;
                if (fillColor==null)
                    fillColor = NSColor.FromRgba (0, 0, 0, 0.1f);
                fillColor.Set ();

                NSBezierPath highlitedFill = new NSBezierPath ();
                highlitedFill.AppendPathWithRoundedRect (dirtyRect, CornerRadius, CornerRadius);
                highlitedFill.Fill ();
            }
            
            if (!this.Enabled && DoNotChangeColorWhenDisabled != true) 
            {
                NSColor fillColor = HighlitedColorOverlay;
                if (fillColor == null)
                    fillColor = NSColor.FromRgba (0, 0, 0, 0.1f);
                fillColor.Set ();

                NSBezierPath highlitedFill = new NSBezierPath ();
                highlitedFill.AppendPathWithRoundedRect (dirtyRect, CornerRadius, CornerRadius);
                highlitedFill.Fill ();
            }
            // -----------------------------------------
            //base.DrawRect (dirtyRect);

            if (!Highlighted && BorderShadow != null)
                BorderShadow.Set ();


            NSBezierPath bounds = new NSBezierPath ();
            bounds.AppendPathWithRoundedRect (dirtyRect, CornerRadius, CornerRadius);
            bounds.AddClip ();

            bounds.LineWidth = BorderLineWidth;

            if (BorderColor != null)
                BorderColor.SetStroke ();
            
            bounds.Stroke ();
        }

        private void SaveChanges ()
        {
            if (TitleTextAttributedString != null)
                AttributedTitle = TitleTextAttributedString;
            else
            {
                if (TitleText == null)
                    TitleText = "";

                AttributedTitle = new NSAttributedString (
                    TitleText,
                    new NSStringAttributes 
                    {
                        ForegroundColor = TitleForegroundColor,
                        Font = TitleFont,
                        ParagraphStyle = new NSMutableParagraphStyle 
                        { 
                            Alignment = NSTextAlignment.Center 
                        }
                    });
            }
            
            NeedsDisplay = true;
        }

        public enum IconLocationEnum
        {
            // Title must be have  NSTextAlignment.Center!
            Left_BeforeCenteredText,
            // Title must be have  NSTextAlignment.Center!
            Right_BeforeCenteredText,
            // Title must be have  NSTextAlignment.Center!
            Left_AfterCenteredText,
            // Title must be have  NSTextAlignment.Center!
            Right_AfterCenteredText,
            // Title must NOT be centered!
            Right,
            // Title must NOT be centered!
            Left
        }

        /// <summary>
        /// false - button does not chnage color when disable
        /// </summary>
        public bool DoNotChangeColorWhenDisabled
        {
            get { return __DoNotChangeColorWhenDisabled; }
            set { __DoNotChangeColorWhenDisabled = value; SaveChanges(); }
        }
        bool __DoNotChangeColorWhenDisabled;

        /// <summary>
        /// Gets or sets the icon location.
        /// </summary>
        public IconLocationEnum IconLocation
        {
            get { return __IconLocation; }
            set { __IconLocation = value; SaveChanges (); }
        }
        IconLocationEnum __IconLocation;

        /// <summary>
        /// Gets or sets the width of the border line.
        /// </summary>
        public nfloat BorderLineWidth
        {
            get { return __BorderLineWidth; }
            set { __BorderLineWidth = value;  SaveChanges ();}
        }
        private nfloat __BorderLineWidth;

        /// <summary>
        /// Gets or sets the color of the border.
        /// </summary>
        public NSColor BorderColor
        {
            get { return __BorderColor; }
            set { __BorderColor = value;  SaveChanges ();}
        }
        NSColor __BorderColor;

        public NSColor BackgroundColor 
        {
            set { Gradient = new NSGradient (value, value); }    
        }

        /// <summary>
        /// Gets or sets the border shadow.
        /// </summary>
        public NSShadow BorderShadow 
        {
            get { return __Shadow; }
            set { __Shadow = value; SaveChanges (); }
        }
        NSShadow __Shadow;

        public NSColor HighlitedColorOverlay
        {
            get { return __HighlitedColor; }
            set { __HighlitedColor = value; SaveChanges (); }
        }
        NSColor __HighlitedColor;

        /// <summary>
        /// Gets or sets the color of the title foreground.
        /// </summary>
        public NSColor TitleForegroundColor 
        { 
            get { return __TitleForegroundColor; }
            set { __TitleForegroundColor = value; SaveChanges (); } 
        }
        private NSColor __TitleForegroundColor;

        /// <summary>
        /// Gets or sets the title font.
        /// </summary>
        public NSFont TitleFont 
        { 
            get { return __TitleFont; } 
            set { __TitleFont = value; SaveChanges (); } 
        }
        private NSFont __TitleFont;

        /// <summary>
        /// Gets or sets the title text.
        /// </summary>
        public string TitleText 
        {
            get { return __TitleText; } 
            set 
            { 
                __TitleText = value; 
                __TitleTextAttributedString = null; 
                SaveChanges (); 
            }
        }
        private string __TitleText;

        public NSAttributedString TitleTextAttributedString
        {
            get { return __TitleTextAttributedString; } 
            set 
            { 
                __TitleTextAttributedString = value;
                __TitleText = null;
                SaveChanges (); 
            } 
        }
        private NSAttributedString __TitleTextAttributedString;

        /// <summary>
        /// Corner angle radius
        /// </summary>
        public nfloat CornerRadius
        {
            set 
            {
                Bordered = false;
                WantsLayer = true;
                Layer.CornerRadius = value;
                
                SaveChanges ();
            }
            get { return Layer.CornerRadius; }
        }

        /// <summary>
        /// Example:
        /// NSGradient gradient = new NSGradient (NSColor.FromRgb (57, 158, 230), NSColor.FromRgb (23, 143, 230));
        /// </summary>
        public NSGradient Gradient
        {
            get { return __Gradient;  }
            set 
            { 
                __Gradient = value;
                SaveChanges (); 
            }
        }
        private NSGradient __Gradient;

        /// <summary>
        /// Gets or sets the gradient angle.
        /// </summary>
        public nfloat GradientAngle
        {
            get { return __GradientAngle; }
            set { __GradientAngle = value; SaveChanges ();}
        }
        private nfloat __GradientAngle;
    }
}
