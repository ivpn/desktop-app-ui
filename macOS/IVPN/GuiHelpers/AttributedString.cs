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
