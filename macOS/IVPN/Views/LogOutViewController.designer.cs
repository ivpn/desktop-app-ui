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
	[Register ("LogOutViewController")]
	partial class LogOutViewController
	{
		[Outlet]
		AppKit.NSProgressIndicator GuiProgressSpinner { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (GuiProgressSpinner != null) {
				GuiProgressSpinner.Dispose ();
				GuiProgressSpinner = null;
			}
		}
	}
}
