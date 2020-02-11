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
    [Register ("FirewallNotificationWindowController")]
    partial class FirewallNotificationWindowController
    {
        [Outlet]
        AppKit.NSButton CloseButton { get; set; }

        [Outlet]
        AppKit.NSTextField FirewallLabel { get; set; }

        [Outlet]
        AppKit.NSTextField FirewallStatusLabel { get; set; }

        [Outlet]
        AppKit.NSView GuiPausePopoverView { get; set; }

        [Outlet]
        AppKit.NSImageView ImageLogo { get; set; }

        [Outlet]
        AppKit.NSButton PauseAddButton { get; set; }

        [Outlet]
        AppKit.NSTextField PauseTimeLeftLabel { get; set; }

        [Outlet]
        IVPN.CustomButton ResumeBtn { get; set; }

        [Outlet]
        AppKit.NSTextField ResumeInLabel { get; set; }

        [Outlet]
        AppKit.NSTextField VPNLabel { get; set; }

        [Outlet]
        AppKit.NSTextField VPNStatusLabel { get; set; }

        [Action ("OnButtonPauseAddPressed:")]
        partial void OnButtonPauseAddPressed (Foundation.NSObject sender);

        [Action ("OnCloseButton:")]
        partial void OnCloseButton (Foundation.NSObject sender);

        [Action ("OnResumeButtonPressed:")]
        partial void OnResumeButtonPressed (Foundation.NSObject sender);

        [Action ("PauseMenuItemButtonPressed:")]
        partial void PauseMenuItemButtonPressed (Foundation.NSObject sender);
        
        void ReleaseDesignerOutlets ()
        {
            if (CloseButton != null) {
                CloseButton.Dispose ();
                CloseButton = null;
            }

            if (FirewallLabel != null) {
                FirewallLabel.Dispose ();
                FirewallLabel = null;
            }

            if (FirewallStatusLabel != null) {
                FirewallStatusLabel.Dispose ();
                FirewallStatusLabel = null;
            }

            if (GuiPausePopoverView != null) {
                GuiPausePopoverView.Dispose ();
                GuiPausePopoverView = null;
            }

            if (ImageLogo != null) {
                ImageLogo.Dispose ();
                ImageLogo = null;
            }

            if (PauseAddButton != null) {
                PauseAddButton.Dispose ();
                PauseAddButton = null;
            }

            if (PauseTimeLeftLabel != null) {
                PauseTimeLeftLabel.Dispose ();
                PauseTimeLeftLabel = null;
            }

            if (ResumeInLabel != null) {
                ResumeInLabel.Dispose ();
                ResumeInLabel = null;
            }

            if (VPNLabel != null) {
                VPNLabel.Dispose ();
                VPNLabel = null;
            }

            if (VPNStatusLabel != null) {
                VPNStatusLabel.Dispose ();
                VPNStatusLabel = null;
            }

            if (ResumeBtn != null) {
                ResumeBtn.Dispose ();
                ResumeBtn = null;
            }
        }
    }
}
