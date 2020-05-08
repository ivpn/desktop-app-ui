//
//  IVPN Client Desktop
//  https://github.com/ivpn/desktop-app-ui
//
//  Created by Stelnykovych Alexandr.
//  Copyright (c) 2020 Privatus Limited.
//
//  This file is part of the IVPN Client Desktop.
//
//  The IVPN Client Desktop is free software: you can redistribute it and/or
//  modify it under the terms of the GNU General Public License as published by the Free
//  Software Foundation, either version 3 of the License, or (at your option) any later version.
//
//  The IVPN Client Desktop is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
//  details.
//
//  You should have received a copy of the GNU General Public License
//  along with the IVPN Client Desktop. If not, see <https://www.gnu.org/licenses/>.
//

﻿using AppKit;
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
