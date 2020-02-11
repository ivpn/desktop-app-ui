
using System;
using CoreGraphics;
using Foundation;
using AppKit;
using ObjCRuntime;

using IVPN;
using MacLib;
using System.Diagnostics;

namespace IVPN_Uninstaller
{
    class MainClass
    {
        static void Main(string[] args)
        {
            NSApplication.Init();

            AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
                Logging.Info(e.ExceptionObject.ToString());
            };

            Logging.OmitDate = true;

            NSApplication.Main(args);
        }
    }
}

