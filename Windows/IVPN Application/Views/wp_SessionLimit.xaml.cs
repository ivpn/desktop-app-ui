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
using System.Windows.Controls;

namespace IVPN.Views
{
    /// <summary>
    /// Interaction logic for wp_SessionLimit.xaml
    /// </summary>
    public partial class wp_SessionLimit : Page
    {
        private ViewModelSessionLimit __SessionLimitModel;
        public wp_SessionLimit()
        {
            InitializeComponent();

            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null)
                throw new InvalidOperationException("App.Current.MainWindow as MainWindow == null");

            __SessionLimitModel = mainWindow.SessionLimitViewModel;
            DataContext = __SessionLimitModel;
        }

        private void GuiButtonUpgradePlan_OnClick(object sender, RoutedEventArgs e)
        {
            __SessionLimitModel.UpgradeToProPlanCommand.Execute(null);
        }

        private void GuiButtonLogOutAllDevices_OnClick(object sender, RoutedEventArgs e)
        {
            __SessionLimitModel.LogOutAllSessionsCommand.Execute(null);
        }

        private void GuiTryAgain_OnClick(object sender, RoutedEventArgs e)
        {
            __SessionLimitModel.TryAgainCommand.Execute(null);
        }
    }
}
