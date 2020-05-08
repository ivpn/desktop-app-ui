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
using IVPN.GuiHelpers;
using IVPN.Models.Session;

namespace IVPN
{
    public partial class SubscriptionWillExpireWindowController : NSWindowController
    {
        private int __DaysLeft;
        AccountStatus __SessionStatus;
        string __Username;

        public SubscriptionWillExpireWindowController(IntPtr handle) : base(handle)
        {
        }

        [Export("initWithCoder:")]
        public SubscriptionWillExpireWindowController(NSCoder coder) : base(coder)
        {
        }

        public SubscriptionWillExpireWindowController(AccountStatus sessionStatus, string username) : base("SubscriptionWillExpireWindow")
        {
            __SessionStatus = sessionStatus;
            __Username = username ?? "";
            __DaysLeft = (int)(__SessionStatus.ActiveUtil - DateTime.Now).TotalDays;
            if (__DaysLeft < 0)
                __DaysLeft = 0;
        }

        private readonly NSColor TitleDaysTextColor = NSColor.FromRgb(57, 158, 230);

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            // Disable title-bar (but keep close/minimize/expand buttons on content-view)
            // IMPORTANT! 'FullSizeContentView' implemented since OS X 10.10 !!!
            Window.TitleVisibility = NSWindowTitleVisibility.Hidden;
            Window.TitlebarAppearsTransparent = true;
            Window.StyleMask |= NSWindowStyle.FullSizeContentView;

            // Progress indicator
            if (!__SessionStatus.IsActive)
                __DaysLeft = 0;

            // Normally we should show it days left <= 3
            // if '__DaysLeft' > 3 - set max value to __DaysLeft+1
            int maxValue = 3;
            if (__DaysLeft > 3)
                maxValue = __DaysLeft + 1;

            GuiProgressIndicator.MinValue = 0;
            GuiProgressIndicator.MaxValue = maxValue;
            GuiProgressIndicator.DoubleValue = maxValue - __DaysLeft;

            string cancelBtnText = LocalizedStrings.Instance.LocalizedString("Button_ContinueTrial");
            string subscriptionBtnText;
            if (__SessionStatus.IsOnFreeTrial)
                subscriptionBtnText = LocalizedStrings.Instance.LocalizedString("Button_GetSubscription");
            else
                subscriptionBtnText = LocalizedStrings.Instance.LocalizedString("Button_RenewSubscription");

            // BUTTON Continue Trial
            GuiButtonCancel.Gradient = new NSGradient(NSColor.FromRgb(240, 244, 247), NSColor.FromRgb(240, 244, 247));
            GuiButtonCancel.BorderColor = NSColor.SystemGrayColor;
            GuiButtonCancel.TitleForegroundColor = NSColor.Black;
            GuiButtonCancel.TitleFont = UIUtils.GetSystemFontOfSize(13f, NSFontWeight.Medium);
            GuiButtonCancel.TitleText = cancelBtnText;

            // BUTTON Get Subscription
            GuiButtonGoToAccount.Gradient = new NSGradient(NSColor.FromRgb(128, 187, 249), NSColor.FromRgb(17, 130, 254));
            GuiButtonGoToAccount.BorderShadow = new NSShadow
            {
                ShadowOffset = new CoreGraphics.CGSize(0f, 1f),
                ShadowColor = NSColor.FromRgba(0, 0, 0, 0.18f)
            };
            GuiButtonGoToAccount.TitleText = subscriptionBtnText;
            GuiButtonGoToAccount.TitleFont = UIUtils.GetSystemFontOfSize(13f, NSFontWeight.Medium);
            GuiButtonGoToAccount.TitleForegroundColor = NSColor.White;

