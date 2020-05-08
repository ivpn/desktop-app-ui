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

using System.ComponentModel;

using Foundation;
using IVPN.ViewModels;

namespace IVPN
{
    public class MainViewModelAdapter: ObservableObject
    {
        private ConnectionInfoAdapter __ConnectionInfo;

        public MainViewModelAdapter(MainViewModel mainViewModel): base(mainViewModel)
        {
            mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
            mainViewModel.PropertyWillChange += MainViewModel_PropertyWillChange;

            if (ViewModel.ConnectionInfo != null)
                ConnectionInfoAdapter = new ConnectionInfoAdapter(ViewModel.ConnectionInfo);
        }

        void MainViewModel_PropertyWillChange(object sender, PropertyChangingEventArgs e)
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => MainViewModel_PropertyWillChange(sender, e));
                return;
            }

            WillChangeValue(e.PropertyName);
        }

        void MainViewModel_PropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => MainViewModel_PropertyChanged(sender, e));
                return;
            }

            DidChangeValue(e.PropertyName);

            if (e.PropertyName == ViewModel.GetPropertyName(() => ViewModel.ConnectionInfo)) 
            {
                if (ViewModel.ConnectionInfo != null)
                    ConnectionInfoAdapter = new ConnectionInfoAdapter(ViewModel.ConnectionInfo);                
            }
            else if (e.PropertyName.Equals(nameof(MainViewModel.ConnectionState))
                    || e.PropertyName.Equals(nameof(MainViewModel.PauseStatus))
                    )
            {
                WillChangeValue("isConnected");
                DidChangeValue("isConnected");

                WillChangeValue("isDisconnected");
                DidChangeValue("isDisconnected");
            }

        }
            
        [Export("connectionInfoAdapter")]
        public ConnectionInfoAdapter ConnectionInfoAdapter
        {
            get {
                return __ConnectionInfo;
            }
            set {
                WillChangeValue("connectionInfoAdapter");
                __ConnectionInfo = value;
                DidChangeValue("connectionInfoAdapter");
            }
        }


        [Export("isConnected")]
        public bool IsConnected => ViewModel.ConnectionState == Models.ServiceState.Connected && ViewModel.PauseStatus == MainViewModel.PauseStatusEnum.Resumed;

        [Export("isDisconnected")]
        public bool IsDisconnected => !IsConnected;


        public MainViewModel ViewModel
        {
            get 
            {
                return (MainViewModel)ObservedObject;
            }
        }
    }
}

