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

﻿using IVPN.Interfaces;
using System;
using System.Windows;

namespace IVPN 
{
    class StringUtils : ILocalizedStrings
    {
        /// <summary>
        /// Get localized string by KEY
        /// </summary>
        /// <param name="key">Key to search localized string</param>
        /// <param name="defaultText">The value to return if the key was not found.This parameter can be null.</param>
        /// <returns>A localized version of the string </returns>
        public static string String(string key, string defaultText = null)
        {
            string retText = Application.Current.TryFindResource(key) as string;
            if (retText == null)
            {
                if (defaultText == null)
                    throw new ResourceReferenceKeyNotFoundException($"'{key}' resource not found.", key) ;

                retText = defaultText;
            }

            return retText.Replace(@"\n", Environment.NewLine);
        }

        public string LocalizedString(string key, string defaultText = null)
        {
            return String(key, defaultText);
        }
    }
}
