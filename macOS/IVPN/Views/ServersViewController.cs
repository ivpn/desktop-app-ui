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
using Foundation;
using AppKit;
using IVPN.ViewModels;
using System.ComponentModel;
using IVPN.Models;
using CoreGraphics;
using System.Collections.Generic;

namespace IVPN
{
    public partial class ServersViewController : AppKit.NSViewController
    {
        private ServerListViewModel __ViewModel;
        private NSView __ServersListView;

        private ServerFastestSelectionButton __FastestServerButton;
        private readonly List<ServerSelectionButton> __ServerButtons = new List<ServerSelectionButton>();

        /*
         * // Implementation of 'Config' button (will be in use after implementation configuration for each server)
         * 
        private bool __IsServersUpdateRequired;
        private readonly CustomButton __ConfigureButton;
        private bool __IsConfigurationMode;
        */
        #region Constructors

        // Called when created from unmanaged code
        public ServersViewController (IntPtr handle) : base (handle)
        {
            Initialize ();
        }

        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public ServersViewController (NSCoder coder) : base (coder)
        {
            Initialize ();
        }

        // Call to load from the XIB/NIB file
        public ServersViewController (CustomButton configureButton) : base ("ServersView", NSBundle.MainBundle)
        {
            // Implementation of 'Config' button (will be in use after implementation configuration for each server)
            // __ConfigureButton = configureButton;
            Initialize ();
        }

        // Shared initialization code
        void Initialize ()
        {
            /*
             * Implementation of 'Config' button (will be in use after implementation configuration for each server)
             * 
            if (__ConfigureButton!=null)
                __ConfigureButton.Activated += __ConfigureButton_Activated;
            */               
        }

        #endregion

        //strongly typed view accessor
        public new ServersView View 
        {
            get { return (ServersView)base.View; }
        }

        /*
         * Implementation of 'Config' button (will be in use after implementation configuration for each server)
         * 
        public bool IsConfigMode
        {
            get => __IsConfigurationMode;
            set
            {
                __IsConfigurationMode = value;
                UpdateConfigMode();
            }
        }
        */

        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();

            ScrollViewer.BorderType = NSBorderType.NoBorder;
        }

        public override void ViewWillAppear ()
        {
            base.ViewWillAppear ();

            UpdateCaptionText (__ViewModel.ServerSelectionType);
            UpdateServersButtons();
        }

        public void SetViewModel (ServerListViewModel viewModel)
        {
            __ViewModel = viewModel;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            viewModel.OnError += ViewModel_OnError;

            viewModel.Service.Servers.PropertyChanged += Servers_PropertyChanged;
            viewModel.Service.Servers.OnPingsUpdated += Servers_OnPingsUpdated;
        }

        void ViewModel_OnError (string errorText, string errorDescription = "")
        {
            var alert = new NSAlert ();

            alert.MessageText = errorText;
            alert.InformativeText = errorDescription;

            alert.AddButton (LocalizedStrings.Instance.LocalizedString("Button_Close"));
            alert.BeginSheetForResponse (View.Window, (result) => { });
        }

