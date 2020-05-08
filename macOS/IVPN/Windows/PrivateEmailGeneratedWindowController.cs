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
using IVPN.Models.PrivateEmail;
using IVPN.GuiHelpers;

namespace IVPN
{
    public partial class PrivateEmailGeneratedWindowController : NSWindowController
    {
        private PrivateEmailsManagerViewModel __Model;
        private PrivateEmailInfo __GeneratedEmailInfo;
        private bool __NeedToCloseWindow;

        private static PrivateEmailGeneratedWindowController __Instance;

        public PrivateEmailGeneratedWindowController (IntPtr handle) : base (handle)
        {
        }

        [Export ("initWithCoder:")]
        public PrivateEmailGeneratedWindowController (NSCoder coder) : base (coder)
        {
        }

        public static void Generate(PrivateEmailsManagerViewModel model)
        {
            __Instance?.Close();
            __Instance = new PrivateEmailGeneratedWindowController(model);
            __Instance.Window.Center();
            __Instance.ShowWindow(null);
        }

        public static void CloseAllWindows()
        {
            __Instance?.Close();
        }

        private PrivateEmailGeneratedWindowController (PrivateEmailsManagerViewModel model) : base ("PrivateEmailGeneratedWindow")
        {
            __Model = model;

            __Model.OnError += __Model_OnError;

            __Model.OnWillExecute += (sender) => 
            {
                GuiProgressIndicator.StartAnimation (this);
                this.Window.ContentView = GuiInProgressView;
            };

            __Model.OnDidExecute += (sender) => 
            {
                GuiProgressIndicator.StopAnimation (this);
                this.Window.ContentView = GuiMainView;    

                if (__NeedToCloseWindow)
                    Close ();
            };

            __Model.OnNewEmailGenerated += (PrivateEmailInfo emailInfo) => 
            {
                __GeneratedEmailInfo = emailInfo;

                GuiGeneratedEmailField.StringValue = emailInfo.Email;
                GuiForwardToEmailField.StringValue = emailInfo.ForwardToEmail;
                GuiNotesField.TextStorage.SetString (new NSAttributedString(emailInfo.Notes));
            };
        }

        public override void Close ()
        {
            __Model.OnError -= __Model_OnError;
            base.Close ();
        }

        void __Model_OnError (string errorText, string errorDescription = "")
        {
            if (Window.IsVisible)
            {
                if (string.IsNullOrEmpty(errorDescription))
                    IVPNAlert.Show(errorText);
                else
                    IVPNAlert.Show(errorText, errorDescription);
            }
            Close ();
        }

        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();

            // Disable title-bar (but keep close/minimize/expand buttons on content-view)
            Window.TitleVisibility = NSWindowTitleVisibility.Hidden;
            Window.TitlebarAppearsTransparent = true;
            Window.StyleMask |= NSWindowStyle.FullSizeContentView;

            // set window background color
            //if (!Colors.IsDarkMode)
            //    Window.BackgroundColor = NSColor.FromRgba (255, 255, 255, 0.95f);

            //Stylyze buttons
            CustomButtonStyles.ApplyStyleGreyButtonV2(GuiBtnCopy, LocalizedStrings.Instance.LocalizedString ("Button_PrivateEmail_Copy"));
            CustomButtonStyles.ApplyStyleGreyButtonV2(GuiBtnDelete, LocalizedStrings.Instance.LocalizedString ("Button_PrivateEmail_Discard"));
            CustomButtonStyles.ApplyStyleMainButton(GuiBtnOk, LocalizedStrings.Instance.LocalizedString ("Button_OK"));

            // set padding for Notes control
            GuiNotesField.TextContainerInset = new CoreGraphics.CGSize (5, 5);

            __Model.GenerateNewEmail ();
        }

        public new PrivateEmailGeneratedWindow Window {
            get { return (PrivateEmailGeneratedWindow)base.Window; }
        }

        partial void OnButtonCopy (Foundation.NSObject sender)
        {
            NSPasteboard pb = NSPasteboard.GeneralPasteboard;
            pb.DeclareTypes (new string [] { NSPasteboard.NSStringType }, null);
            pb.SetStringForType (GuiGeneratedEmailField.StringValue, NSPasteboard.NSStringType);
        }

        partial void OnButtonDelete (Foundation.NSObject sender)
        {
            if (__GeneratedEmailInfo != null) 
            {
                __NeedToCloseWindow = true;
                __Model.DeleteEmail (__GeneratedEmailInfo);
            }
        }

        partial void OnButtonOk (Foundation.NSObject sender)
        {
            if (__GeneratedEmailInfo==null)
            {
                Close ();
                return;
            }

            if (!String.Equals (GuiNotesField.String, __GeneratedEmailInfo.Notes))
            {
                __NeedToCloseWindow = true;
                __Model.UpdateNotes (__GeneratedEmailInfo, GuiNotesField.String);
                return;
            }

            Close ();
        }
    }
}
