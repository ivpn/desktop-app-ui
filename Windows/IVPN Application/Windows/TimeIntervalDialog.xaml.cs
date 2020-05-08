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

﻿using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace IVPN.Windows
{
    /// <summary>
    /// Interaction logic for TimeIntervalDialog.xaml
    /// </summary>
    public partial class TimeIntervalDialog : Window
    {
        public static bool ShowInputTimeIntervalDialog(out double retIntervalSeconds, Window owner = null, string descriptionText = null)
        {
            retIntervalSeconds = 0;

            var dlg = new TimeIntervalDialog();

            if (string.IsNullOrEmpty(descriptionText) == false)
                dlg.GuiTextBlockTextDescription.Text = descriptionText;

            if (owner != null)
                dlg.Owner = owner;

            if (dlg.ShowDialog() == true)
            {
                retIntervalSeconds = dlg.__ResultInSeconds;
                return true;
            }

            return false;
        }

        private double __ResultInSeconds;

        private TimeIntervalDialog()
        {
            InitializeComponent();
            GuiTextBoxHours.Focus();
        }

        private void GuiButton_OnOkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                __ResultInSeconds = double.Parse(GuiTextBoxHours.Text) * 60 * 60;
                __ResultInSeconds += double.Parse(GuiTextBoxMinutes.Text) * 60;
            }
            catch
            {
                MessageBox.Show(this, "Please, enter correct time interval", "Wrong data", MessageBoxButton.OK);
                return;
            }

            DialogResult = true;
            Close();
        }
        
        private void GuiTextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static readonly Regex RegexNumbers = new Regex("[^0-9]+$"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !RegexNumbers.IsMatch(text);
        }
    }
}
