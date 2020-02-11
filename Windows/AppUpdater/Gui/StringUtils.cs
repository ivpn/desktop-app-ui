using System;
using System.Windows;

namespace AppUpdater.Gui
{
    internal class StringUtils
    {
        private static ResourceDictionary _resourceDictionary;

        private static ResourceDictionary GetResourceDictionary()
        {
            if (_resourceDictionary != null)
                return _resourceDictionary;

            _resourceDictionary = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/AppUpdater;component/Gui/Strings.xaml", UriKind.Absolute)
            };

            return _resourceDictionary;
        }

        public static string String(string key)
        {
            ResourceDictionary resDic = GetResourceDictionary();
            if (!resDic.Contains(key))
                return key; // TODO: what we should return here?
            
            string ret = resDic[key] as string;
            if (ret == null)
                return key; // TODO: what we should return here?
            return ret.Replace(@"\n", Environment.NewLine);
        }

        public static string String(string key, params object[] objs)
        {
            ResourceDictionary resDic = GetResourceDictionary();
            if (!resDic.Contains(key))
                return key; // TODO: what we should return here?

            string ret = resDic[key] as string;
            if (ret == null)
                return key; // TODO: what we should return here?

            return System.String.Format(ret, objs).Replace(@"\n", Environment.NewLine);
        }
    }
}
