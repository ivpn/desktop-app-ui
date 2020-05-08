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
using System.Linq;
using Foundation;

namespace IVPN
{
    /// <summary>
    /// Usage example:
    ///     GuiTextBlock.Formatter = new NSNumberFormatter();
    /// </summary>
    public class NumberFormatterForTextField : NSNumberFormatter
    {
        private int __MaxDigitsCount = 0;
        private int? __MaxValue = 0;

        public NumberFormatterForTextField(int maxDigitsCount = 0, int? maxValue = null)
        {
            __MaxDigitsCount = maxDigitsCount;
            __MaxValue = maxValue;
        }

        public override bool IsPartialStringValid(string partialString, out string newString, out NSString error)
        {
            newString = partialString;
            error = new NSString("");
            if (partialString.Length == 0)
                return true;

            bool isOK = true;
            if (__MaxDigitsCount > 0 && partialString.Length > __MaxDigitsCount)
            {
                partialString = partialString.Remove(0, partialString.Length - __MaxDigitsCount);
                newString = partialString;
                isOK = false;
            }

            // you could allow use partialString.All(c => c >= '0' && c <= '9') if internationalization is not a concern
            if (!partialString.All(char.IsDigit))
            {
                newString = new string(partialString.Where(char.IsDigit).ToArray());
                return false;
            }

            if (__MaxValue!=null)
            {
                int value;
                bool isValueChanged = false;
                try
                {
                    value = int.Parse(newString);
                    if (value > (int)__MaxValue)
                    {
                        value = (int)__MaxValue;
                        isValueChanged = true;
                    }
                }
                catch
                {
                    value = 0;
                    isValueChanged = true;
                }

                if (isValueChanged)
                {
                    newString = $"{value}";
                    return false;
                }
            }

            return isOK;
        }
    }
}
