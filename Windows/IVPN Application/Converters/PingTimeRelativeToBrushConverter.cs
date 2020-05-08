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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace IVPN.Converters
{
    public class PingTimeRelativeToBrushConverter :IValueConverter
    {
        private Brush __GreenBrush;
        private Brush __RedBrush;
        private Brush __YellowBrush;

        public PingTimeRelativeToBrushConverter()
        {            
            __GreenBrush = new SolidColorBrush(Color.FromRgb(0x4A, 0xD1, 0x00));
            __YellowBrush = new SolidColorBrush(Color.FromRgb(0xCD, 0xC1, 0x00));
            __RedBrush = new SolidColorBrush(Color.FromRgb(0xE2, 0x40, 0x00));
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double pingTimeRelative = (double)value;

            if (pingTimeRelative < 0)
                pingTimeRelative = 0;

            if (pingTimeRelative > 1)
                pingTimeRelative = 1;

            double red_component = pingTimeRelative;
            double green_component = 1 - pingTimeRelative;

            var color = Color.FromRgb((byte)(0x20 * green_component + 0xE2 * red_component),
                                      (byte)(0xD1 * green_component + 0x40 * red_component),
                                       00);

            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
