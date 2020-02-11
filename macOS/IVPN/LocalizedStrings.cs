using Foundation;
using IVPN.Interfaces;

namespace IVPN
{
    public class LocalizedStrings : ILocalizedStrings
    {
        static private LocalizedStrings __Instance;
        NSBundle __LanguageBundle;

        static public LocalizedStrings Instance
        {
            get
            {
                if (__Instance == null)
                    __Instance = new LocalizedStrings();

                return __Instance;
            }
        }

        private LocalizedStrings()
        {
            var path = NSBundle.MainBundle.PathForResource("en", "lproj");
            __LanguageBundle = NSBundle.FromPath(path);
        }

        public string LocalizedString(string key, string defaultText = null)
        {
            return __LanguageBundle.GetLocalizedString(key, defaultText);
        }
    }
}
