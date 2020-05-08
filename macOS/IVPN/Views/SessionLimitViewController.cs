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
using AppKit;
using Foundation;
using IVPN.ViewModels;

namespace IVPN
{
    public partial class SessionLimitViewController : AppKit.NSViewController
    {
        private ViewModelSessionLimit __SessionLimitViewModel;
        private CoreGraphics.CGRect __InitialLogoutAllBtnFrame;
        #region Constructors

        // Called when created from unmanaged code
        public SessionLimitViewController(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public SessionLimitViewController(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        // Call to load from the XIB/NIB file
        public SessionLimitViewController() : base("SessionLimitView", NSBundle.MainBundle)
        {
            Initialize();
        }


        // Shared initialization code
        void Initialize()
        {
            
        }

        #endregion

        //strongly typed view accessor
        public new SessionLimitView View
        {
            get
            {
                return (SessionLimitView)base.View;
            }
        }

        public void SetViewModel(ViewModelSessionLimit viewModel)
        {
            __SessionLimitViewModel = viewModel;
            __SessionLimitViewModel.PropertyChanged += __SessionLimitViewModel_PropertyChanged;
        }

        private void __SessionLimitViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => __SessionLimitViewModel_PropertyChanged(sender, e));
                return;
            }

            if (nameof(ViewModelSessionLimit.IsCanLogOutAllSessions).Equals(e.PropertyName)
                || nameof(ViewModelSessionLimit.IsCanUpgrade).Equals(e.PropertyName))
            {
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            if (__SessionLimitViewModel == null)
                return;

            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(() => UpdateUI());
                return;
            }

            CustomButtonStyles.ApplyStyleNavigationButton(GuiButtonBack, LocalizedStrings.Instance.LocalizedString("Button_Back"));
            if (__SessionLimitViewModel.IsCanUpgrade)
            {
                UIButtonUpgrade.Hidden = false;
                UIButtonLogOutAll.Frame = __InitialLogoutAllBtnFrame;

                CustomButtonStyles.ApplyStyleMainButton(UIButtonUpgrade, LocalizedStrings.Instance.LocalizedString("Button_SwitchToProPlan", "Switch to IVPN Pro plan"));
                CustomButtonStyles.ApplyStyleSecondaryButton(UIButtonLogOutAll, LocalizedStrings.Instance.LocalizedString("Button_LogOutOtherDevices", "Log out from all other devices"));
            }
            else
            {   
                UIButtonLogOutAll.Frame = UIButtonUpgrade.Frame;
                UIButtonUpgrade.Hidden = true;
                CustomButtonStyles.ApplyStyleMainButton(UIButtonLogOutAll, LocalizedStrings.Instance.LocalizedString("Button_LogOutOtherDevices", "Log out from all other devices"));
            }
           
            UIButtonLogOutAll.Hidden = !__SessionLimitViewModel.IsCanLogOutAllSessions;
        }
            
        public void Navigated()
        {
            // 
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            __InitialLogoutAllBtnFrame = UIButtonLogOutAll.Frame;

            UpdateUI();

            UIButtonTryAgain.AttributedTitle = new NSAttributedString(
                LocalizedStrings.Instance.LocalizedString("Button_TryAgain", "Try again"),
                new NSStringAttributes
                {
                    ForegroundColor = NSColor.FromRgb(23, 143, 230),
                    Font = UIUtils.GetSystemFontOfSize(14f),
                    ParagraphStyle = new NSMutableParagraphStyle { Alignment = NSTextAlignment.Center }
                });

            // witching light\dark theme
            View.OnApperianceChanged += () =>
            {
                UpdateUI();
            };
        }

        partial void OnButtonLogoutAllDevices(Foundation.NSObject sender)
        {
            __SessionLimitViewModel.LogOutAllSessionsCommand.Execute(null);
        }

        partial void OnButtonTryAgain(Foundation.NSObject sender)
        {
            __SessionLimitViewModel.TryAgainCommand.Execute(null);
        }

        partial void OnButtonUpgradePlan(Foundation.NSObject sender)
        {
            __SessionLimitViewModel.UpgradeToProPlanCommand.Execute(null);
        }

        partial void OnGoBackButtonPressed(Foundation.NSObject sender)
        {
            __SessionLimitViewModel.GoBackCommand.Execute(null);
        }
    }
}
