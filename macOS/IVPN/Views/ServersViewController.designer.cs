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
    [Register ("ServersViewController")]
    partial class ServersViewController
	{
		[Outlet]
		AppKit.NSBox HorizontalLine { get; set; }

		[Outlet]
		AppKit.NSScrollView ScrollViewer { get; set; }

		[Outlet]
		AppKit.NSTextField SelectServerText { get; set; }

		[Outlet]
		AppKit.NSView ServersView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (SelectServerText != null) {
				SelectServerText.Dispose ();
				SelectServerText = null;
			}

			if (HorizontalLine != null) {
				HorizontalLine.Dispose ();
				HorizontalLine = null;
			}

			if (ScrollViewer != null) {
				ScrollViewer.Dispose ();
				ScrollViewer = null;
			}

			if (ServersView != null) {
				ServersView.Dispose ();
				ServersView = null;
			}
		}
	}
}
