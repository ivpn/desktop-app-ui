using IVPN.Interfaces;
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
