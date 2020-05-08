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

﻿using System;

namespace IVPN
{
    public class IVPNUpdater
    {
        /// <summary>
        /// Software updater: checks for a new update and perform update
        /// </summary>
        private static Sparkle.SUUpdater __updater;

        static readonly object __locker = new object();
        static private Foundation.NSUrl __originalFeedUrl;

        private static void Initialize()
        {
            lock (__locker)
            {
                if (__updater == null)
                {
                    __updater = new Sparkle.SUUpdater()
                    {
                        AutomaticallyChecksForUpdates = true,
                        AutomaticallyDownloadsUpdates = false,
                        UpdateCheckInterval = 60 * 60 * 12 // in seconds (12 hours)
                    };
                    __originalFeedUrl = __updater.FeedURL;
                }
            }
        }

        /// <summary>
        /// Initialize and check for updates (if it was not done before)
        /// </summary>
        public static void InitializeUpdater()
        {
            lock (__locker)
            {
                if (__updater == null)
                {
                    Initialize();

                    // Start update-check
                    __updater.InvokeOnMainThread(() => {
                        __updater.CheckForUpdatesInBackground();
                    });
                }
            }
        }

        /// <summary>
        /// Manually check for updates (user selected in menuitem)
        ///
        /// Appcast files for automatic and manual updates are divided.
        /// If appcast for manual update is available AND version of manual update is newer - we should use appcast with manual update.
        /// Othervise - we are using 'original' appcast file for automatic updates.
        /// </summary>
        public static void CheckForUpdates(Foundation.NSObject sender)
        {
            lock (__locker)
            {   
                Initialize();                
                
                try
                {
                    string manualUrl = __originalFeedUrl.AbsoluteString.Replace(".xml", "_manualupdate.xml");

                    string verManual = "";
                    bool isManualInfoReceived = false;
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        try { verManual = ManuallyGetUpdateVersion(manualUrl); }
                        catch { } // ignore all errors
                        finally { isManualInfoReceived = true; }
                    });

                    string verDefault = ManuallyGetUpdateVersion(__originalFeedUrl.AbsoluteString);

                    // parallel downloading appcasts 
                    System.Threading.SpinWait.SpinUntil(() => isManualInfoReceived == true, 3000);

                    if (!string.IsNullOrEmpty(verManual) && !string.IsNullOrEmpty(verDefault))
                    {
                        Sparkle.SUStandardVersionComparator c = new Sparkle.SUStandardVersionComparator();
                        if (c.CompareVersion(verDefault, verManual) == Foundation.NSComparisonResult.Ascending)
                            __updater.FeedURL = new Foundation.NSUrl(manualUrl);                            
                    }                        
                }
                catch (Exception ex)
                {
                    Logging.Info(ex.ToString());
                }
                finally
                {
                    __updater.CheckForUpdates(sender);
                    __updater.FeedURL = __originalFeedUrl;
                }
            }
        }

        private static string ManuallyGetUpdateVersion(string appcastPath)
        {
            System.Xml.XmlDocument xmlDocManual = new System.Xml.XmlDocument();
            xmlDocManual.Load(appcastPath);

            System.Xml.XmlNamespaceManager nsmgr = new System.Xml.XmlNamespaceManager(xmlDocManual.NameTable);
            nsmgr.AddNamespace("sparkle", "http://www.andymatuschak.org/xml-namespaces/sparkle");

            return xmlDocManual.SelectSingleNode("/rss/channel/item/enclosure/@sparkle:version", nsmgr).Value;
        }
    }    
}
