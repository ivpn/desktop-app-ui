using System;

using Foundation;
using AppKit;

using IVPN.Interfaces;

namespace IVPN
{
    public class AppNotifications: IAppNotifications
    {
        private NSUserNotificationCenter __NotificationCenter;
        private MainWindowController __MainWindowController;


        public AppNotifications(MainWindowController mainWindowController)
        {
            __MainWindowController = mainWindowController;
            InitNotificationCenter();
        }

        private void InitNotificationCenter()
        {
            try {
                __NotificationCenter = NSUserNotificationCenter.DefaultUserNotificationCenter;
                __NotificationCenter.ShouldPresentNotification += (center, note) => true;
                __NotificationCenter.DidActivateNotification += (sender, note) =>  {
                    __NotificationCenter.RemoveDeliveredNotification(note.Notification);
                    __MainWindowController.Window.MakeKeyAndOrderFront(__MainWindowController);
                };
            }
            catch {
                Logging.Info("NSUserNotificationCenter not available");
            }
        }

        private void Notify(string title, string description)
        {
            if (__NotificationCenter == null)
                return;

            NSUserNotification note = new NSUserNotification();
            note.Title = title;
            note.InformativeText = description;
            note.DeliveryDate = NSDate.Now;
            note.SoundName = NSUserNotification.NSUserNotificationDefaultSoundName;

            __NotificationCenter.ScheduleNotification(note);
        }

        public void TrayIconNotifyInsecureWiFi(string message)
        {
            Notify(
                "Insecure Wi-Fi Network", 
                "Connection to an insecure WiFi network detected. To protect your privacy, a VPN connection is automatically being established.");
        }

        public void ShowConnectedTrayBaloon(string baloonText)
        {
            // Do not show tray notification 
            // ( Firewall window can be used as indicator of connection process )
            //
            // Notify("Connected", baloonText);
        }

        public void ShowDisconnectedNotification()
        {
            // Do not show tray notification 
            // ( Firewall window can be used as indicator of connection process )
            //
            // Notify("Disconnected", "You have disconnected from the VPN.");
        }

        public void UpdateConnectedTimeDuration(string serverName, string connectedTime)
        {

        }
    }
}

