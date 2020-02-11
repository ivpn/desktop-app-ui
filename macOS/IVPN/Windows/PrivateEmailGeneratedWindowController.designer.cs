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
	[Register ("PrivateEmailGeneratedWindowController")]
	partial class PrivateEmailGeneratedWindowController
	{
		[Outlet]
		IVPN.CustomButton GuiBtnCopy { get; set; }

		[Outlet]
		IVPN.CustomButton GuiBtnDelete { get; set; }

		[Outlet]
		IVPN.CustomButton GuiBtnOk { get; set; }

		[Outlet]
		AppKit.NSTextField GuiForwardToEmailField { get; set; }

		[Outlet]
		AppKit.NSTextField GuiGeneratedEmailField { get; set; }

		[Outlet]
		AppKit.NSView GuiInProgressView { get; set; }

		[Outlet]
		AppKit.NSView GuiMainView { get; set; }

		[Outlet]
		AppKit.NSTextView GuiNotesField { get; set; }

		[Outlet]
		AppKit.NSProgressIndicator GuiProgressIndicator { get; set; }

		[Action ("OnButtonCopy:")]
		partial void OnButtonCopy (Foundation.NSObject sender);

		[Action ("OnButtonDelete:")]
		partial void OnButtonDelete (Foundation.NSObject sender);

		[Action ("OnButtonOk:")]
		partial void OnButtonOk (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (GuiBtnCopy != null) {
				GuiBtnCopy.Dispose ();
				GuiBtnCopy = null;
			}

			if (GuiBtnDelete != null) {
				GuiBtnDelete.Dispose ();
				GuiBtnDelete = null;
			}

			if (GuiBtnOk != null) {
				GuiBtnOk.Dispose ();
				GuiBtnOk = null;
			}

			if (GuiForwardToEmailField != null) {
				GuiForwardToEmailField.Dispose ();
				GuiForwardToEmailField = null;
			}

			if (GuiGeneratedEmailField != null) {
				GuiGeneratedEmailField.Dispose ();
				GuiGeneratedEmailField = null;
			}

			if (GuiInProgressView != null) {
				GuiInProgressView.Dispose ();
				GuiInProgressView = null;
			}

			if (GuiMainView != null) {
				GuiMainView.Dispose ();
				GuiMainView = null;
			}

			if (GuiNotesField != null) {
				GuiNotesField.Dispose ();
				GuiNotesField = null;
			}

			if (GuiProgressIndicator != null) {
				GuiProgressIndicator.Dispose ();
				GuiProgressIndicator = null;
			}
		}
	}
}
