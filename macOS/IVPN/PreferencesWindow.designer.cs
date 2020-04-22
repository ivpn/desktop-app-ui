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
	[Register ("PreferencesWindowController")]
	partial class PreferencesWindowController
	{
		[Outlet]
		AppKit.NSView AntiTrackerSettings { get; set; }

		[Outlet]
		AppKit.NSView ConnectionSettingsView { get; set; }

		[Outlet]
		AppKit.NSView DnsSettings { get; set; }

		[Outlet]
		AppKit.NSView FirewallSettingsView { get; set; }

		[Outlet]
		AppKit.NSView GeneralSettingsView { get; set; }

		[Outlet]
		AppKit.NSButton GuiBtnFirewallTypeAlwaysOn { get; set; }

		[Outlet]
		AppKit.NSButton GuiBtnFirewallTypeOnDemand { get; set; }

		[Outlet]
		AppKit.NSButton GuiBtnLaunchAtLogin { get; set; }

		[Outlet]
		AppKit.NSButton GuiBtnProtocolTypeOpenVPN { get; set; }

		[Outlet]
		AppKit.NSButton GuiBtnProtocolTypeWireGuard { get; set; }

		[Outlet]
		AppKit.NSButton GuiBtnWireguardTooltip { get; set; }

		[Outlet]
		AppKit.NSButton GuiButtonOpenvpnTooltip { get; set; }

		[Outlet]
		AppKit.NSView GuiPanelOpenvpnTooltip { get; set; }

		[Outlet]
		AppKit.NSPanel GuiPanelWireguardConfigDetails { get; set; }

		[Outlet]
		AppKit.NSView GuiPanelWireguardTooltip { get; set; }

		[Outlet]
		AppKit.NSView GuiProgressViewWireguardKeysGeneration { get; set; }

		[Outlet]
		AppKit.NSProgressIndicator GuiProgressWireguardKeysGeneration { get; set; }

		[Outlet]
		AppKit.NSTabView GuiProtocolsConfigTabView { get; set; }

		[Outlet]
		AppKit.NSTabViewItem GuiTabConfigOpenVPN { get; set; }

		[Outlet]
		AppKit.NSTabViewItem GuiTabConfigWireGuard { get; set; }

		[Outlet]
		AppKit.NSView GuiViewWireguardConfig { get; set; }

		[Outlet]
		AppKit.NSProgressIndicator GuiWireguardConfigDetailsProgressIndicator { get; set; }

		[Outlet]
		AppKit.NSView GuiWireguardConfigDetailsView { get; set; }

		[Outlet]
		AppKit.NSView GuiWireguardConfigDetailsViewProgress { get; set; }

		[Outlet]
		AppKit.NSTextField GuiWireGuardDescription { get; set; }

		[Outlet]
		AppKit.NSPopUpButton NetworksDefaultActionBtn { get; set; }

		[Outlet]
		AppKit.NSView NetworksSettings { get; set; }

		[Outlet]
		AppKit.NSView NetworksView { get; set; }

		[Outlet]
		AppKit.NSView OpenVPNSettings { get; set; }

		[Outlet]
		AppKit.NSToolbarItem OpenVpnViewExtraParameters { get; set; }

		[Outlet]
		AppKit.NSView SettingsView { get; set; }

		[Outlet]
		AppKit.NSToolbar Toolbar { get; set; }

		[Action ("GenerateWireguardKeysBtn:")]
		partial void GenerateWireguardKeysBtn (Foundation.NSObject sender);

		[Action ("OnBtnCloseWireguardConfigDetails:")]
		partial void OnBtnCloseWireguardConfigDetails (Foundation.NSObject sender);

		[Action ("OnFirewallTypeChanged:")]
		partial void OnFirewallTypeChanged (Foundation.NSObject sender);

		[Action ("OnGuiBtnOpenvpnTooltip:")]
		partial void OnGuiBtnOpenvpnTooltip (Foundation.NSObject sender);

		[Action ("OnGuiBtnWireguardTooltip:")]
		partial void OnGuiBtnWireguardTooltip (Foundation.NSObject sender);

		[Action ("OnLaunchAtLoginChanged:")]
		partial void OnLaunchAtLoginChanged (Foundation.NSObject sender);

		[Action ("OnNetworksDefaultActionChanged:")]
		partial void OnNetworksDefaultActionChanged (Foundation.NSObject sender);

		[Action ("OnProtocolChangedOpenVPN:")]
		partial void OnProtocolChangedOpenVPN (Foundation.NSObject sender);

		[Action ("OnProtocolChangedWireGuard:")]
		partial void OnProtocolChangedWireGuard (Foundation.NSObject sender);

		[Action ("SetAllNetworksToDefaultAction:")]
		partial void SetAllNetworksToDefaultAction (Foundation.NSObject sender);

		[Action ("ShowAntitrackerSettings:")]
		partial void ShowAntitrackerSettings (Foundation.NSObject sender);

		[Action ("ShowConnectionSettings:")]
		partial void ShowConnectionSettings (Foundation.NSObject sender);

		[Action ("ShowDiagnosticsSettings:")]
		partial void ShowDiagnosticsSettings (Foundation.NSObject sender);

		[Action ("ShowDnsSettings:")]
		partial void ShowDnsSettings (Foundation.NSObject sender);

		[Action ("ShowFirewallSettings:")]
		partial void ShowFirewallSettings (Foundation.NSObject sender);

		[Action ("ShowGeneralSettings:")]
		partial void ShowGeneralSettings (Foundation.NSObject sender);

		[Action ("ShowNetworksSettings:")]
		partial void ShowNetworksSettings (Foundation.NSObject sender);

		[Action ("ShowOpenVPNSettings:")]
		partial void ShowOpenVPNSettings (Foundation.NSObject sender);

		[Action ("ShowWireguardConfigDetails:")]
		partial void ShowWireguardConfigDetails (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (AntiTrackerSettings != null) {
				AntiTrackerSettings.Dispose ();
				AntiTrackerSettings = null;
			}

			if (ConnectionSettingsView != null) {
				ConnectionSettingsView.Dispose ();
				ConnectionSettingsView = null;
			}

			if (DnsSettings != null) {
				DnsSettings.Dispose ();
				DnsSettings = null;
			}

			if (FirewallSettingsView != null) {
				FirewallSettingsView.Dispose ();
				FirewallSettingsView = null;
			}

			if (GeneralSettingsView != null) {
				GeneralSettingsView.Dispose ();
				GeneralSettingsView = null;
			}

			if (GuiBtnFirewallTypeAlwaysOn != null) {
				GuiBtnFirewallTypeAlwaysOn.Dispose ();
				GuiBtnFirewallTypeAlwaysOn = null;
			}

			if (GuiBtnFirewallTypeOnDemand != null) {
				GuiBtnFirewallTypeOnDemand.Dispose ();
				GuiBtnFirewallTypeOnDemand = null;
			}

			if (GuiBtnLaunchAtLogin != null) {
				GuiBtnLaunchAtLogin.Dispose ();
				GuiBtnLaunchAtLogin = null;
			}

			if (GuiBtnProtocolTypeOpenVPN != null) {
				GuiBtnProtocolTypeOpenVPN.Dispose ();
				GuiBtnProtocolTypeOpenVPN = null;
			}

			if (GuiBtnProtocolTypeWireGuard != null) {
				GuiBtnProtocolTypeWireGuard.Dispose ();
				GuiBtnProtocolTypeWireGuard = null;
			}

			if (GuiButtonOpenvpnTooltip != null) {
				GuiButtonOpenvpnTooltip.Dispose ();
				GuiButtonOpenvpnTooltip = null;
			}

			if (GuiBtnWireguardTooltip != null) {
				GuiBtnWireguardTooltip.Dispose ();
				GuiBtnWireguardTooltip = null;
			}

			if (GuiPanelOpenvpnTooltip != null) {
				GuiPanelOpenvpnTooltip.Dispose ();
				GuiPanelOpenvpnTooltip = null;
			}

			if (GuiPanelWireguardTooltip != null) {
				GuiPanelWireguardTooltip.Dispose ();
				GuiPanelWireguardTooltip = null;
			}

			if (GuiPanelWireguardConfigDetails != null) {
				GuiPanelWireguardConfigDetails.Dispose ();
				GuiPanelWireguardConfigDetails = null;
			}

			if (GuiProgressViewWireguardKeysGeneration != null) {
				GuiProgressViewWireguardKeysGeneration.Dispose ();
				GuiProgressViewWireguardKeysGeneration = null;
			}

			if (GuiProgressWireguardKeysGeneration != null) {
				GuiProgressWireguardKeysGeneration.Dispose ();
				GuiProgressWireguardKeysGeneration = null;
			}

			if (GuiProtocolsConfigTabView != null) {
				GuiProtocolsConfigTabView.Dispose ();
				GuiProtocolsConfigTabView = null;
			}

			if (GuiTabConfigOpenVPN != null) {
				GuiTabConfigOpenVPN.Dispose ();
				GuiTabConfigOpenVPN = null;
			}

			if (GuiTabConfigWireGuard != null) {
				GuiTabConfigWireGuard.Dispose ();
				GuiTabConfigWireGuard = null;
			}

			if (GuiViewWireguardConfig != null) {
				GuiViewWireguardConfig.Dispose ();
				GuiViewWireguardConfig = null;
			}

			if (GuiWireguardConfigDetailsProgressIndicator != null) {
				GuiWireguardConfigDetailsProgressIndicator.Dispose ();
				GuiWireguardConfigDetailsProgressIndicator = null;
			}

			if (GuiWireguardConfigDetailsView != null) {
				GuiWireguardConfigDetailsView.Dispose ();
				GuiWireguardConfigDetailsView = null;
			}

			if (GuiWireguardConfigDetailsViewProgress != null) {
				GuiWireguardConfigDetailsViewProgress.Dispose ();
				GuiWireguardConfigDetailsViewProgress = null;
			}

			if (GuiWireGuardDescription != null) {
				GuiWireGuardDescription.Dispose ();
				GuiWireGuardDescription = null;
			}

			if (NetworksDefaultActionBtn != null) {
				NetworksDefaultActionBtn.Dispose ();
				NetworksDefaultActionBtn = null;
			}

			if (NetworksSettings != null) {
				NetworksSettings.Dispose ();
				NetworksSettings = null;
			}

			if (NetworksView != null) {
				NetworksView.Dispose ();
				NetworksView = null;
			}

			if (OpenVPNSettings != null) {
				OpenVPNSettings.Dispose ();
				OpenVPNSettings = null;
			}

			if (OpenVpnViewExtraParameters != null) {
				OpenVpnViewExtraParameters.Dispose ();
				OpenVpnViewExtraParameters = null;
			}

			if (SettingsView != null) {
				SettingsView.Dispose ();
				SettingsView = null;
			}

			if (Toolbar != null) {
				Toolbar.Dispose ();
				Toolbar = null;
			}
		}
	}

	[Register ("PreferencesWindow")]
	partial class PreferencesWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
