using System;
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
