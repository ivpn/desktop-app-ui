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
	[Register ("FastestServerConfigViewController")]
	partial class FastestServerConfigViewController
	{
		[Outlet]
		AppKit.NSScrollView UiScrollViewer { get; set; }

		[Outlet]
		AppKit.NSView UiServersView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (UiServersView != null) {
				UiServersView.Dispose ();
				UiServersView = null;
			}

			if (UiScrollViewer != null) {
				UiScrollViewer.Dispose ();
				UiScrollViewer = null;
			}
		}
	}
}
