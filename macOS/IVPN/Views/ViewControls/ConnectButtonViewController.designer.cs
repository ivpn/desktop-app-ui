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
	[Register ("ConnectButtonViewController")]
	partial class ConnectButtonViewController
	{
		[Outlet]
		AppKit.NSImageView GuiConnectButtonImage { get; set; }

		[Outlet]
		AppKit.NSTextField GuiConnectButtonText { get; set; }

		[Outlet]
		AppKit.NSView GuiGeoLookupCityView { get; set; }

		[Outlet]
		AppKit.NSView GuiGeoLookupDurationView { get; set; }

		[Outlet]
		AppKit.NSView GuiGeoLookupErrorView { get; set; }

		[Outlet]
		AppKit.NSView GuiGeoLookupPublicIpView { get; set; }

		[Outlet]
		AppKit.NSView GuiGeoLookupUpdateView { get; set; }

		[Outlet]
		AppKit.NSView GuiGeoLookupView { get; set; }

		[Outlet]
		AppKit.NSButton GuiInformationButton { get; set; }

		[Outlet]
		AppKit.NSTextField GuiLabelToDoDescription { get; set; }

		[Outlet]
		AppKit.NSPopUpButton GuiNetworkActionPopUpBtn { get; set; }

		[Outlet]
		IVPN.CustomButton GuiNotificationButtonBottom { get; set; }

		[Outlet]
		AppKit.NSImageView GuiPauseButton { get; set; }

		[Outlet]
		IVPN.CustomButton GuiPauseDlgCancelBtn { get; set; }

		[Outlet]
		IVPN.CustomTextField GuiPauseDlgHoursTextBlock { get; set; }

		[Outlet]
		IVPN.CustomTextField GuiPauseDlgMinutesTextBlock { get; set; }

		[Outlet]
		IVPN.CustomButton GuiPauseDlgOkBtn { get; set; }

		[Outlet]
		AppKit.NSTextField GuiPauseLeftTimeText { get; set; }

		[Outlet]
		AppKit.NSView GuiPausePopoverView { get; set; }

		[Outlet]
		AppKit.NSTextField GuiPopoverConstLabelClientIP { get; set; }

		[Outlet]
		AppKit.NSTextField GuiPopoverConstLabelDuration { get; set; }

		[Outlet]
		AppKit.NSTextField GuiPopoverConstLabelServerIP { get; set; }

		[Outlet]
		AppKit.NSTextField GuiPopoverLabelClientIP { get; set; }

		[Outlet]
		AppKit.NSTextField GuiPopoverLabelDuration { get; set; }

		[Outlet]
		AppKit.NSTextField GuiPopoverLabelServerIP { get; set; }

		[Outlet]
		AppKit.NSView GuiPopoverView { get; set; }

		[Outlet]
		AppKit.NSWindow GuiSetPauseIntervalWindow { get; set; }

		[Outlet]
		AppKit.NSButton GuiWiFiButton { get; set; }

		[Action ("GuiInformationButtonPressed:")]
		partial void GuiInformationButtonPressed (Foundation.NSObject sender);

		[Action ("GuiNotificationButtonBottomPressed:")]
		partial void GuiNotificationButtonBottomPressed (Foundation.NSObject sender);

		[Action ("GuiPauseLeftTimeTextClick:")]
		partial void GuiPauseLeftTimeTextClick (Foundation.NSObject sender);

		[Action ("GuiSetPauseIntervalWindowHoursEnter:")]
		partial void GuiSetPauseIntervalWindowHoursEnter (Foundation.NSObject sender);

		[Action ("OnGuiPauseDlgCancelBtnPressed:")]
		partial void OnGuiPauseDlgCancelBtnPressed (Foundation.NSObject sender);

		[Action ("OnGuiPauseDlgHoursTextBlockEnter:")]
		partial void OnGuiPauseDlgHoursTextBlockEnter (Foundation.NSObject sender);

		[Action ("OnGuiPauseDlgMinutesTextBlockEnter:")]
		partial void OnGuiPauseDlgMinutesTextBlockEnter (Foundation.NSObject sender);

		[Action ("OnGuiPauseDlgOkBtnPressed:")]
		partial void OnGuiPauseDlgOkBtnPressed (Foundation.NSObject sender);

		[Action ("PauseMenuItemButtonPressed:")]
		partial void PauseMenuItemButtonPressed (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (GuiConnectButtonImage != null) {
				GuiConnectButtonImage.Dispose ();
				GuiConnectButtonImage = null;
			}

			if (GuiConnectButtonText != null) {
				GuiConnectButtonText.Dispose ();
				GuiConnectButtonText = null;
			}

			if (GuiGeoLookupCityView != null) {
				GuiGeoLookupCityView.Dispose ();
				GuiGeoLookupCityView = null;
			}

			if (GuiGeoLookupDurationView != null) {
				GuiGeoLookupDurationView.Dispose ();
				GuiGeoLookupDurationView = null;
			}

			if (GuiGeoLookupErrorView != null) {
				GuiGeoLookupErrorView.Dispose ();
				GuiGeoLookupErrorView = null;
			}

			if (GuiGeoLookupPublicIpView != null) {
				GuiGeoLookupPublicIpView.Dispose ();
				GuiGeoLookupPublicIpView = null;
			}

			if (GuiGeoLookupUpdateView != null) {
				GuiGeoLookupUpdateView.Dispose ();
				GuiGeoLookupUpdateView = null;
			}

			if (GuiGeoLookupView != null) {
				GuiGeoLookupView.Dispose ();
				GuiGeoLookupView = null;
			}

			if (GuiInformationButton != null) {
				GuiInformationButton.Dispose ();
				GuiInformationButton = null;
			}

			if (GuiLabelToDoDescription != null) {
				GuiLabelToDoDescription.Dispose ();
				GuiLabelToDoDescription = null;
			}

			if (GuiNetworkActionPopUpBtn != null) {
				GuiNetworkActionPopUpBtn.Dispose ();
				GuiNetworkActionPopUpBtn = null;
			}

			if (GuiNotificationButtonBottom != null) {
				GuiNotificationButtonBottom.Dispose ();
				GuiNotificationButtonBottom = null;
			}

			if (GuiPauseButton != null) {
				GuiPauseButton.Dispose ();
				GuiPauseButton = null;
			}

			if (GuiPauseDlgCancelBtn != null) {
				GuiPauseDlgCancelBtn.Dispose ();
				GuiPauseDlgCancelBtn = null;
			}

			if (GuiPauseDlgHoursTextBlock != null) {
				GuiPauseDlgHoursTextBlock.Dispose ();
				GuiPauseDlgHoursTextBlock = null;
			}

			if (GuiPauseDlgMinutesTextBlock != null) {
				GuiPauseDlgMinutesTextBlock.Dispose ();
				GuiPauseDlgMinutesTextBlock = null;
			}

			if (GuiPauseDlgOkBtn != null) {
				GuiPauseDlgOkBtn.Dispose ();
				GuiPauseDlgOkBtn = null;
			}

			if (GuiPauseLeftTimeText != null) {
				GuiPauseLeftTimeText.Dispose ();
				GuiPauseLeftTimeText = null;
			}

			if (GuiPausePopoverView != null) {
				GuiPausePopoverView.Dispose ();
				GuiPausePopoverView = null;
			}

			if (GuiPopoverConstLabelClientIP != null) {
				GuiPopoverConstLabelClientIP.Dispose ();
				GuiPopoverConstLabelClientIP = null;
			}

			if (GuiPopoverConstLabelDuration != null) {
				GuiPopoverConstLabelDuration.Dispose ();
				GuiPopoverConstLabelDuration = null;
			}

			if (GuiPopoverConstLabelServerIP != null) {
				GuiPopoverConstLabelServerIP.Dispose ();
				GuiPopoverConstLabelServerIP = null;
			}

			if (GuiPopoverLabelClientIP != null) {
				GuiPopoverLabelClientIP.Dispose ();
				GuiPopoverLabelClientIP = null;
			}

			if (GuiPopoverLabelDuration != null) {
				GuiPopoverLabelDuration.Dispose ();
				GuiPopoverLabelDuration = null;
			}

			if (GuiPopoverLabelServerIP != null) {
				GuiPopoverLabelServerIP.Dispose ();
				GuiPopoverLabelServerIP = null;
			}

			if (GuiPopoverView != null) {
				GuiPopoverView.Dispose ();
				GuiPopoverView = null;
			}

			if (GuiSetPauseIntervalWindow != null) {
				GuiSetPauseIntervalWindow.Dispose ();
				GuiSetPauseIntervalWindow = null;
			}

			if (GuiWiFiButton != null) {
				GuiWiFiButton.Dispose ();
				GuiWiFiButton = null;
			}
		}
	}
}
