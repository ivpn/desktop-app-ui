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
	[Register ("SessionLimitViewController")]
	partial class SessionLimitViewController
	{
		[Outlet]
		IVPN.CustomButton GuiButtonBack { get; set; }

		[Outlet]
		IVPN.CustomButton UIButtonLogOutAll { get; set; }

		[Outlet]
		AppKit.NSButton UIButtonTryAgain { get; set; }

		[Outlet]
		IVPN.CustomButton UIButtonUpgrade { get; set; }

		[Action ("OnButtonLogoutAllDevices:")]
		partial void OnButtonLogoutAllDevices (Foundation.NSObject sender);

		[Action ("OnButtonTryAgain:")]
		partial void OnButtonTryAgain (Foundation.NSObject sender);

		[Action ("OnButtonUpgradePlan:")]
		partial void OnButtonUpgradePlan (Foundation.NSObject sender);

		[Action ("OnGoBackButtonPressed:")]
		partial void OnGoBackButtonPressed (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (UIButtonLogOutAll != null) {
				UIButtonLogOutAll.Dispose ();
				UIButtonLogOutAll = null;
			}

			if (UIButtonTryAgain != null) {
				UIButtonTryAgain.Dispose ();
				UIButtonTryAgain = null;
			}

			if (UIButtonUpgrade != null) {
				UIButtonUpgrade.Dispose ();
				UIButtonUpgrade = null;
			}

			if (GuiButtonBack != null) {
				GuiButtonBack.Dispose ();
				GuiButtonBack = null;
			}
		}
	}
}
