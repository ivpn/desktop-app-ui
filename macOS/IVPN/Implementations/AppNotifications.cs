//
//  IVPN Client Desktop
//  https://github.com/ivpn/desktop-app-ui
//
//  Created by Stelnykovych Alexandr.
//  Copyright (c) 2020 Privatus Limited.
//
//  This file is part of the IVPN Client Desktop.
//
//  The IVPN Client Desktop is free software: you can redistribute it and/or
//  modify it under the terms of the GNU General Public License as published by the Free
//  Software Foundation, either version 3 of the License, or (at your option) any later version.
//
//  The IVPN Client Desktop is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
//  details.
//
//  You should have received a copy of the GNU General Public License
//  along with the IVPN Client Desktop. If not, see <https://www.gnu.org/licenses/>.
//

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

