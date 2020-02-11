// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace IVPN
{
    [Register ("PrivateEmailManageWindowController")]
    partial class PrivateEmailManageWindowController
    {
        [Outlet]
        IVPN.CustomButton GuiBtnAdd { get; set; }

        [Outlet]
        IVPN.CustomButton GuiBtnCopy { get; set; }

        [Outlet]
        IVPN.CustomButton GuiBtnDelete { get; set; }

        [Outlet]
        IVPN.CustomButton GuiBtnEdit { get; set; }

        [Outlet]
        AppKit.NSButton GuiBtnRefresh { get; set; }

        [Outlet]
        AppKit.NSTextField GuiInfoLabel { get; set; }

        [Outlet]
        AppKit.NSTextField GuiLabelEmailsCount { get; set; }

        [Outlet]
        AppKit.NSProgressIndicator GuiProgressSpiner { get; set; }

        [Outlet]
        AppKit.NSTableView GuiTableEmails { get; set; }

        [Outlet]
        AppKit.NSScrollView GuiTableScrollView { get; set; }

        [Action ("OnClose:")]
        partial void OnClose (Foundation.NSObject sender);

        [Action ("OnCopy:")]
        partial void OnCopy (Foundation.NSObject sender);

        [Action ("OnCreate:")]
        partial void OnCreate (Foundation.NSObject sender);

        [Action ("OnDelete:")]
        partial void OnDelete (Foundation.NSObject sender);

        [Action ("OnEdit:")]
        partial void OnEdit (Foundation.NSObject sender);

        [Action ("OnRefresh:")]
        partial void OnRefresh (Foundation.NSObject sender);
        
        void ReleaseDesignerOutlets ()
        {
            if (GuiBtnAdd != null) {
                GuiBtnAdd.Dispose ();
                GuiBtnAdd = null;
            }

            if (GuiBtnCopy != null) {
                GuiBtnCopy.Dispose ();
                GuiBtnCopy = null;
            }

            if (GuiBtnDelete != null) {
                GuiBtnDelete.Dispose ();
                GuiBtnDelete = null;
            }

            if (GuiBtnEdit != null) {
                GuiBtnEdit.Dispose ();
                GuiBtnEdit = null;
            }

            if (GuiBtnRefresh != null) {
                GuiBtnRefresh.Dispose ();
                GuiBtnRefresh = null;
            }

            if (GuiInfoLabel != null) {
                GuiInfoLabel.Dispose ();
                GuiInfoLabel = null;
            }

            if (GuiLabelEmailsCount != null) {
                GuiLabelEmailsCount.Dispose ();
                GuiLabelEmailsCount = null;
            }

            if (GuiProgressSpiner != null) {
                GuiProgressSpiner.Dispose ();
                GuiProgressSpiner = null;
            }

            if (GuiTableEmails != null) {
                GuiTableEmails.Dispose ();
                GuiTableEmails = null;
            }

            if (GuiTableScrollView != null) {
                GuiTableScrollView.Dispose ();
                GuiTableScrollView = null;
            }
        }
    }
}
