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
	[Register ("ServerSelectionViewController")]
	partial class ServerSelectionViewController
	{
		[Outlet]
		IVPN.CustomButton GuiBtnMultiHop { get; set; }

		[Outlet]
		IVPN.CustomButton GuiBtnSingeHop { get; set; }

		[Outlet]
		AppKit.NSTextField GuiEntryServerAutomaticText { get; set; }

		[Outlet]
		AppKit.NSTextField GuiEntryServerDescription { get; set; }

		[Outlet]
		AppKit.NSImageView GuiEntryServerImage { get; set; }

		[Outlet]
		AppKit.NSTextField GuiEntryServerName { get; set; }

		[Outlet]
		AppKit.NSImageView GuiEntryServerPingStatusImage { get; set; }

		[Outlet]
		AppKit.NSBox GuiEntryServerView { get; set; }

		[Outlet]
		AppKit.NSTextField GuiExitServerDescription { get; set; }

		[Outlet]
		AppKit.NSImageView GuiExitServerImage { get; set; }

		[Outlet]
		AppKit.NSTextField GuiExitServerName { get; set; }

		[Outlet]
		AppKit.NSImageView GuiExitServerPingStatusImage { get; set; }

		[Outlet]
		AppKit.NSBox GuiExitServerView { get; set; }

		[Outlet]
		AppKit.NSTextField GuiLabelServerChangePopoverText { get; set; }

		[Outlet]
		AppKit.NSTextField GuiLabelServerChangePopoverTitle { get; set; }

		[Outlet]
		AppKit.NSView GuiUnbaleToChangeWhenConnectedView { get; set; }

		[Action ("OnMultiHopPressed:")]
		partial void OnMultiHopPressed (Foundation.NSObject sender);

		[Action ("OnSingleHopPressed:")]
		partial void OnSingleHopPressed (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (GuiBtnMultiHop != null) {
				GuiBtnMultiHop.Dispose ();
				GuiBtnMultiHop = null;
			}

			if (GuiBtnSingeHop != null) {
				GuiBtnSingeHop.Dispose ();
				GuiBtnSingeHop = null;
			}

			if (GuiEntryServerAutomaticText != null) {
				GuiEntryServerAutomaticText.Dispose ();
				GuiEntryServerAutomaticText = null;
			}

			if (GuiEntryServerDescription != null) {
				GuiEntryServerDescription.Dispose ();
				GuiEntryServerDescription = null;
			}

			if (GuiEntryServerImage != null) {
				GuiEntryServerImage.Dispose ();
				GuiEntryServerImage = null;
			}

			if (GuiEntryServerName != null) {
				GuiEntryServerName.Dispose ();
				GuiEntryServerName = null;
			}

			if (GuiEntryServerPingStatusImage != null) {
				GuiEntryServerPingStatusImage.Dispose ();
				GuiEntryServerPingStatusImage = null;
			}

			if (GuiEntryServerView != null) {
				GuiEntryServerView.Dispose ();
				GuiEntryServerView = null;
			}

			if (GuiExitServerDescription != null) {
				GuiExitServerDescription.Dispose ();
				GuiExitServerDescription = null;
			}

			if (GuiExitServerImage != null) {
				GuiExitServerImage.Dispose ();
				GuiExitServerImage = null;
			}

			if (GuiExitServerName != null) {
				GuiExitServerName.Dispose ();
				GuiExitServerName = null;
			}

			if (GuiExitServerPingStatusImage != null) {
				GuiExitServerPingStatusImage.Dispose ();
				GuiExitServerPingStatusImage = null;
			}

			if (GuiExitServerView != null) {
				GuiExitServerView.Dispose ();
				GuiExitServerView = null;
			}

			if (GuiLabelServerChangePopoverText != null) {
				GuiLabelServerChangePopoverText.Dispose ();
				GuiLabelServerChangePopoverText = null;
			}

			if (GuiLabelServerChangePopoverTitle != null) {
				GuiLabelServerChangePopoverTitle.Dispose ();
				GuiLabelServerChangePopoverTitle = null;
			}

			if (GuiUnbaleToChangeWhenConnectedView != null) {
				GuiUnbaleToChangeWhenConnectedView.Dispose ();
				GuiUnbaleToChangeWhenConnectedView = null;
			}
		}
	}
}
