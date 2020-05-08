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

﻿using System;
using AppKit;
using CoreGraphics;
using Foundation;

namespace IVPN
{
    /// <summary>
    /// Custom secure text field cell.
    /// </summary>
    [Register ("CustomTextFieldCellSecure")]
    public class CustomTextFieldCellSecure : NSSecureTextFieldCell
    {
        #region Constructors
        public CustomTextFieldCellSecure ()
        {
            // Init
            Initialize ();
        }

        public CustomTextFieldCellSecure (IntPtr handle) : base (handle)
        {
            // Init
            Initialize ();
        }

        private void Initialize ()
        {
            Title = "";
            CellPaddingHorisontally = 15;
            CellPaddingVertically = 7;

            this.Font = UIUtils.GetSystemFontOfSize (14);
            this.UsesSingleLineMode = true;
        }
        #endregion

        public override CGRect DrawingRectForBounds (CGRect theRect)
        {
            if (CellPaddingLeft != 0 || CellPaddingRight != 0 || CellPaddingTop != 0 || CellPaddingButtom != 0) {
                var rect = new CGRect (
                    theRect.X + CellPaddingLeft,
                    theRect.Y + CellPaddingTop,
                    theRect.Width - CellPaddingRight,
                    theRect.Height - CellPaddingButtom);

                return base.DrawingRectForBounds (rect);
            }

            return base.DrawingRectForBounds (theRect);
        }

        public nfloat CellPaddingLeft { get; set; }
        public nfloat CellPaddingRight { get; set; }
        public nfloat CellPaddingTop { get; set; }
        public nfloat CellPaddingButtom { get; set; }

        public nfloat CellPaddingVertically {
            set {
                CellPaddingTop = value;
                CellPaddingButtom = value;
            }
        }

        public nfloat CellPaddingHorisontally {
            set {
                CellPaddingLeft = value;
                CellPaddingRight = value;
            }
        }
    }
}
