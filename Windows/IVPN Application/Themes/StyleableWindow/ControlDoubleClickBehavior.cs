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
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfStyleableWindow.StyleableWindow
{
    public static class ControlDoubleClickBehavior
    {
        public static ICommand GetExecuteCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(ExecuteCommand);
        }

        public static void SetExecuteCommand(DependencyObject obj, ICommand command)
        {
            obj.SetValue(ExecuteCommand, command);
        }

        public static readonly DependencyProperty ExecuteCommand = DependencyProperty.RegisterAttached("ExecuteCommand",          
            typeof(ICommand), typeof(ControlDoubleClickBehavior),
            new UIPropertyMetadata(null, OnExecuteCommandChanged));

        public static Window GetExecuteCommandParameter(DependencyObject obj)
        {
            return (Window) obj.GetValue(ExecuteCommandParameter);
        }

        public static void SetExecuteCommandParameter(DependencyObject obj, ICommand command)
        {
            obj.SetValue(ExecuteCommandParameter, command);
        }

        public static readonly DependencyProperty ExecuteCommandParameter = DependencyProperty.RegisterAttached("ExecuteCommandParameter",
            typeof(Window), typeof(ControlDoubleClickBehavior));

        private static void OnExecuteCommandChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as Control;

            if (control != null)
            {
                control.MouseDoubleClick += control_MouseDoubleClick;
            }
        }

        static void control_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var control = sender as Control;
            
            if(control != null)
            {
                var command = control.GetValue(ExecuteCommand) as ICommand;
                var commandParameter = control.GetValue(ExecuteCommandParameter);

                if (command.CanExecute(e))
                {
                    command.Execute(commandParameter);
                }
            }
        }       
    }
}
