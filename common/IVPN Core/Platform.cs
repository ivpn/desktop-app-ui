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
using System.Collections.Generic;
using System.IO;
using IVPN.Exceptions;

namespace IVPN
{
    public static class Platform
    {
		static Platform ()
		{
			// Default .Net implementation of PowerModeChanged
			// (Microsoft.Win32.SystemEvents.PowerModeChanged does not work on MacOS: do nothing for Mono)
			// 
			// Do not use 'Microsoft.Win32.SystemEvents.PowerModeChanged' event directly in common code
			// Instead, use: 'Platform.PowerModeChanged'
			Microsoft.Win32.SystemEvents.PowerModeChanged += NativeNotifyPowerModeChanged;
		}

        private static string sVersionString = null;

        public enum PlatformImplementation
        {
            WiFi,
        }

        private static Dictionary<PlatformImplementation, object> sImplementations = new Dictionary<PlatformImplementation, object>();

        public static void RegisterImplementation(PlatformImplementation impl, object inst)
        {
            sImplementations[impl] = inst;
        }

        public static object GetImplementation(PlatformImplementation impl)
        {
            return sImplementations.ContainsKey(impl) ? sImplementations[impl] : null;
        }

        public static string InstallationDirectory { get; set; }

        /// <summary>
        /// Global app version info
        /// Must be initialized immediately after application start
        /// </summary>        
        public static string Version
        {
            get
            {
                if (string.IsNullOrEmpty(sVersionString))
                    throw new IVPNInternalException("Version not defined");

                return sVersionString;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new IVPNInternalException("Unable to define empty Version");
                if (string.IsNullOrEmpty(sVersionString) == false)
                    throw new IVPNInternalException("Version already defined");
                sVersionString = value;
            }
        }
        
        public static string HttpUserAgent
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                        return "ivpn/windows " + sVersionString;

                    default:
                        return "ivpn/macos " + sVersionString;
                }
            }
        }

        public static string ShortPlatformName
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                        return "win";

                    default:
                        return "mac";
                }
            }
        }

        public static string PlatformClientName
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                        return "IVPN Client for Windows";

                    default:
                        return "IVPN Client for macOS";
                }
            }
        }

        public static string DeviceType => "desktop";

        public static bool IsUnixLike
        {
            get
            {
                return Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix;
            }
        }

        public static bool IsWindows
        {
            get
            {
                return Environment.OSVersion.Platform == PlatformID.Win32NT;
            }
        }

        public static string SettingsDirectory
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                        return Directory.GetCurrentDirectory() + "\\etc";

                    default:
                        return "/Library/Application Support/IVPN";
                }
            }
        }

        public static string UserSettingsDirectory
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IVPN"); 

                    default:
                        string userfolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        return Path.Combine(userfolder, "Library/Application Support/IVPN");
                }
            }
        }

        public static string LogDirectory
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                        return Path.Combine(InstallationDirectory, "log");

                    default:
                        return SettingsDirectory;
                }
            }
        }

        public static string ServiceLogFilePath
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                        return Path.Combine(LogDirectory, "ivpn.log");

                    default:
                        return "/Library/Logs/IVPN Agent.log";
                }
            }
        }

        public static string OpenSSLExecutablePath
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                        return Path.Combine(InstallationDirectory, "OpenVPN\\" + (Environment.Is64BitProcess ? "x86_64" : "x86") + "\\openssl.exe");

                    default:
                        return "/usr/bin/openssl";
                }
            }
        }

        #region WireGuard
        public static string WireGuardWgExecutablePath
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                        if (Environment.Is64BitOperatingSystem)
                            return Path.Combine(InstallationDirectory, @"WireGuard\x86_64\wg.exe");
                        else
                            return Path.Combine(InstallationDirectory, @"WireGuard\x86\wg.exe");

                    default:
                        return InstallationDirectory + "/Contents/MacOS/WireGuard/wg";
                }
            }
        }

        #endregion //WireGuard

        #region PowerModeChanged
        /// <summary>
        /// Native .Net implementation of Microsoft.Win32.SystemEvents.PowerModeChanged does not work on MacOS: do nothing for Mono
        /// Therefore, we can use this function to get power change notifications from OS-specific implementation
        /// 
        /// Do not use 'Microsoft.Win32.SystemEvents.PowerModeChanged' event directly in common code
        /// Instead, use: 'Platform.PowerModeChanged'
        /// </summary>
        public static void NotifyPowerModeChanged (object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
		{
			// External notification in use. 
			// Unsubscribe from default event for future
			PowerModeChanged -= NativeNotifyPowerModeChanged;

            // Call event

            PowerModeChanged?.Invoke(sender, e);
        }

		/// <summary>
		/// .Net implementation of PowerModeChanged
		/// </summary>
		public static void NativeNotifyPowerModeChanged (object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
		{
            PowerModeChanged?.Invoke(sender, e);
        }

        public static event Microsoft.Win32.PowerModeChangedEventHandler PowerModeChanged = delegate { };
		#endregion //PowerModeChanged
	}
}
