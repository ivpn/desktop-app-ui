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
using System.Windows.Media;

namespace AppUpdater.Gui
{
    /// <summary>
    /// Interaction logic for UpdateAvailableDialog.xaml
    /// </summary>
    internal partial class UpdateAvailableDialog 
    {
        private static UpdateAvailableDialog _currentWindow;

        public string ReleaseNotesLink { get; private set; }
        public string NewVersion { get; private set; }
        public string CompanyName { get { return GuiController.CompanyName; } }
        public string AppName { get { return GuiController.AppName; } }
        public string CurVersion { get { return GuiController.AppVersion; } }

        private bool _isSentCommandToUpdater;

        public static void ShowWindow(Appcast appcast)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _currentWindow = new UpdateAvailableDialog(appcast);
                _currentWindow.Owner = Application.Current.MainWindow;
                _currentWindow.Show();
            });
        }

        public static void HideWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_currentWindow!=null)
                    _currentWindow.Close();
                _currentWindow = null;
            });
        }

        private UpdateAvailableDialog(Appcast appcast)
        {
            ReleaseNotesLink = appcast.ReleaseNotesLink;
            NewVersion = appcast.Version;

            DataContext = this;
            InitializeComponent();

            if (GuiController.AppIcon != null)
            {
                ImageSource imSource = GuiController.ToImageSource(GuiController.AppIcon);
                Icon = imSource;
                GuiImage.Source = imSource;
            }

            if (!string.IsNullOrEmpty(ReleaseNotesLink))
            {
                try
                {
                    GuiWebBrowser.Source = new Uri(ReleaseNotesLink);
                }
                catch (Exception)
                {
                    // ignore errors
                }
            }
            else
            {
                GuiWebBrowser.Visibility = Visibility.Collapsed;
            }
        }

        private void GuiButtonSkipThisVersion_Click(object sender, RoutedEventArgs e)
        {
            _isSentCommandToUpdater = true;
            Updater.SkipThisVersion();
            Close();
        }

        private void GuiButtonRemindMeLater_Click(object sender, RoutedEventArgs e)
        {
            _isSentCommandToUpdater = true;
            Updater.Cancel();
            Close();
        }

        private void GuiButtonInstallUpdate_Click(object sender, RoutedEventArgs e)
        {
            _isSentCommandToUpdater = true;
            Updater.Continue();
            Close();
        }

        private void GuiWebBrowser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (GuiWebBrowser.Source == null)
                return;

            e.Cancel = true;
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _currentWindow = null;
            if (_isSentCommandToUpdater==false)
                Updater.Cancel();
        }

    }
}