            // TITLE
            if (!__SessionStatus.IsActive)
            {
                string title = LocalizedStrings.Instance.LocalizedString("Label_SubscriptionExpired");
                if (__SessionStatus.IsOnFreeTrial)
                    title = LocalizedStrings.Instance.LocalizedString("Label_FreeTrialExpired");

                NSMutableAttributedString attrTitle = new NSMutableAttributedString(title);

                NSStringAttributes stringAttributes0 = new NSStringAttributes();
                stringAttributes0.Font = UIUtils.GetSystemFontOfSize(20f, NSFontWeight.Medium);
                attrTitle.AddAttributes(stringAttributes0, new NSRange(0, title.Length));
                GuiLabelTitleText.AttributedStringValue = attrTitle;

                // DESCRIPTION
                string description = LocalizedStrings.Instance.LocalizedString("Label_AccountDaysLeftDescription_Expired");
                if (__SessionStatus.IsOnFreeTrial)
                    description = LocalizedStrings.Instance.LocalizedString("Label_TrialDaysLeftDescription_Expired");

                description = string.Format(description, __DaysLeft);
                GuiLabelDescriptionText.AttributedStringValue = AttributedString.Create(description, null, NSTextAlignment.Left);
            }
            else
            {
                string title;
                string daysStr = string.Format("{0}", __DaysLeft);
                if (__DaysLeft == 0)
                {
                    title = LocalizedStrings.Instance.LocalizedString("Label_AccountDaysLeftTitle_LastDay_PARAMETRIZED");
                    if (__SessionStatus.IsOnFreeTrial)
                        title = LocalizedStrings.Instance.LocalizedString("Label_TrialDaysLeftTitle_LastDay_PARAMETRIZED");
                    daysStr = "";
                }
                else if (__DaysLeft == 1)
                {
                    title = LocalizedStrings.Instance.LocalizedString("Label_AccountDaysLeftTitle_OneDay_PARAMETRIZED");
                    if (__SessionStatus.IsOnFreeTrial)
                        title = LocalizedStrings.Instance.LocalizedString("Label_TrialDaysLeftTitle_OneDay_PARAMETRIZED");
                }
                else
                {
                    title = LocalizedStrings.Instance.LocalizedString("Label_AccountDaysLeftTitle_PARAMETRIZED");
                    if (__SessionStatus.IsOnFreeTrial)
                        title = LocalizedStrings.Instance.LocalizedString("Label_TrialDaysLeftTitle_PARAMETRIZED");
                }

                int numberSymbolPos = title.LastIndexOf("{0}", StringComparison.Ordinal);
                title = string.Format(title, daysStr);
                NSMutableAttributedString attrTitle = new NSMutableAttributedString(title);

                NSStringAttributes stringAttributes0 = new NSStringAttributes();
                stringAttributes0.Font = UIUtils.GetSystemFontOfSize(20f, NSFontWeight.Medium);

                NSStringAttributes stringAttributes1 = new NSStringAttributes();
                stringAttributes1.ForegroundColor = TitleDaysTextColor;

                attrTitle.AddAttributes(stringAttributes0, new NSRange(0, title.Length));
                attrTitle.AddAttributes(stringAttributes1, new NSRange(numberSymbolPos, title.Length - numberSymbolPos));

                GuiLabelTitleText.AttributedStringValue = attrTitle;

                // DESCRIPTION
                string description;
                if (__DaysLeft == 0)
                {
                    description = LocalizedStrings.Instance.LocalizedString("Label_AccountDaysLeftDescription_LastDay");
                    if (__SessionStatus.IsOnFreeTrial)
                        description = LocalizedStrings.Instance.LocalizedString("Label_TrialDaysLeftDescription_LastDay");
                }
                else if (__DaysLeft == 1)
                {
                    description = LocalizedStrings.Instance.LocalizedString("Label_AccountDaysLeftDescription_OneDay");
                    if (__SessionStatus.IsOnFreeTrial)
                        description = LocalizedStrings.Instance.LocalizedString("Label_TrialDaysLeftDescription_OneDay");
                }
                else
                {
                    description = LocalizedStrings.Instance.LocalizedString("Label_AccountDaysLeftDescription_PARAMETRIZED");
                    if (__SessionStatus.IsOnFreeTrial)
                        description = LocalizedStrings.Instance.LocalizedString("Label_TrialDaysLeftDescription_PARAMETRIZED");
                }

                description = string.Format(description, __DaysLeft);
                GuiLabelDescriptionText.AttributedStringValue = AttributedString.Create(description, null, NSTextAlignment.Left);
            }
        }

        public override void WindowDidLoad()
        {
            base.WindowDidLoad();
        }

        public override void Close()
        {
            base.Close();
        }

        public new SubscriptionWillExpireWindow Window
        {
            get { return (SubscriptionWillExpireWindow)base.Window; }
        }

        partial void GuiButtonGoToAccountPressed(Foundation.NSObject sender)
        {
            NSWorkspace.SharedWorkspace.OpenUrl(new NSUrl(Constants.GetRenewUrl(__Username)));
            Close();
        }

        partial void OnGuiButtonCancelPressed(Foundation.NSObject sender)
        {
            Close();
        }
    }
}
