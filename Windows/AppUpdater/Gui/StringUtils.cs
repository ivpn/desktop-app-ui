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