        private void ViewModel_PropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread (() => ViewModel_PropertyChanged (sender, e));
                return;
            }

            switch (e.PropertyName) 
            {
            case nameof (__ViewModel.ServerSelectionType):
                UpdateCaptionText (__ViewModel.ServerSelectionType);
                break;

            case nameof (__ViewModel.DisallowedCountryCode):
                UpdateDisallowedServers ();
                break;
            }
        }

        void Servers_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => ViewModel_PropertyChanged(sender, e));
                return;
            }

            if (e.PropertyName == nameof(__ViewModel.Service.Servers.ServersList))
            {
                /*
                 * Implementation of 'Config' button (will be in use after implementation configuration for each server)
                 * 
                __IsServersUpdateRequired = true;
                if (!__IsConfigurationMode)
                    UpdateServersButtons();
                */
                UpdateServersButtons();
            }
        }

        void Servers_OnPingsUpdated()
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(Servers_OnPingsUpdated);
                return;
            }

            foreach (var btn in __ServerButtons)
                btn.UpdateUI();
        }

        private void UpdateCaptionText (ServerSelectionType serverSelectionType)
        {
            if (SelectServerText == null)
                return;

            switch (serverSelectionType) 
            {
            case ServerSelectionType.SingleServer:
                    SelectServerText.StringValue = LocalizedStrings.Instance.LocalizedString ("Button_SelectServer");
                break;
            case ServerSelectionType.EntryServer:
                    SelectServerText.StringValue = LocalizedStrings.Instance.LocalizedString ("Button_SelectEntryServer");
                break;
            case ServerSelectionType.ExitServer:
                    SelectServerText.StringValue = LocalizedStrings.Instance.LocalizedString ("Button_SelectExitServer");
                break;
            }

            SelectServerText.SizeToFit ();
        }

        private void UpdateDisallowedServers ()
        {
            foreach (var view in __ServersListView.Subviews) 
            {
                var serverSelectionButton = view as ServerSelectionButton;

                if (serverSelectionButton == null)
                    continue;

                serverSelectionButton.DisabledForSelection =
                    serverSelectionButton.ServerLocation.CountryCode == __ViewModel.DisallowedCountryCode;
            }
        }

        private void AddNewLocation (ViewStacker stacker, ServerLocation location)
        {
            var btn = new ServerSelectionButton (location);
            // Implementation of 'Config' button (will be in use after implementation configuration for each server)
            //btn.IsConfigMode = IsConfigMode;

            if (location.CountryCode == __ViewModel.DisallowedCountryCode)
                btn.DisabledForSelection = true;

            btn.Activated += (object sender, EventArgs e) => 
            {
                __ViewModel.SelectServerCommand.Execute (((ServerSelectionButton)sender).ServerLocation);
            };
            btn.OnConfigButtonPressed += (object sender, EventArgs e) => 
            { 

            };

            __ServerButtons.Add(btn);
            stacker.Add (btn);
        }

        private void UpdateServersButtons ()
        {
            if (ServersView == null)
                return;

            // Implementation of 'Config' button (will be in use after implementation configuration for each server)
            //__IsServersUpdateRequired = false;

            if (__FastestServerButton!=null)
                __FastestServerButton.IsSelected = __ViewModel.IsAutomaticServerSelected;

            __ServerButtons.Clear();
            ViewStacker stacker = new ViewStacker ();

            // automatic setver selection button
            __FastestServerButton = null;
            if (__ViewModel.ServerSelectionType == ServerSelectionType.SingleServer)
            {
                // automatic server selection is allowed only for singlehop
                __FastestServerButton = new ServerFastestSelectionButton(__ViewModel.IsAutomaticServerSelected);

                //  Implementation of 'Config' button (will be in use after implementation configuration for each server)
                //__FastestServerButton.IsConfigMode = IsConfigMode;
                __FastestServerButton.IsConfigMode = true;

                __FastestServerButton.Activated += (object sender, EventArgs e) =>
                {
                    __ViewModel.SelectAutomaticServerSelectionCommand.Execute(null);
                };
                __FastestServerButton.OnConfigButtonPressed += (object sender, EventArgs e) =>
                {
                    __ViewModel.ConfigureAutomaticServerSelectionCommand.Execute(null);
                };
                stacker.Add(__FastestServerButton);
            }

            // servers buttons
            List<ServerLocation> servers = new List<ServerLocation>(__ViewModel.Service.Servers.ServersList);

            foreach (var location in servers)
                AddNewLocation (stacker, location);

            stacker.Add (new MarginControl (10));

            var newView = stacker.CreateView ((float)ScrollViewer.Frame.Height);
            ServersView.Frame = newView.Frame;

            if (__ServersListView == null)
            {
                ServersView.AddSubview(newView);
                ServersView.ScrollPoint(new CGPoint(0, newView.Frame.Bottom));
            }
            else
                ServersView.ReplaceSubviewWith(__ServersListView, newView);

            __ServersListView = newView;

            UpdateDisallowedServers();
        }

        /*
         *  Implementation of 'Config' button (will be in use after implementation configuration for each server)
         * 
        void __ConfigureButton_Activated(object sender, EventArgs e)
        {
            IsConfigMode = !IsConfigMode;
        }

        private void UpdateConfigMode()
        {
            if (IsConfigMode)
            {
                CustomButtonStyles.ApplyStyleTitleConfigureButtonPressed(__ConfigureButton);

                if (__FastestServerButton != null)
                    __FastestServerButton.IsConfigMode = true;

                foreach (var btn in __ServerButtons)
                    btn.IsConfigMode = true;
            }
            else
            {
                CustomButtonStyles.ApplyStyleTitleConfigureButton(__ConfigureButton);

                if (__FastestServerButton!=null)
                    __FastestServerButton.IsConfigMode = false;

                foreach (var btn in __ServerButtons)
                    btn.IsConfigMode = false;

                if (__IsServersUpdateRequired)
                    UpdateServersButtons();
            }
        }
        */
    }
}
