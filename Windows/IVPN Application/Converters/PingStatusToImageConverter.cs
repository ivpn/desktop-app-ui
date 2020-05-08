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
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace IVPN.Converters
{
    class PingStatusToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is double))
                return null;
            double pingTimeRelative = (double)value;

            string imageResourceUri;

            if (pingTimeRelative < 0.6)
                imageResourceUri = "pack://application:,,,/IVPN Client;component/Resources/iconStatusGood.png";
            else if (pingTimeRelative <= 0.9)
                imageResourceUri = "pack://application:,,,/IVPN Client;component/Resources/iconStatusModerate.png";
            else
                imageResourceUri = "pack://application:,,,/IVPN Client;component/Resources/iconStatusBad.png";
            try
            {
                return new BitmapImage(new Uri(imageResourceUri));
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
