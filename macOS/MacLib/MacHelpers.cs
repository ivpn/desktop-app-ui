using System;
using System.Runtime.InteropServices;

using ObjCRuntime;
using Foundation;
using AppKit;
using Security;

namespace MacLib
{
    public class MacHelpers
    {
        public static NSString ToNSString(string str)
        {
            if (str == null)
                return new NSString("");

            return new NSString(str);
        }

        public static NSDictionary SMJobDictionary(string helperLabel)
        {
            IntPtr ptr = GetSMJobDictionary(helperLabel);
            if (ptr == IntPtr.Zero)
                return null;

            return Runtime.GetNSObject(ptr) as NSDictionary;
        }

        public static NSDictionary GetBundleInfoDictionary(string programUrl)
        {
            IntPtr dictPtr = CFBundleCopyInfoDictionary(programUrl);
            if (dictPtr == IntPtr.Zero)
                return null;

            return Runtime.GetNSObject (dictPtr) as NSDictionary;
        }
            
        public static string GetBundleVersion(string programPath)
        {
            NSDictionary dict = GetBundleInfoDictionary(programPath);
            if (dict == null)
                return null;

            NSObject value;

            if (!dict.TryGetValue(new NSString("CFBundleVersion"), out value))
                return null;

            return ((NSString)value).ToString();
        }

        public static bool IsIVPNAppIsRunning(out NSRunningApplication runningApplication)
        {
            runningApplication = null;

            NSWorkspace workspace = new NSWorkspace();
            foreach (var application in workspace.RunningApplications) {
                if (application.LocalizedName == "IVPN" &&
                    application.ProcessIdentifier != NSProcessInfo.ProcessInfo.ProcessIdentifier) {                    
                    runningApplication = application;
                    return true;
                }
            }

            return false;
        }

        public static bool RemoveFile(Authorization auth, string fileName) 
        {
            if (!System.IO.File.Exists(fileName))
                return true;
            
            if (auth.ExecuteWithPrivileges("/bin/rm", AuthorizationFlags.Defaults, 
                    new[] { fileName }) != 0)
                return false;

            return true;
        }

        public static bool RemoveDirectory(Authorization auth, string directoryName) 
        {
            if (!System.IO.Directory.Exists(directoryName))
                return true;

            if (auth.ExecuteWithPrivileges("/bin/rm", AuthorizationFlags.Defaults, new[] {
                    "-rf",
                    directoryName
                }) != 0)
                    return false;
            
            return true;
        }

        public static void LogLine(string message) 
        {
            using (var s = new NSString(message)) {
                NSLog(s.Handle);
            }
        }

        [DllImport("/System/Library/Frameworks/Foundation.framework/Foundation")]
        private extern static void NSLog(IntPtr message);

        [DllImport("libivpn", EntryPoint = "get_smjob_dictionary")]
        private extern static IntPtr GetSMJobDictionary(string label);

        [DllImport("libivpn", EntryPoint = "get_bundle_dictionary")]
        private extern static IntPtr CFBundleCopyInfoDictionary(string applicationPath);

        [DllImport("libivpn", EntryPoint = "connect_to_agent")]
        public extern static void LibLaunchAgent(string machServiceName, LaunchAgentStartedDelegate handler);

        [DllImport("libivpn", EntryPoint = "install_helper")]
        public extern static bool LibInstallHelper(string label);

        [DllImport("libivpn", EntryPoint = "install_helper_with_auth")]
        public extern static bool LibInstallHelperWithAuthorization(string label, IntPtr authRef);

        [DllImport("libivpn", EntryPoint = "remove_helper_with_auth")]
        public extern static bool LibUninstallHelperWithAuthorization(string label, IntPtr authRef);
    }
}

