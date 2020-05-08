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
using System.Xml;
using System.Xml.XPath;

namespace AppUpdater
{
    /**
    // Example of appcast file:
    <rss xmlns:sparkle="http://www.andymatuschak.org/xml-namespaces/sparkle" version="2.0">
        <channel>
            <item>
                <sparkle:releaseNotesLink>
                    https://.../releasenotes.html
                </sparkle:releaseNotesLink>
     
                <enclosure  url="https:// ... .msi" 
                            sparkle:version="2.6.6" 
                            length="55597826" 
                            sparkle:dsaSignature="MC4CFQDlkzQ2dYsLoEM8egoDb4RjoM1bCQIVAJtcutrPecFCBemdZsRX62sX+aKw"/>
            </item>
        </channel>
    </rss>     
    */

    /// <summary>
    /// Appcast file content
    /// Appcast file structure is the same as in original Sparkle 
    /// 
    /// Contains also static method to open and parse appcast xml file
    /// </summary>
    public class Appcast
    {
        public string ReleaseNotesLink { get; private set; }
        public string UpdateLink { get; private set; }
        public string Version { get; private set; }
        public UInt32 Length { get; private set; }
        public string Signature { get; private set; }
        
        /// <summary>
        /// Download and read appcast
        /// </summary>
        /// <param name="appcastPath"></param>
        /// <returns></returns>
        internal static Appcast Read(string appcastPath)
        {
            XmlDocument myXmlDocument = new XmlDocument();
            try
            {

                myXmlDocument.Load(appcastPath); 
            }
            catch (Exception ex)
            {
                throw new UpdaterExceptionAppcastDownload("Error loading appcast file.\n" + ex.Message + "\n\nPlease, check internet connection", ex);
            }

            try
            {
                XPathNavigator navigator = myXmlDocument.CreateNavigator();
                XmlNamespaceManager xmlns = new XmlNamespaceManager(navigator.NameTable);

                const string sparkleNamespaceUrl = "http://www.andymatuschak.org/xml-namespaces/sparkle";
                xmlns.AddNamespace("sparkle", sparkleNamespaceUrl);

                if (myXmlDocument.DocumentElement == null)
                    throw new UpdaterExceptionAppcastDownload("Error loading appcast file.");

                Appcast appcast = new Appcast();

                XmlNode releaseNotesNode = myXmlDocument.DocumentElement.SelectSingleNode("/rss/channel/item/sparkle:releaseNotesLink", xmlns);
                if (releaseNotesNode != null)
                {
                    string rn  = releaseNotesNode.InnerText.Trim();
                    if (!string.IsNullOrEmpty(rn))
                        appcast.ReleaseNotesLink = releaseNotesNode.InnerText.Trim();
                }

                XmlNode enclosureNode = myXmlDocument.DocumentElement.SelectSingleNode("/rss/channel/item/enclosure");
                if (enclosureNode==null || enclosureNode.Attributes==null)
                    throw new UpdaterExceptionAppcastParsing("Not found information about update");
                
                XmlAttribute urlAttr = enclosureNode.Attributes["url"];

                XmlAttribute versionAttr = enclosureNode.Attributes["sparkle:version"];
                XmlAttribute lengthAttr = enclosureNode.Attributes["length"];
                XmlAttribute dsaSignatureAttr = enclosureNode.Attributes["sparkle:dsaSignature"];

                if (urlAttr == null)
                    throw new UpdaterExceptionAppcastParsing("Update link not defined");
                if (versionAttr == null)
                    throw new UpdaterExceptionAppcastParsing("Version not defined");
                if (!Uri.IsWellFormedUriString(urlAttr.InnerText.Trim(), UriKind.RelativeOrAbsolute))
                    throw new UpdaterExceptionAppcastParsing("Update link error");

                appcast.UpdateLink = urlAttr.InnerText.Trim();
                appcast.Version = versionAttr.InnerText.Trim();

                UInt32 length;
                if (lengthAttr!=null && UInt32.TryParse(lengthAttr.InnerText.Trim(), out length))
                    appcast.Length = length;

                appcast.Signature = dsaSignatureAttr.InnerText.Trim();

                return appcast;
            }
            catch (Exception ex)
            {
                throw new UpdaterExceptionAppcastParsing("Error parsing appcast file", ex);
            }
        }
    }
}
