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
using System.Windows.Input;

namespace IVPN.ViewModels
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> __MethodToExecute;
        private readonly Func<bool> __CanExecuteEvaluator;

        public RelayCommand(Action<T> methodToExecute, Func<bool> canExecuteEvaluator)
        {
            __MethodToExecute = methodToExecute;
            __CanExecuteEvaluator = canExecuteEvaluator;
        }

        public RelayCommand(Action<T> methodToExecute)
            : this(methodToExecute, null)
        {}

        public bool CanExecute(object parameter)
        {
            if (__CanExecuteEvaluator == null)
                return true;
            
            bool result = __CanExecuteEvaluator.Invoke();
            CanExecuteChanged(this, EventArgs.Empty);
            return result;
        }

        public void Execute(object parameter)
        {
            __MethodToExecute.Invoke((T)parameter);
        }

        public event EventHandler CanExecuteChanged = delegate {};
    }
}
