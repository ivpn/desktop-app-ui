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
	[Register ("MainPageViewController")]
	partial class MainPageViewController
	{
		[Outlet]
		AppKit.NSView GuiAntitrackerInfoView { get; set; }

		[Outlet]
		AppKit.NSTextField GuiAntiTrackerLabel { get; set; }

		[Outlet]
		IVPN.CustomSwitchControl GuiAntiTrackerSwitchControl { get; set; }

		[Outlet]
		AppKit.NSView GuiAntitrackerWhenConnectedInfo { get; set; }

		[Outlet]
		IVPN.PageView GuiButtonsPanel { get; set; }

		[Outlet]
		IVPN.PageView GuiConnectButtonView { get; set; }

		[Outlet]
		AppKit.NSView GuiFirewallInfoView { get; set; }

		[Outlet]
		AppKit.NSTextField GuiFirewallLabel { get; set; }

		[Outlet]
		AppKit.NSView GuiFirewallOffInfoView { get; set; }

		[Outlet]
		IVPN.CustomSwitchControl GuiFirewallSwitchControl { get; set; }

		[Outlet]
		IVPN.PageView GuiServerSelectionView { get; set; }

		[Action ("OnInfoButtonAntitracker:")]
		partial void OnInfoButtonAntitracker (Foundation.NSObject sender);

		[Action ("OnInfoButtonFirewall:")]
		partial void OnInfoButtonFirewall (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (GuiAntitrackerInfoView != null) {
				GuiAntitrackerInfoView.Dispose ();
				GuiAntitrackerInfoView = null;
			}

			if (GuiAntitrackerWhenConnectedInfo != null) {
				GuiAntitrackerWhenConnectedInfo.Dispose ();
				GuiAntitrackerWhenConnectedInfo = null;
			}

			if (GuiAntiTrackerLabel != null) {
				GuiAntiTrackerLabel.Dispose ();
				GuiAntiTrackerLabel = null;
			}

			if (GuiAntiTrackerSwitchControl != null) {
				GuiAntiTrackerSwitchControl.Dispose ();
				GuiAntiTrackerSwitchControl = null;
			}

			if (GuiButtonsPanel != null) {
				GuiButtonsPanel.Dispose ();
				GuiButtonsPanel = null;
			}

			if (GuiConnectButtonView != null) {
				GuiConnectButtonView.Dispose ();
				GuiConnectButtonView = null;
			}

			if (GuiFirewallInfoView != null) {
				GuiFirewallInfoView.Dispose ();
				GuiFirewallInfoView = null;
			}

			if (GuiFirewallLabel != null) {
				GuiFirewallLabel.Dispose ();
				GuiFirewallLabel = null;
			}

			if (GuiFirewallOffInfoView != null) {
				GuiFirewallOffInfoView.Dispose ();
				GuiFirewallOffInfoView = null;
			}

			if (GuiFirewallSwitchControl != null) {
				GuiFirewallSwitchControl.Dispose ();
				GuiFirewallSwitchControl = null;
			}

			if (GuiServerSelectionView != null) {
				GuiServerSelectionView.Dispose ();
				GuiServerSelectionView = null;
			}
		}
	}
}
