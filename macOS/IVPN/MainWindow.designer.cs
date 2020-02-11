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
	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		AppKit.NSButton diagnosticsSendButton { get; set; }

		[Outlet]
		AppKit.NSWindow diagnosticsSheet { get; set; }

		[Outlet]
		AppKit.NSTextView diagnosticsTextView { get; set; }

		[Outlet]
		AppKit.NSTextView diagnosticsTextViewUserComments { get; set; }

		[Outlet]
		IVPN.PageView FastestServerConfigPageView { get; set; }

		[Outlet]
		IVPN.CustomButton GuiIntroBtnConnect { get; set; }

		[Outlet]
		IVPN.CustomButton GuiIntroBtnFirewallClose { get; set; }

		[Outlet]
		IVPN.CustomButton GuiIntroBtnFirewallContinue { get; set; }

		[Outlet]
		IVPN.CustomButton GuiIntroBtnServersClose { get; set; }

		[Outlet]
		IVPN.CustomButton GuiIntroBtnServersConnect { get; set; }

		[Outlet]
		IVPN.CustomButton GuiIntroBtnWelcomeShowMe { get; set; }

		[Outlet]
		AppKit.NSPanel GuiIntroductionPanelConnectBtn { get; set; }

		[Outlet]
		AppKit.NSPanel GuiIntroductionPanelFirewall { get; set; }

		[Outlet]
		AppKit.NSPanel GuiIntroductionPanelServers { get; set; }

		[Outlet]
		AppKit.NSPanel GuiIntroductionPanelWelcome { get; set; }

		[Outlet]
		AppKit.NSTextField GuiIntroLabelConnectText { get; set; }

		[Outlet]
		AppKit.NSTextField GuiIntroLabelConnectTitle { get; set; }

		[Outlet]
		AppKit.NSTextField GuiIntroLabelFirewallText { get; set; }

		[Outlet]
		AppKit.NSTextField GuiIntroLabelFirewallTitle { get; set; }

		[Outlet]
		AppKit.NSTextField GuiIntroLabelServersText { get; set; }

		[Outlet]
		AppKit.NSTextField GuiIntroLabelServersTitle { get; set; }

		[Outlet]
		AppKit.NSTextField GuiIntroLabelWelcome { get; set; }

		[Outlet]
		AppKit.NSImageView GuiIntroLogoImage { get; set; }

		[Outlet]
		AppKit.NSToolbar GuiToolBar { get; set; }

		[Outlet]
		AppKit.NSToolbarItem GuiToolBarItemBackButton { get; set; }

		[Outlet]
		AppKit.NSToolbarItem GuiToolbarItemConfigureButton { get; set; }

		[Outlet]
		AppKit.NSToolbarItem GuiToolBarItemFlexibleSpace { get; set; }

		[Outlet]
		AppKit.NSToolbarItem GuiToolBarItemLogo { get; set; }

		[Outlet]
		AppKit.NSToolbarItem GuiToolBarItemMenu { get; set; }

		[Outlet]
		AppKit.NSToolbarItem GuiToolBarItemSpace { get; set; }

		[Outlet]
		AppKit.NSMenu GuiToolBarMenu { get; set; }

		[Outlet]
		AppKit.NSView GuiTopTransparentView { get; set; }

		[Outlet]
		AppKit.NSView InitPageView { get; set; }

		[Outlet]
		AppKit.NSView LogInPageView { get; set; }

		[Outlet]
		IVPN.PageView LogOutPageView { get; set; }

		[Outlet]
		AppKit.NSView MainPageView { get; set; }

		[Outlet]
		AppKit.NSView ResetPasswordPageView { get; set; }

		[Outlet]
		AppKit.NSView ServersPageView { get; set; }

		[Outlet]
		IVPN.PageView SessionLimitPageView { get; set; }

		[Outlet]
		AppKit.NSView SignUpPageView { get; set; }

		[Outlet]
		AppKit.NSView WindowView { get; set; }

		[Action ("connectPressed:")]
		partial void connectPressed (Foundation.NSObject sender);

		[Action ("disconnectPressed:")]
		partial void disconnectPressed (Foundation.NSObject sender);

		[Action ("dismissDiagnosticsSheet:")]
		partial void dismissDiagnosticsSheet (Foundation.NSObject sender);

		[Action ("dismissPreferencesSheet:")]
		partial void dismissPreferencesSheet (Foundation.NSObject sender);

		[Action ("downloadUpdateButtonPressed:")]
		partial void downloadUpdateButtonPressed (Foundation.NSObject sender);

		[Action ("GuiToolBarPreferencesPressed:")]
		partial void GuiToolBarPreferencesPressed (Foundation.NSObject sender);

		[Action ("MenuItemCheckForUpdatesPressed:")]
		partial void MenuItemCheckForUpdatesPressed (Foundation.NSObject sender);

		[Action ("MenuItemLogOutPressed:")]
		partial void MenuItemLogOutPressed (Foundation.NSObject sender);

		[Action ("MenuItemPreferencesPressed:")]
		partial void MenuItemPreferencesPressed (Foundation.NSObject sender);

		[Action ("MenuItemQuitPressed:")]
		partial void MenuItemQuitPressed (Foundation.NSObject sender);

		[Action ("OnIntroBtnConnectPressed:")]
		partial void OnIntroBtnConnectPressed (Foundation.NSObject sender);

		[Action ("OnIntroBtnFirewallClosePressed:")]
		partial void OnIntroBtnFirewallClosePressed (Foundation.NSObject sender);

		[Action ("OnIntroBtnFirewallContinuePressed:")]
		partial void OnIntroBtnFirewallContinuePressed (Foundation.NSObject sender);

		[Action ("OnIntroBtnServersClosePressed:")]
		partial void OnIntroBtnServersClosePressed (Foundation.NSObject sender);

		[Action ("OnIntroBtnServersConnectPressed:")]
		partial void OnIntroBtnServersConnectPressed (Foundation.NSObject sender);

		[Action ("OnIntroBtnWelcomePressed:")]
		partial void OnIntroBtnWelcomePressed (Foundation.NSObject sender);

		[Action ("proxySettingChanged:")]
		partial void proxySettingChanged (Foundation.NSObject sender);

		[Action ("rememberButtonPressed:")]
		partial void rememberButtonPressed (Foundation.NSObject sender);

		[Action ("serverListButtonChanged:")]
		partial void serverListButtonChanged (Foundation.NSObject sender);

		[Action ("openPreferencesSheet:")]
		partial void ShowPreferencesWindow (Foundation.NSObject sender);

		[Action ("submitDiagnosticsLogs:")]
		partial void submitDiagnosticsLogs (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (diagnosticsSendButton != null) {
				diagnosticsSendButton.Dispose ();
				diagnosticsSendButton = null;
			}

			if (diagnosticsSheet != null) {
				diagnosticsSheet.Dispose ();
				diagnosticsSheet = null;
			}

			if (diagnosticsTextView != null) {
				diagnosticsTextView.Dispose ();
				diagnosticsTextView = null;
			}

			if (diagnosticsTextViewUserComments != null) {
				diagnosticsTextViewUserComments.Dispose ();
				diagnosticsTextViewUserComments = null;
			}

			if (FastestServerConfigPageView != null) {
				FastestServerConfigPageView.Dispose ();
				FastestServerConfigPageView = null;
			}

			if (GuiIntroLogoImage != null) {
				GuiIntroLogoImage.Dispose ();
				GuiIntroLogoImage = null;
			}

			if (GuiIntroBtnConnect != null) {
				GuiIntroBtnConnect.Dispose ();
				GuiIntroBtnConnect = null;
			}

			if (GuiIntroBtnFirewallClose != null) {
				GuiIntroBtnFirewallClose.Dispose ();
				GuiIntroBtnFirewallClose = null;
			}

			if (GuiIntroBtnFirewallContinue != null) {
				GuiIntroBtnFirewallContinue.Dispose ();
				GuiIntroBtnFirewallContinue = null;
			}

			if (GuiIntroBtnServersClose != null) {
				GuiIntroBtnServersClose.Dispose ();
				GuiIntroBtnServersClose = null;
			}

			if (GuiIntroBtnServersConnect != null) {
				GuiIntroBtnServersConnect.Dispose ();
				GuiIntroBtnServersConnect = null;
			}

			if (GuiIntroBtnWelcomeShowMe != null) {
				GuiIntroBtnWelcomeShowMe.Dispose ();
				GuiIntroBtnWelcomeShowMe = null;
			}

			if (GuiIntroductionPanelConnectBtn != null) {
				GuiIntroductionPanelConnectBtn.Dispose ();
				GuiIntroductionPanelConnectBtn = null;
			}

			if (GuiIntroductionPanelFirewall != null) {
				GuiIntroductionPanelFirewall.Dispose ();
				GuiIntroductionPanelFirewall = null;
			}

			if (GuiIntroductionPanelServers != null) {
				GuiIntroductionPanelServers.Dispose ();
				GuiIntroductionPanelServers = null;
			}

			if (GuiIntroductionPanelWelcome != null) {
				GuiIntroductionPanelWelcome.Dispose ();
				GuiIntroductionPanelWelcome = null;
			}

			if (GuiIntroLabelConnectText != null) {
				GuiIntroLabelConnectText.Dispose ();
				GuiIntroLabelConnectText = null;
			}

			if (GuiIntroLabelConnectTitle != null) {
				GuiIntroLabelConnectTitle.Dispose ();
				GuiIntroLabelConnectTitle = null;
			}

			if (GuiIntroLabelFirewallText != null) {
				GuiIntroLabelFirewallText.Dispose ();
				GuiIntroLabelFirewallText = null;
			}

			if (GuiIntroLabelFirewallTitle != null) {
				GuiIntroLabelFirewallTitle.Dispose ();
				GuiIntroLabelFirewallTitle = null;
			}

			if (GuiIntroLabelServersText != null) {
				GuiIntroLabelServersText.Dispose ();
				GuiIntroLabelServersText = null;
			}

			if (GuiIntroLabelServersTitle != null) {
				GuiIntroLabelServersTitle.Dispose ();
				GuiIntroLabelServersTitle = null;
			}

			if (GuiIntroLabelWelcome != null) {
				GuiIntroLabelWelcome.Dispose ();
				GuiIntroLabelWelcome = null;
			}

			if (GuiToolBar != null) {
				GuiToolBar.Dispose ();
				GuiToolBar = null;
			}

			if (GuiToolBarItemBackButton != null) {
				GuiToolBarItemBackButton.Dispose ();
				GuiToolBarItemBackButton = null;
			}

			if (GuiToolbarItemConfigureButton != null) {
				GuiToolbarItemConfigureButton.Dispose ();
				GuiToolbarItemConfigureButton = null;
			}

			if (GuiToolBarItemFlexibleSpace != null) {
				GuiToolBarItemFlexibleSpace.Dispose ();
				GuiToolBarItemFlexibleSpace = null;
			}

			if (GuiToolBarItemLogo != null) {
				GuiToolBarItemLogo.Dispose ();
				GuiToolBarItemLogo = null;
			}

			if (GuiToolBarItemMenu != null) {
				GuiToolBarItemMenu.Dispose ();
				GuiToolBarItemMenu = null;
			}

			if (GuiToolBarItemSpace != null) {
				GuiToolBarItemSpace.Dispose ();
				GuiToolBarItemSpace = null;
			}

			if (GuiToolBarMenu != null) {
				GuiToolBarMenu.Dispose ();
				GuiToolBarMenu = null;
			}

			if (GuiTopTransparentView != null) {
				GuiTopTransparentView.Dispose ();
				GuiTopTransparentView = null;
			}

			if (InitPageView != null) {
				InitPageView.Dispose ();
				InitPageView = null;
			}

			if (LogInPageView != null) {
				LogInPageView.Dispose ();
				LogInPageView = null;
			}

			if (LogOutPageView != null) {
				LogOutPageView.Dispose ();
				LogOutPageView = null;
			}

			if (SessionLimitPageView != null) {
				SessionLimitPageView.Dispose ();
				SessionLimitPageView = null;
			}

			if (MainPageView != null) {
				MainPageView.Dispose ();
				MainPageView = null;
			}

			if (ResetPasswordPageView != null) {
				ResetPasswordPageView.Dispose ();
				ResetPasswordPageView = null;
			}

			if (ServersPageView != null) {
				ServersPageView.Dispose ();
				ServersPageView = null;
			}

			if (SignUpPageView != null) {
				SignUpPageView.Dispose ();
				SignUpPageView = null;
			}

			if (WindowView != null) {
				WindowView.Dispose ();
				WindowView = null;
			}
		}
	}

	[Register ("MainWindow")]
	partial class MainWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
