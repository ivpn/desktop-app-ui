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
	[Register ("ExceptionWindowController")]
	partial class ExceptionWindowController
	{
		[Outlet]
		AppKit.NSButton CheckBoxIsCreateTicket { get; set; }

		[Outlet]
		AppKit.NSTextView CommentsTextField { get; set; }

		[Outlet]
		AppKit.NSWindow DiagnosticLogs { get; set; }

		[Outlet]
		AppKit.NSTextView DiagnosticLogsTextView { get; set; }

		[Outlet]
		AppKit.NSScrollView DiagnosticLogsView { get; set; }

		[Outlet]
		AppKit.NSButton GuiButtonViewReport { get; set; }

		[Outlet]
		AppKit.NSView GuiViewForPopover { get; set; }

		[Outlet]
		AppKit.NSProgressIndicator ProgressIndicator { get; set; }

		[Outlet]
		AppKit.NSButton ReportButton { get; set; }

		[Outlet]
		AppKit.NSTextField TextHeader { get; set; }

		[Action ("CloseDiagnosticLogs:")]
		partial void CloseDiagnosticLogs (Foundation.NSObject sender);

		[Action ("OnCancel:")]
		partial void OnCancel (Foundation.NSObject sender);

		[Action ("OnSendReport:")]
		partial void OnSendReport (Foundation.NSObject sender);

		[Action ("OnViewReport:")]
		partial void OnViewReport (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (CheckBoxIsCreateTicket != null) {
				CheckBoxIsCreateTicket.Dispose ();
				CheckBoxIsCreateTicket = null;
			}

			if (CommentsTextField != null) {
				CommentsTextField.Dispose ();
				CommentsTextField = null;
			}

			if (DiagnosticLogs != null) {
				DiagnosticLogs.Dispose ();
				DiagnosticLogs = null;
			}

			if (DiagnosticLogsTextView != null) {
				DiagnosticLogsTextView.Dispose ();
				DiagnosticLogsTextView = null;
			}

			if (DiagnosticLogsView != null) {
				DiagnosticLogsView.Dispose ();
				DiagnosticLogsView = null;
			}

			if (ProgressIndicator != null) {
				ProgressIndicator.Dispose ();
				ProgressIndicator = null;
			}

			if (ReportButton != null) {
				ReportButton.Dispose ();
				ReportButton = null;
			}

			if (TextHeader != null) {
				TextHeader.Dispose ();
				TextHeader = null;
			}

			if (GuiViewForPopover != null) {
				GuiViewForPopover.Dispose ();
				GuiViewForPopover = null;
			}

			if (GuiButtonViewReport != null) {
				GuiButtonViewReport.Dispose ();
				GuiButtonViewReport = null;
			}
		}
	}
}
