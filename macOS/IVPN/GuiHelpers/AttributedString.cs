using AppKit;
using Foundation;

namespace IVPN.GuiHelpers
{
    public class AttributedString
    {
        /// <summary>
        /// Create attribytes string with a required style
        /// </summary>
        public static NSAttributedString Create (string text, NSColor color = null, NSTextAlignment? aligment = null, NSFont font = null)
        {
            if (text == null)
                text = "";

            NSMutableParagraphStyle paragraphStyle = null;

            NSStringAttributes stringAttributes = new NSStringAttributes ();

            if (color!=null)
                stringAttributes.ForegroundColor = color;

            if (font != null)
                stringAttributes.Font = font;

            if (aligment != null) 
            {
                if (paragraphStyle == null)
                    paragraphStyle = new NSMutableParagraphStyle ();
                paragraphStyle.Alignment = (NSTextAlignment)aligment;
            }

            if (paragraphStyle != null)
                stringAttributes.ParagraphStyle = paragraphStyle;
            
            return new NSAttributedString (text, stringAttributes);
        }

    }
}
