using AppKit;
using Foundation;

namespace IVPN
{
    /// <summary>
    /// Menu delegate invert highlited item.
    /// 
    /// If menu item text is not contract - it can be not visible when highlited (default highlight color is blue)
    /// This class inverts highlited menu item text to white color (to make it visible)
    /// 
    /// https://stackoverflow.com/questions/20722374/set-font-color-of-nsmenuitem-to-alternate-when-highlighted
    /// </summary>
    public class MenuDelegateInvertHighlitedItem : NSMenuDelegate
    {
        public NSColor HighlitedTextColor { get; set; } = NSColor.White;
        const string OriginalColorKey = @"the_original_color";

        public override void MenuWillHighlightItem(NSMenu menu, NSMenuItem item)
        {
            RestoreTextColor(menu.HighlightedItem);
            OverrideTextColor(item);
        }

        public override void MenuDidClose(NSMenu menu)
        {
            RestoreTextColor(menu.HighlightedItem);
        }

        private void RestoreTextColor(NSMenuItem item)
        {
            if (item == null)
                return;

            NSMutableAttributedString title = new NSMutableAttributedString(item.AttributedTitle);
            RenameAttribute(title, OriginalColorKey, NSStringAttributeKey.ForegroundColor);
            item.AttributedTitle = title;
        }

        private void OverrideTextColor(NSMenuItem item)
        {
            if (item == null)
                return;

            NSMutableAttributedString title = new NSMutableAttributedString(item.AttributedTitle);
            RenameAttribute(title, NSStringAttributeKey.ForegroundColor, OriginalColorKey);

            title.AddAttribute(NSStringAttributeKey.ForegroundColor, HighlitedTextColor, new NSRange(0, title.Length));

            item.AttributedTitle = title;
        }

        private void RenameAttribute(NSMutableAttributedString text, string fromAttr, string toAttr)
        {
            NSRange fullRange = new NSRange(0, text.Length);

            NSString nsFromAttr = new NSString(fromAttr);
            NSString nsToAttr = new NSString(toAttr);

            text.RemoveAttribute(toAttr, fullRange);

            text.EnumerateAttribute(nsFromAttr,
                                    fullRange,
                                    NSAttributedStringEnumeration.None,
                                    (NSObject value, NSRange range, ref bool stop) =>
                                    {
                                        if (value == null)
                                            return;

                                        text.AddAttribute(nsToAttr, value, range);
                                    }
                                    );
        }
    }

}
