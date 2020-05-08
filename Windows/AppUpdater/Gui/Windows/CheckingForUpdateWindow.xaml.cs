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

﻿using System.Windows;
using System.Windows.Media;

namespace AppUpdater.Gui
{
    /// <summary>
    /// Interaction logic for CheckingForUpdateWindow.xaml
    /// </summary>
    internal partial class CheckingForUpdateWindow : Window
    {
        private static CheckingForUpdateWindow _currWindow;
        public static void ShowWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                HideWindow();
                
                _currWindow = new CheckingForUpdateWindow();
                _currWindow.Owner = Application.Current.MainWindow;
                _currWindow.Show();
            });
        }

        public static void HideWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_currWindow != null)
                    _currWindow.DoClose();
                _currWindow = null;
            });
        }

        private bool _isClosed;

        private CheckingForUpdateWindow()
        {
            InitializeComponent();

            if (GuiController.AppIcon != null)
            {
                ImageSource imSource = GuiController.ToImageSource(GuiController.AppIcon);
                Icon = imSource;
            }
        }
        
        private void DoClose()
        {
            _isClosed = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_isClosed == false) // if user closed window
                Updater.Cancel();
        }
    }
}
