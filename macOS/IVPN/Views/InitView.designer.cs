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
    [Register ("InitViewController")]
    partial class InitViewController
    {
        [Outlet]
        AppKit.NSProgressIndicator ProgressIndicator { get; set; }

        [Outlet]
        AppKit.NSView ServiceErrorDetailsView { get; set; }

        [Action ("Retry:")]
        partial void Retry (Foundation.NSObject sender);
        
        void ReleaseDesignerOutlets ()
        {
            if (ProgressIndicator != null) {
                ProgressIndicator.Dispose ();
                ProgressIndicator = null;
            }

            if (ServiceErrorDetailsView != null) {
                ServiceErrorDetailsView.Dispose ();
                ServiceErrorDetailsView = null;
            }
        }
    }

    [Register ("InitView")]
    partial class InitView
    {
        
        void ReleaseDesignerOutlets ()
        {
        }
    }
}
