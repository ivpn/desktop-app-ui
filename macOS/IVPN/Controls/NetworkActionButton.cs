using AppKit;
using CoreGraphics;
using IVPNCommon.ViewModels;
using static IVPN.Models.Configuration.NetworkActionsConfig;

namespace IVPN
{
    public class NetworkActionButton: NSButton
    {
        public NetworkAction NetworkAction { get; }
        public ViewModelNetworksSettings NetworksSettingsModel { get; }

        private NSTextField __Title;
        private NSPopUpButton __PopUpButton;

        public NetworkActionButton(NetworkAction networkAction, 
                                   ViewModelNetworksSettings networksSettingsModel,
                                   float width) : base()
        {
            NetworkAction = networkAction;
            NetworksSettingsModel = networksSettingsModel;
            const int constButtonHeight = 35;
            const int constImgHeight = 16;

            Bordered = false;
            Title = "";
            Frame = new CGRect(0, 0, width, constButtonHeight);

            // wifi icon
            var wifiIconView = new NSImageView();
            wifiIconView.Frame = new CGRect(20, (constButtonHeight - constImgHeight) / 2, constImgHeight, constImgHeight);
            wifiIconView.Image = NSImage.ImageNamed("iconWiFiSmallBlue");
            AddSubview(wifiIconView);

            // title
            __Title = UIUtils.NewLabel(networkAction.Network.SSID);
            __Title.Frame = new CGRect(49, wifiIconView.Frame.Y, width/2, 18);
            __Title.TextColor = NSColor.FromRgb(38, 57, 77);
            AddSubview(__Title);

            WiFiActionTypeEnum action = networkAction.Action;
            if (action == WiFiActionTypeEnum.Default)
                action = networksSettingsModel.NetworkActions.DefaultActionType;

            System.nfloat xpos = __Title.Frame.Width + __Title.Frame.X + 20;

            //action
            __PopUpButton = new NSPopUpButton();
            __PopUpButton.Bordered = false;
            __PopUpButton.Frame = new CGRect(xpos, (constButtonHeight - 24) / 2, width - xpos - 10, 24);
            AddSubview(__PopUpButton);

            networkAction.PropertyChanged += NetworkAction_PropertyChanged;
            NetworksSettingsModel.NetworkActions.PropertyChanged += NetworkActions_PropertyChanged;

            CreatePopupButtonElements();
        }


        void NetworkAction_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NetworkAction.Action))
            {
                UpdatePopupButtonElements();
            }
        }

        void NetworkActions_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NetworksSettingsModel.NetworkActions.DefaultActionType))
            {
                CreatePopupButtonElements();
            }
        }

        public static string GetActionName(WiFiActionTypeEnum  action)
        {
            switch (action)
            {
                case WiFiActionTypeEnum.None:
                    return LocalizedStrings.Instance.LocalizedString("NetworkConfig_NoAction");

                case WiFiActionTypeEnum.Untrusted:
                    return LocalizedStrings.Instance.LocalizedString("NetworkConfig_Untrusted");

                case WiFiActionTypeEnum.Trusted:
                    return LocalizedStrings.Instance.LocalizedString("NetworkConfig_Trusted");              
            }
            return LocalizedStrings.Instance.LocalizedString("NetworkConfig_Default");
        }

        private void CreatePopupButtonElements()
        {
            NSMenuItem menuUntrusted = new NSMenuItem("", MenuAction_Activated)
            {
                AttributedTitle = GuiHelpers.AttributedString.Create(GetActionName(WiFiActionTypeEnum.Untrusted), NSColor.SystemRedColor, NSTextAlignment.Center),
                Tag = (int)WiFiActionTypeEnum.Untrusted
            };

            NSMenuItem menuTrusted = new NSMenuItem("", MenuAction_Activated)
            {
                AttributedTitle = GuiHelpers.AttributedString.Create(GetActionName(WiFiActionTypeEnum.Trusted), NSColor.SystemBlueColor, NSTextAlignment.Center),
                Tag = (int)WiFiActionTypeEnum.Trusted
            };

            NSMenuItem menuNoAction = new NSMenuItem("", MenuAction_Activated)
            {
                AttributedTitle = GuiHelpers.AttributedString.Create(GetActionName(WiFiActionTypeEnum.None), NSColor.Black, NSTextAlignment.Center),
                Tag = (int)WiFiActionTypeEnum.None
            };

            string defaultActionText = GetActionName(WiFiActionTypeEnum.Default) + ": " + GetActionName(NetworksSettingsModel.NetworkActions.DefaultActionType);
            NSMenuItem menuDefaultAction = new NSMenuItem("", MenuAction_Activated)
            {
                AttributedTitle = GuiHelpers.AttributedString.Create(defaultActionText, NSColor.SystemGrayColor, NSTextAlignment.Center),
                Tag = (int)WiFiActionTypeEnum.Default
            };

            __PopUpButton.Menu.RemoveAllItems();
            __PopUpButton.Menu.AddItem(menuUntrusted);
            __PopUpButton.Menu.AddItem(menuTrusted);
            //__PopUpButton.Menu.AddItem(menuNoAction); // 'No action' available only for 'Default' action
            __PopUpButton.Menu.AddItem(menuDefaultAction);

            if(__PopUpButton.Menu.Delegate == null)
                __PopUpButton.Menu.Delegate = new MenuDelegateInvertHighlitedItem();

            UpdatePopupButtonElements();
        }

        private void UpdatePopupButtonElements()
        {
            __PopUpButton.SelectItemWithTag((int)NetworkAction.Action);

            if (NetworkAction.Action == WiFiActionTypeEnum.Default)
                __Title.AttributedStringValue = GuiHelpers.AttributedString.Create(NetworkAction.Network.SSID, NSColor.SystemGrayColor);
            else
                __Title.StringValue = NetworkAction.Network.SSID;
        }

        void MenuAction_Activated(object sender, System.EventArgs e)
        {
            NSMenuItem menuItem = sender as NSMenuItem;
            if (menuItem == null)
                return;

            NetworkAction.Action = (WiFiActionTypeEnum)(int)menuItem.Tag;
        }
    }
}
