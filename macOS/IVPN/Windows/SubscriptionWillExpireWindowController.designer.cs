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
    [Register ("SubscriptionWillExpireWindowController")]
    partial class SubscriptionWillExpireWindowController
    {
        [Outlet]
        IVPN.CustomButton GuiButtonCancel { get; set; }

        [Outlet]
        IVPN.CustomButton GuiButtonGoToAccount { get; set; }

        [Outlet]
        AppKit.NSTextField GuiLabelDescriptionText { get; set; }

        [Outlet]
        AppKit.NSTextField GuiLabelTitleText { get; set; }

        [Outlet]
        AppKit.NSProgressIndicator GuiProgressIndicator { get; set; }

        [Action ("GuiButtonGoToAccountPressed:")]
        partial void GuiButtonGoToAccountPressed (Foundation.NSObject sender);

        [Action ("OnGuiButtonCancelPressed:")]
        partial void OnGuiButtonCancelPressed (Foundation.NSObject sender);
        
        void ReleaseDesignerOutlets ()
        {
            if (GuiButtonCancel != null) {
                GuiButtonCancel.Dispose ();
                GuiButtonCancel = null;
            }

            if (GuiButtonGoToAccount != null) {
                GuiButtonGoToAccount.Dispose ();
                GuiButtonGoToAccount = null;
            }

            if (GuiLabelDescriptionText != null) {
                GuiLabelDescriptionText.Dispose ();
                GuiLabelDescriptionText = null;
            }

            if (GuiLabelTitleText != null) {
                GuiLabelTitleText.Dispose ();
                GuiLabelTitleText = null;
            }

            if (GuiProgressIndicator != null) {
                GuiProgressIndicator.Dispose ();
                GuiProgressIndicator = null;
            }
        }
    }
}
