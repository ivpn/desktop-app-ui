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
    /// <summary>
    /// Emails table data source.
    /// </summary>
    public class EmailsTableDataSource : NSTableViewDataSource
    {
        #region Public Variables
        public PrivateEmailsManagerViewModel PrivateEmailsModel { get; private set; }
        #endregion

        #region Constructors
        public EmailsTableDataSource (PrivateEmailsManagerViewModel privateEmailsModel)
        {
            PrivateEmailsModel = privateEmailsModel;
        }
        #endregion

        #region Override Methods
        public override nint GetRowCount (NSTableView tableView)
        {
            return PrivateEmailsModel.PrivateEmails.Count;
        }
        #endregion
    }
    /// <summary>
    /// Email table delegate.
    /// </summary>
    public class EmailTableDelegate : NSTableViewDelegate
    {
        #region Constants 
        private const string CellIdentifier = "Cell";
        #endregion

        #region Private Variables
        private EmailsTableDataSource DataSource;
        #endregion

        #region Constructors
        public EmailTableDelegate (EmailsTableDataSource datasource)
        {
            this.DataSource = datasource;
        }
        #endregion

        #region Override Methods

        public delegate void OnSelectionChangedDelegate ();
        public event OnSelectionChangedDelegate OnSelectionChanged = delegate { };

        public override void SelectionDidChange (NSNotification notification)
        {
            OnSelectionChanged ();
        }

        public override NSView GetViewForItem (NSTableView tableView, NSTableColumn tableColumn, nint row)
        {
            // This pattern allows you reuse existing views when they are no-longer in use.
            // If the returned view is null, you instance up a new view
            // If a non-null view is returned, you modify it enough to reflect the new data
            NSTextField view = (NSTextField)tableView.MakeView (CellIdentifier, this);
            if (view == null) {
                view = new NSTextField ();
                view.Identifier = CellIdentifier;
                view.BackgroundColor = NSColor.Clear;
                //view.TextColor = NSColor.FromRgb (38, 57, 77);
                //view.Font = UIUtils.GetSystemFontOfSize (16);
                view.Bordered = false;
                view.Selectable = false;
                view.Editable = false;
            }

            // Setup view based on the column selected
            //switch (tableColumn.Identifier) 
            switch (tableColumn.Title) 
            {
            case "Private email address":
            //case "EmailColumn":
                view.StringValue = DataSource.PrivateEmailsModel.PrivateEmails [(int)row].Email;
                view.Alignment = NSTextAlignment.Center;
                break;

            case "Notes":
                //case "NoteColumn":
                view.StringValue = DataSource.PrivateEmailsModel.PrivateEmails [(int)row].Notes;
                view.Alignment = NSTextAlignment.Left;
                break;
                /*
            case "Action":
                // Create new button
                var button = new NSButton (new CGRect (0, 0, 16, 16));
                //button.BezelStyle = NSBezelStyle.
                button.SetButtonType (NSButtonType.MomentaryPushIn);
                button.Image = NSImage.ImageNamed ("iconStatusBad");
                //button.Title = "Delete";
                button.Tag = row;
                button.BezelStyle = NSBezelStyle.Recessed;
                button.Bordered = false;
                button.ToolTip = "Remove email";

                // Wireup events
                button.Activated += (sender, e) => {
                    NSButton btn = sender as NSButton;
                    PrivateEmailInfo email = DataSource.PrivateEmailsModel.PrivateEmails [(int)btn.Tag];

                    // Configure alert
                    var alert = new NSAlert () {
                        AlertStyle = NSAlertStyle.Informational,
                        InformativeText = $"Are you sure you want to delete {email.Email}? This operation cannot be undone.",
                        MessageText = $"Delete {email.Email}?",
                    };
                    alert.AddButton ("Cancel");
                    alert.AddButton ("Delete");

                    nint result = alert.RunModal ();

                    if (result == 1001) 
                    {
                        // Remove the given row from the dataset
                        DataSource.PrivateEmailsModel.DeleteEmail (email);
                    };
                };

                var button1 = new NSButton (new CGRect (16, 0, 16, 16));
                button1.SetButtonType (NSButtonType.MomentaryPushIn);
                button1.Image = NSImage.ImageNamed ("iconStatusModerate");
                button1.Tag = row;
                button1.BezelStyle = NSBezelStyle.Recessed;
                button1.Bordered = false;
                button1.ToolTip = "Edit notes";
                button1.Activated += (sender, e) => 
                {
                    NSButton btn = sender as NSButton;
                    PrivateEmailInfo email = DataSource.PrivateEmailsModel.PrivateEmails [(int)btn.Tag];

                    string newNotes = IVPNAlert.ShowInputBox ($"Notes for {email.Email}", "", email.Notes);
                    if (newNotes == null || (email.Notes!=null && newNotes.Equals (email.Notes)))
                        return;

                    DataSource.PrivateEmailsModel.UpdateNotes (email, newNotes);
                };

                var button2 = new NSButton (new CGRect (32, 0, 16, 16));
                button2.SetButtonType (NSButtonType.MomentaryPushIn);
                button2.Image = NSImage.ImageNamed ("iconStatusGood");
                button2.Tag = row;
                button2.BezelStyle = NSBezelStyle.Recessed;
                button2.Bordered = false;
                button2.ToolTip = "Copy to clipboard";
                button2.Activated += (sender, e) => 
                {
                    NSButton btn = sender as NSButton;
                    PrivateEmailInfo email = DataSource.PrivateEmailsModel.PrivateEmails [(int)btn.Tag];

                    NSPasteboard pb = NSPasteboard.GeneralPasteboard;
                    pb.DeclareTypes (new string [] { NSPasteboard.NSStringType }, null);
                    pb.SetStringForType (email.Email, NSPasteboard.NSStringType);
                };

                // Add to view
                view.AddSubview (button);
                view.AddSubview (button1);
                view.AddSubview (button2);
                break;
                */
            }

            return view;
        }
        #endregion
    }
    /// <summary>
    /// Private email manage window controller.
    /// </summary>
    public partial class PrivateEmailManageWindowController : NSWindowController
    {
        PrivateEmailsManagerViewModel __Model;
        private bool __IsUpdateInProgress;

        public PrivateEmailManageWindowController (IntPtr handle) : base (handle)
        {
        }

        [Export ("initWithCoder:")]
        public PrivateEmailManageWindowController (NSCoder coder) : base (coder)
        {
        }

        public PrivateEmailManageWindowController (PrivateEmailsManagerViewModel model) : base ("PrivateEmailManageWindow")
        {
            __Model = model;
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

            // Stylyze buttons
            CustomButtonStyles.ApplyStyleGreyButtonV2(GuiBtnEdit, LocalizedStrings.Instance.LocalizedString("Button_PrivateEmail_Notes"));
            CustomButtonStyles.ApplyStyleGreyButtonV2(GuiBtnCopy, LocalizedStrings.Instance.LocalizedString("Button_PrivateEmail_Copy"));
            CustomButtonStyles.ApplyStyleGreyButtonV2(GuiBtnAdd, LocalizedStrings.Instance.LocalizedString("Button_PrivateEmail_Create"));
            CustomButtonStyles.ApplyStyleGreyButtonV2(GuiBtnDelete, LocalizedStrings.Instance.LocalizedString("Button_PrivateEmail_Delete"));

            GuiProgressSpiner.StopAnimation (this);
            GuiProgressSpiner.Hidden = true;
            GuiBtnRefresh.Hidden = false;

            // Model event handlers
            __Model.OnError += (errorText, errorDescription) => 
            {
                if (Window.IsVisible)
                {
                    if (string.IsNullOrEmpty(errorDescription))
                        IVPNAlert.Show(errorText);
                    else
                        IVPNAlert.Show(errorText, errorDescription);
                }
            };

            __Model.OnWillExecute += (sender) => 
            {
                InvokeOnMainThread (() => {
                    __IsUpdateInProgress = true;
                    SetEnableButtons ();

                    GuiInfoLabel.StringValue = LocalizedStrings.Instance.LocalizedString ("Label_PrivateEmail_UdpatingProgress");
                    GuiProgressSpiner.Hidden = false;
                    GuiBtnRefresh.Hidden = true;
                    GuiProgressSpiner.StartAnimation (this);

                    EnableView.Disable (this.GuiTableScrollView);
                });
            };

            __Model.OnDidExecute += (sender) => 
            {
                InvokeOnMainThread (() => {
                    __IsUpdateInProgress = false;
                    SetEnableButtons ();

                    GuiProgressSpiner.Hidden = true;
                    GuiBtnRefresh.Hidden = false;
                    GuiInfoLabel.StringValue = LocalizedStrings.Instance.LocalizedString ("Label_PrivateEmail_Title");
                    EnableView.Enable (this.GuiTableScrollView);
                });
            };

            __Model.PropertyChanged += __Model_PropertyChanged;
            GuiTableEmails.DoubleClick += (sender, e) => OnEdit(null);

            UpdateGui ();
        }

        public override void ShowWindow (NSObject sender)
        {
            base.ShowWindow (sender);

            RefreshList();
        }

        public new PrivateEmailManageWindow Window {
            get { return (PrivateEmailManageWindow)base.Window; }
        }

        private void __Model_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals (nameof (PrivateEmailsManagerViewModel.PrivateEmails)))
                UpdateGui ();
        }

        private void RefreshList()
        {
            __Model.ReloadEmailsInfo();
        }

        private void UpdateGui()
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread (UpdateGui);
                return;
            }

            nint oldSelectedRow = GuiTableEmails.SelectedRow;
            if (oldSelectedRow < 0)
                oldSelectedRow = 0;
            
            EmailTableDelegate oldDelegate = GuiTableEmails.Delegate as EmailTableDelegate;
            if (oldDelegate != null)
                oldDelegate.OnSelectionChanged -= SetEnableButtons;

            EmailsTableDataSource dataSource = new EmailsTableDataSource (__Model);
            GuiTableEmails.DataSource = dataSource;
            EmailTableDelegate tableDelegate = new EmailTableDelegate (dataSource);
            GuiTableEmails.Delegate = tableDelegate;

            GuiLabelEmailsCount.StringValue = string.Format (LocalizedStrings.Instance.LocalizedString ("Label_PrivateEmail_AmountOfEmailsTitle_PARAMETRIZED"), __Model.PrivateEmails.Count);

            //restore selection
            if (__Model.PrivateEmails.Count>0 && oldSelectedRow>=0)
            {
                if (oldSelectedRow < __Model.PrivateEmails.Count)
                    GuiTableEmails.SelectRow (oldSelectedRow, false);
                else
                    GuiTableEmails.SelectRow (__Model.PrivateEmails.Count-1, false);
            }

            tableDelegate.OnSelectionChanged += SetEnableButtons;
            SetEnableButtons ();
        }

        void SetEnableButtons ()
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread (() =>SetEnableButtons ());
                return;
            }

            bool isEnabled = GuiTableEmails.SelectedRow >= 0;
            if (__IsUpdateInProgress) 
            {
                isEnabled = false;
                GuiBtnAdd.Enabled = false;
            } 
            else
                GuiBtnAdd.Enabled = true;
            
            GuiBtnEdit.Enabled = isEnabled;
            GuiBtnCopy.Enabled = isEnabled;
            GuiBtnDelete.Enabled = isEnabled;
        }

        partial void OnCreate (Foundation.NSObject sender)
        {
            PrivateEmailGeneratedWindowController.Generate(__Model);
        }

        partial void OnDelete (Foundation.NSObject sender)
        {
            int row = (int)GuiTableEmails.SelectedRow;
            if (row < 0)
                return;

            PrivateEmailInfo email = __Model.PrivateEmails [row];

            // Configure alert
            var alert = new NSAlert () 
            {
                AlertStyle = NSAlertStyle.Informational,
                InformativeText = string.Format (LocalizedStrings.Instance.LocalizedString ("Label_PrivateEmail_DeleteEmailQuestion_Informative_PARAMETRIZED"), email.Email), 
                MessageText = string.Format (LocalizedStrings.Instance.LocalizedString ("Label_PrivateEmail_DeleteEmailQuestion_Header_PARAMETRIZED"), email.Email),
            };
            alert.AddButton (LocalizedStrings.Instance.LocalizedString ("Button_Cancel"));
            alert.AddButton (LocalizedStrings.Instance.LocalizedString ("Button_PrivateEmail_Delete"));

            nint result = alert.RunModal ();

            if (result == (int)NSAlertButtonReturn.Second) 
            {
                // Remove the given row from the dataset
                __Model.DeleteEmail (email);
            };
        }

        partial void OnCopy (Foundation.NSObject sender)
        {
            int row = (int)GuiTableEmails.SelectedRow;
            if (row < 0)
                return;

            PrivateEmailInfo email = __Model.PrivateEmails [row];

            NSPasteboard pb = NSPasteboard.GeneralPasteboard;
            pb.DeclareTypes (new string [] { NSPasteboard.NSStringType }, null);
            pb.SetStringForType (email.Email, NSPasteboard.NSStringType);
        }

        partial void OnEdit (Foundation.NSObject sender)
        {
            int row = (int)GuiTableEmails.SelectedRow;
            if (row < 0)
                return;

            PrivateEmailInfo email = __Model.PrivateEmails [row];

            string newNotes = IVPNAlert.ShowInputBox (string.Format (LocalizedStrings.Instance.LocalizedString ("Label_PrivateEmail_NotesDialog_PARAMETRIZED"), email.Email),
                                                      "",
                                                      email.Notes, 
                                                      true);
            if (newNotes == null || (email.Notes != null && newNotes.Equals (email.Notes)))
                return;

            __Model.UpdateNotes (email, newNotes);
        }

        partial void OnRefresh(Foundation.NSObject sender)
        {
            RefreshList();
        }

        partial void OnClose (Foundation.NSObject sender)
        {
            Close ();
        }
    }
}
