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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace IVPN
{
    public partial class ExceptionWindow : Window
    {
        private ErrorReporterEvent __EventToSend;

        private readonly Exception __Exception;
        private readonly bool __IsServiceException;

        public ExceptionWindow(Exception ex, bool isServiceException, string messageText = null)
        {
            InitializeComponent();
            GuiTextBlockMessageText.Text = messageText;

            __Exception = ex;
            __IsServiceException = isServiceException;

            Initialize();
        }

        private void Initialize()
        {
            UIProgressBar.IsIndeterminate = false;
            DataContext = this;
            InitializeEventToSend();

            // ask user to enable logging (if logging not enabled)
            GuiTextBlockAskEnableLogging.Visibility =
                (App.Settings.IsLoggingEnabled) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void InitializeEventToSend()
        {
            if (__EventToSend!=null)
                return;

            __EventToSend = ErrorReporter.PrepareEventToSend(__Exception, App.Settings, GetIVPNLog(), GetIVPNLog0(), null, null, __IsServiceException);
        }

        private string GetIVPNLog()
        {
            return FileUtils.TailOfLog(Platform.ServiceLogFilePath);
        }

        private string GetIVPNLog0()
        {
            return FileUtils.TailOfLog(Platform.ServiceLogFilePath + ".0");
        }

        private string GetUserComments()
        {
            return GuiTextBoxUserComments.Text;
        }
        
        private void ViewReport_Click(object sender, RoutedEventArgs e)
        {
            InitializeEventToSend();

            string logFile = Path.GetTempFileName();
            StreamWriter writer = new StreamWriter(logFile);
            writer.Write(__EventToSend.ToString());
            writer.Close();

            OpenNotepad(logFile);
        }

        private static void OpenNotepad(string logFile)
        {
            Process p = Process.Start("notepad.exe", logFile);

            if (p != null)
            {
                p.Exited += (sender, e) =>
                {
                    try { File.Delete(logFile); }
                    catch
                    {
                        // ignored
                    }
                };
            }
            else
            {
                try { File.Delete(logFile); }
                catch
                {
                    // ignored
                }
            }
        }

        private async void SendReport(object sender, RoutedEventArgs e)
        {
            string error;
            try
            {
                UIProgressBar.IsIndeterminate = true;
                UIProgressBar.Visibility = Visibility.Visible;
                
                error = await UploadDiagnosticsReport(GetUserComments());
            }
            finally
            {
                UIProgressBar.IsIndeterminate = false;
                GuiTextBoxUserComments.IsEnabled = false;
                GuiCheckBoxIsCreateTicket.IsEnabled = false;
                UIProgressBar.Visibility = Visibility.Collapsed;
            }
            
            if (!string.IsNullOrEmpty(error))
                MessageBox.Show(this, 
                    error, 
                    StringUtils.String("Crash_SentErrorHeader"), 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            else
            {
                GuiSendReportButton.IsEnabled = false;
                MessageBox.Show(this, 
                    StringUtils.String("Crash_SentSuccessfully"),
                    StringUtils.String("Crash_SentSuccessfullyHeader"), 
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private async Task<string> UploadDiagnosticsReport(string userComments)
        {
            string error = "";
            await Task.Run(() =>
            {
                try
                {
                    InitializeEventToSend();
                    ErrorReporter.SendReport(__EventToSend, userComments);
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }
            });

            return error;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
