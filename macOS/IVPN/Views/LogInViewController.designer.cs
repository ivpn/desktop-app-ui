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
	[Register ("LogInViewController")]
	partial class LogInViewController
	{
		[Outlet]
		IVPN.CustomButton GuiButtonLogIn { get; set; }

		[Outlet]
		IVPN.CustomButton GuiButtonStartFreeTrial { get; set; }

		[Outlet]
		IVPN.PopoverContentView GuiPopoverContent_CredentialsError { get; set; }

		[Outlet]
		IVPN.PopoverContentView GuiPopoverContent_EnterUserrname { get; set; }

		[Outlet]
		IVPN.PopoverContentView GuiPopoverContent_InvalidUserrname { get; set; }

		[Outlet]
		AppKit.NSProgressIndicator GuiProgressIndicator { get; set; }

		[Outlet]
		AppKit.NSTextField GuiTextAccountIdDescription { get; set; }

		[Outlet]
		IVPN.CustomTextField GuiTextViewUser { get; set; }

		[Action ("OnForgotPasswordPressed:")]
		partial void OnForgotPasswordPressed (Foundation.NSObject sender);

		[Action ("OnLogInPressed:")]
		partial void OnLogInPressed (Foundation.NSObject sender);

		[Action ("OnPasswordRecoveryPressed:")]
		partial void OnPasswordRecoveryPressed (Foundation.NSObject sender);

		[Action ("OnSkipAndContinuePressed:")]
		partial void OnSkipAndContinuePressed (Foundation.NSObject sender);

		[Action ("OnStartFreeTrialPressed:")]
		partial void OnStartFreeTrialPressed (Foundation.NSObject sender);

		[Action ("OnTryAgainLoginPressed:")]
		partial void OnTryAgainLoginPressed (Foundation.NSObject sender);

		[Action ("TestClick:")]
		partial void TestClick (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (GuiButtonLogIn != null) {
				GuiButtonLogIn.Dispose ();
				GuiButtonLogIn = null;
			}

			if (GuiButtonStartFreeTrial != null) {
				GuiButtonStartFreeTrial.Dispose ();
				GuiButtonStartFreeTrial = null;
			}

			if (GuiPopoverContent_CredentialsError != null) {
				GuiPopoverContent_CredentialsError.Dispose ();
				GuiPopoverContent_CredentialsError = null;
			}

			if (GuiPopoverContent_EnterUserrname != null) {
				GuiPopoverContent_EnterUserrname.Dispose ();
				GuiPopoverContent_EnterUserrname = null;
			}

			if (GuiPopoverContent_InvalidUserrname != null) {
				GuiPopoverContent_InvalidUserrname.Dispose ();
				GuiPopoverContent_InvalidUserrname = null;
			}

			if (GuiProgressIndicator != null) {
				GuiProgressIndicator.Dispose ();
				GuiProgressIndicator = null;
			}

			if (GuiTextViewUser != null) {
				GuiTextViewUser.Dispose ();
				GuiTextViewUser = null;
			}

			if (GuiTextAccountIdDescription != null) {
				GuiTextAccountIdDescription.Dispose ();
				GuiTextAccountIdDescription = null;
			}
		}
	}
}
