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

﻿using AppKit;
using Foundation;
using IVPN.ViewModels;

namespace IVPN
{
    public class ProofsViewModelAdapter : ObservableObject
    {
        private ProofsViewModel __ProofsViewModel;
        public ProofsViewModelAdapter(ProofsViewModel proofsViewModel) : base(proofsViewModel)
        {
            __ProofsViewModel = proofsViewModel;
            __ProofsViewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName.Equals(nameof(ProofsViewModel.GeoLookup)))
                {
                    if (__ProofsViewModel.GeoLookup == null)
                        GeoLookup = null;
                    else
                        GeoLookup = new ObservableObject(__ProofsViewModel.GeoLookup);
                }
                else if (e.PropertyName.Equals(nameof(ProofsViewModel.State)))
                {
                    WillChangeValue("IsProgress");
                    DidChangeValue("IsProgress");
                }
            };


        }

        [Export("geoLookup")]
        public ObservableObject GeoLookup 
        {
            get => __GeoLookup;
            private set
            {
                WillChangeValue("CountryImage");
                WillChangeValue("isSecured");
                WillChangeValue("isNotSecured");
                WillChangeValue("geoLookup");
                __GeoLookup = value;
                DidChangeValue("geoLookup");
                DidChangeValue("isNotSecured");
                DidChangeValue("isSecured");
                DidChangeValue("CountryImage");
            } 
        }
        private ObservableObject __GeoLookup;

        [Export("isSecured")]
        public bool IsSecured
        {
            get
            {
                var geoInfo = __ProofsViewModel.GeoLookup;
                if (geoInfo == null || geoInfo.IsIvpnServer == false)
                    return false;
                return true;
            }
        }
        [Export("isNotSecured")]
        public bool IsNotSecured => !IsSecured;

        [Export("CountryImage")]
        public NSImage CountryImage
        {
            get
            {
                var geoInfo = __ProofsViewModel.GeoLookup;
                if (geoInfo != null)
                    return GuiHelpers.CountryCodeToImage.GetCountryFlagFromAllCountriesSet(geoInfo.CountryCode);
                return null;
            }
        }

        [Export("IsProgress")]
        public bool IsProgress
        {
            get
            {
                return __ProofsViewModel.State == ProofsViewModel.StateEnum.Updating;
            }
        }
    }
}
