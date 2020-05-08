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

﻿using IVPN.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace IVPN.Controls
{
    public class NotificationWindow : Window
    {        
        private bool __IsClosing;
        private Window __ParentWindow;
        
        public NotificationWindow()            
        {            
            Loaded += NotificationWindow_Loaded;
            Deactivated += NotificationWindow_Deactivated;
            DataContext = this;

            CloseCommand = new RelayCommand(CloseWindow);
            WindowStyle = System.Windows.WindowStyle.None;
        }

        void NotificationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateWindowLocation();
        }

        private void CloseWindow()
        {
            if (!__IsClosing)
                Close();
        }

        void NotificationWindow_Deactivated(object sender, EventArgs e)
        {
            if (!__IsClosing)
                Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            __IsClosing = true;
            base.OnClosing(e);            
        }

        public double VerticalOffset { get; set; }

        public double HorizontalOffset { get; set; }

        public Window ParentWindow
        {
            get
            {
                return __ParentWindow;
            }
            set
            {                
                __ParentWindow = value;                
                __ParentWindow.LocationChanged += (sender, e) =>
                                    {
                                        UpdateWindowLocation();
                                    };

                App.Current.Deactivated += (sender, e) =>
                                    {
                                        if (!__IsClosing)
                                            Close();
                                    };
            }
        }

        private void UpdateWindowLocation()
        {
            Top = ParentWindow.Top + VerticalOffset - ActualHeight;
            Left = ParentWindow.Left + HorizontalOffset - ActualWidth / 2;
        }

        public ICommand CloseCommand { get; }
    }
}
