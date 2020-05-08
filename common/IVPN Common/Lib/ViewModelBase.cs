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
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace IVPN.ViewModels
{
    // TODO: inherite it from ModelBase (will avoid implementation 'property changed' events here)
    public class ViewModelBase : INotifyPropertyChanged
    {
        public delegate void OnErrorDelegate (string errorText, string errorDescription = "");
        public event OnErrorDelegate OnError = delegate { };
        protected void NotifyError(string errorText, string errorDescription = "")
        {
            OnError (errorText, errorDescription);
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public event PropertyChangingEventHandler PropertyWillChange = delegate { };

        // TODO: redundant method (on method call can be used 'nameof()')
        protected void RaisePropertyChanged<T>(Expression<Func<T>> exp)
        {
            var propertyName = GetPropertyName(exp);
            RaisePropertyChanged(propertyName);
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        // TODO: redundant method (on method call can be used 'nameof()')
        protected void RaisePropertyWillChange<T>(Expression<Func<T>> exp)
        {
            var propertyName = GetPropertyName(exp);
            RaisePropertyWillChange(propertyName);
        }

        protected void RaisePropertyWillChange([CallerMemberName] string propertyName = "")
        {
            PropertyWillChange(this, new PropertyChangingEventArgs(propertyName));
        }

        // TODO: redundant method (on method call can be used 'nameof()')
        public string GetPropertyName<T>(Expression<Func<T>> exp)
        {
            return (((MemberExpression)(exp.Body)).Member).Name;
        }
    }
}
