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
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using Foundation;
using IVPN.ViewModels;

namespace IVPN
{
    public partial class FastestServerConfigViewController : AppKit.NSViewController
    {
        ViewModelFastestServerSettings __ViewModel;
        private readonly List<ServerCheckButton> __ServerButtons = new List<ServerCheckButton>();
        private NSView __ServersListView;

        #region Constructors

        // Called when created from unmanaged code
        public FastestServerConfigViewController(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public FastestServerConfigViewController(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        // Call to load from the XIB/NIB file
        public FastestServerConfigViewController() : base("FastestServerConfigView", NSBundle.MainBundle)
        {
            Initialize();
        }

        // Shared initialization code
        void Initialize()
        {
        }

        #endregion

        public override void ViewWillAppear()
        {
            base.ViewWillAppear();
            UpdateServersButtons();
        }

        //strongly typed view accessor
        public new FastestServerConfigView View
        {
            get
            {
                return (FastestServerConfigView)base.View;
            }
        }

        public void SetViewModel(ViewModelFastestServerSettings viewModel)
        {
            __ViewModel = viewModel;
            __ViewModel.OnError += __ViewModel_OnError;

            __ViewModel.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName.Equals(nameof(__ViewModel.Items)))
                {
                    UpdateServersButtons();
                }
            };
            UpdateServersButtons();
        }

        private void AddNewLocation(ViewStacker stacker, ViewModelFastestServerSettings.SelectionItem item)
        {
            var btn = new ServerCheckButton(item.ServerInfo, item.IsSelected);

            btn.OnChecked += () => 
            {
                item.IsSelected = btn.IsChecked;
                // confirm that value was changed
                btn.IsChecked = item.IsSelected;
            };

            __ServerButtons.Add(btn);
            stacker.Add(btn);
        }

        private void UpdateServersButtons()
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(UpdateServersButtons);
                return;
            }

            if (UiServersView == null)
                return;

            __ServerButtons.Clear();
            ViewStacker stacker = new ViewStacker();

            foreach (ViewModelFastestServerSettings.SelectionItem item in __ViewModel.Items)
                AddNewLocation(stacker, item);

            stacker.Add(new MarginControl(10));

            var newView = stacker.CreateView((float)UiScrollViewer.Frame.Height);
            UiServersView.Frame = newView.Frame;

            if (__ServersListView == null)
            {
                UiServersView.AddSubview(newView);
                UiServersView.ScrollPoint(new CGPoint(0, newView.Frame.Bottom));
            }
            else
                UiServersView.ReplaceSubviewWith(__ServersListView, newView);

            __ServersListView = newView;
        }

        void __ViewModel_OnError(string errorText, string errorDescription = "")
        {
            var alert = new NSAlert();

            alert.MessageText = errorText;
            alert.InformativeText = errorDescription;

            alert.AddButton(LocalizedStrings.Instance.LocalizedString("Button_Close"));
            alert.BeginSheetForResponse(View.Window, (result) => { });
        }
    }
}
