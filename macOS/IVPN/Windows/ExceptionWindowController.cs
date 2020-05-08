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

using Foundation;
using AppKit;
using System.Threading.Tasks;

namespace IVPN
{
    public partial class ExceptionWindowController : NSWindowController
    {
        private bool __IsModalDialog;
        private bool __IsServiceException { get; }
        private string __ErrorDescription;
        private ErrorReporterEvent __EventToSend;

        private Exception __Exception;
        private const int LOG_FILE_TAIL_LEN = 10 * 1024; // 10Kb

        public ExceptionWindowController (IntPtr handle) : base (handle)
        {
        }

        [Export ("initWithCoder:")]
        public ExceptionWindowController (NSCoder coder) : base (coder)
        {
        }

        private ExceptionWindowController (Exception e, bool isServiceException, bool isModal, string errorTextHeader = null) : base ("ExceptionWindow")
        {
            __Exception = e;
            __IsModalDialog = isModal;
            __IsServiceException = isServiceException;
            __ErrorDescription = errorTextHeader;
        }

        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();

            if (!string.IsNullOrEmpty (__ErrorDescription))
                TextHeader.StringValue = __ErrorDescription;

            try
            {
                if (!AppDelegate.GetAppSettings().IsLoggingEnabled)
                {
                    Task.Run(() =>
                    {
                        System.Threading.Thread.Sleep(500); // show popup after window is started
                    ShowPopupToEnableLogging();
                    });
                }
            }
            catch
            {
                // Ignore all
            }
        }

        public new ExceptionWindow Window {
            get { return (ExceptionWindow)base.Window; }
        }

        partial void OnCancel (Foundation.NSObject sender)
        {
            Close ();    
        }

        partial void OnSendReport (Foundation.NSObject sender)
        {
            SendReport ();
        }

        private void ShowPopupToEnableLogging()
        {
            if (!NSThread.IsMain)
            {
                InvokeOnMainThread(ShowPopupToEnableLogging);
                return;
            }

            try
            {
                NSPopover popover = new NSPopover();
                NSViewController pausePopoverMenuController = new NSViewController();
                pausePopoverMenuController.View = GuiViewForPopover;

                popover.ContentViewController = pausePopoverMenuController;
                popover.Behavior = NSPopoverBehavior.Transient;

                popover.Show(GuiButtonViewReport.Bounds, GuiButtonViewReport, NSRectEdge.MaxYEdge);
            }
            catch 
            {
                // ignore everything
            }
        }

        partial void OnViewReport (Foundation.NSObject sender)
        {
            DiagnosticLogsTextView.Value = UpdateExceptionMessage ();

            if (__IsModalDialog) 
                NSApplication.SharedApplication.StopModal ();
            
            NSApplication.SharedApplication.RunModalForWindow (DiagnosticLogs);
        }

        partial void CloseDiagnosticLogs (Foundation.NSObject sender)
        {
            NSApplication.SharedApplication.StopModal ();

            DiagnosticLogs.OrderOut (this);

            if (__IsModalDialog)
                NSApplication.SharedApplication.RunModalForWindow (Window);
        }

        /// <summary>
        /// Show exception
        /// </summary>
        /// <returns>The show.</returns>
        /// <param name="e">Exception</param>
        /// <param name="isModal">If set to <c>true</c> is modal.</param>
        public static void Show(Exception e, bool isServiceException, bool isModal, string errorTextHeader = null)
        {
            if (e == null)
                return;

            InitializeAndShowWindow(new ExceptionWindowController (e, isServiceException, isModal, errorTextHeader));
        }

        private static void InitializeAndShowWindow(ExceptionWindowController obj)
        {
            obj.Window.WillClose += (object sender, EventArgs ev) => {
                if (obj.__IsModalDialog)
                    NSApplication.SharedApplication.StopModal ();
            };

            if (obj.__IsModalDialog)
                NSApplication.SharedApplication.RunModalForWindow (obj.Window);
        }

        private string UpdateExceptionMessage ()
        {
            if (__EventToSend != null)
                return __EventToSend.ToString();

            __EventToSend = ErrorReporter.PrepareEventToSend(__Exception,
                    AppDelegate.GetAppSettings(),                 
                    GetLogFileData(Platform.ServiceLogFilePath),
                    GetLogFileData(Platform.ServiceLogFilePath + ".0"),
                    null, 
                    null,
                    __IsServiceException
                    );

            return __EventToSend.ToString();
        }

        private string GetLogFileData(string logFilePath)
        {
            return FileUtils.TailOfLog(logFilePath, LOG_FILE_TAIL_LEN);
        }

        private string GetUserComments()
        {
            return CommentsTextField.Value;
        }

        private async void SendReport ()
        {
            ReportButton.Enabled = false;
            ProgressIndicator.StartAnimation (this);

            string error = await UploadDiagnosticsReport (GetUserComments ());

            ProgressIndicator.StopAnimation (this);
            ReportButton.Enabled = true;

            if (!string.IsNullOrEmpty (error))
                NSAlert.WithMessage ("Error submitting diagnostics Logs", "OK", null, null, error).RunModal ();
            else 
            {
                NSAlert.WithMessage ("Report was sent successfuly", "OK", null, null, "Report was sent to IVPN. Thank you!").RunModal ();
                Close ();
            }
        }

        private async Task<string> UploadDiagnosticsReport (string userComments)
        {
            string error = "";

            UpdateExceptionMessage();

            await Task.Run (() => 
            {
                try 
                {
                    ErrorReporter.SendReport(__EventToSend, userComments);
                } 
                catch (Exception ex) 
                {
                    error = ex.Message;
                }
            });

            return error;
        }
    }
}
