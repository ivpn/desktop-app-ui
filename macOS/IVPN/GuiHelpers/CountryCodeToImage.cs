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
using System.IO;
using AppKit;
using Foundation;

namespace IVPN.GuiHelpers
{
    public class CountryCodeToImage
    {
        public static NSImage GetCountryFlag(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
                return null;

            string ccode = countryCode.ToLower();

            // GB is correct country code, but 'designed' image for GB has name UK in project assets 
            // TODO: rename image name and remove this 
            if (ccode.Equals("gb"))
                ccode = "uk";

            return NSImage.ImageNamed(ccode);
        }

        public static NSImage GetCountryFlagFromAllCountriesSet(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
                return null;

            string ccode = countryCode.ToLower();

            // Reinsurance in case if Great Britain country code will be changed to UK
            if (ccode.Equals("uk"))
                ccode = "gb";

            string imagePath = @"flags/48/" + countryCode + ".png";
            NSImage ret = null;
            try
            {
                ret = GetBundleImage(imagePath);
            }
            catch (Exception ex)
            {
                Logging.Info($"Error: failed to load image '{imagePath}' from resources. ({ex.Message})");
                return null;
            }
            return ret;

        }

        public static NSImage GetBundleImage(string relativePath) 
        { 
            var path = Path.Combine(NSBundle.MainBundle.BundlePath, "Contents", "Resources", relativePath); 
            return new NSImage(path); 
        }
    }
}
