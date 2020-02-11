namespace IVPN.Interfaces
{
    public interface ILocalizedStrings
    {
        /// <summary>
        /// Get localized string by KEY
        /// </summary>
        /// <param name="key">Key to search localized string</param>
        /// <param name="defaultText">The value to return if the key was not found.This parameter can be null.</param>
        /// <returns>A localized version of the string </returns>
        string LocalizedString(string key, string defaultText = null);
    }
}
