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

﻿using Foundation;
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
