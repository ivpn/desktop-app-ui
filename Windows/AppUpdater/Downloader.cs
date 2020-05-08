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
using System.Net;
using System.Threading;

namespace AppUpdater
{
    internal class Downloader
    {
        #region Properties
        /// <summary>
        /// Copy from ...
        /// </summary>
        public string SourceFilePath { get; private set; }

        /// <summary>
        /// Copy to ...
        /// </summary>
        public string DestinationFilePath { get; private set; }

        /// <summary>
        /// Maximum timeout for waiting a new portion of data
        /// </summary>
        public uint MaxTimeoutNoDataReceivedSec { get; private set; }

        /// <summary>
        /// Is downloading in progress?
        /// </summary>
        public bool IsDownloading
        {
            get { return _fileDownloaderWebClient != null; }
        }
        #endregion // Properties

        #region Internal variables
        
        private readonly object _locker = new object();
        private WebClient _fileDownloaderWebClient;

        private DateTime _lastDownloadedBytesTime;
        private Timer _connectionCheckTimer;
        private bool _isTimeout;

        #endregion //Internal variables

        #region Events
        /// <summary>
        /// 'Downloaded' event delegate
        /// </summary>
        /// <param name="downloadedBytes">Downloaded bytes</param>
        /// <param name="totalBytes">Total bytes to download</param>
        public delegate void DownloadedDelegate(long downloadedBytes, long totalBytes);
        public event DownloadedDelegate Downloaded;

        /// <summary>
        /// Download finished delegate
        /// </summary>
        /// <param name="isCanceled">true - download cancelled by user</param>
        /// <param name="ex">Exception object (if there was an problem)</param>
        public delegate void DownloadFinishedDelegate(bool isCanceled, Exception ex);
        public event DownloadFinishedDelegate DownloadFinished;

        #endregion //Events

        public Downloader(string sourceFilePath, string destinationFilePath, uint maxTimeoutNoDataReceivedSec = 30)
        {
            SourceFilePath = sourceFilePath;
            DestinationFilePath = destinationFilePath;
            MaxTimeoutNoDataReceivedSec = maxTimeoutNoDataReceivedSec;
        }
        
        private  void TimerConnectionCheckCallback(object state)
        {
            if (_isTimeout || IsDownloading == false)
            {
                StopConnectionCheckTimer();
                return;
            }

            if ((DateTime.Now - _lastDownloadedBytesTime).TotalSeconds > MaxTimeoutNoDataReceivedSec)
            {
                lock (_locker)
                {
                    _isTimeout = true;
                    CancelDownload();
                }
            }
        }

        private void StartConnectionCheckTimer()
        {
            StopConnectionCheckTimer();
            _connectionCheckTimer = new Timer(TimerConnectionCheckCallback, null, 5000, 1000);
        }
        private void StopConnectionCheckTimer()
        {
            var timer = _connectionCheckTimer;
            timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _connectionCheckTimer = null;
        }

        /// <summary>
        /// Start download
        /// </summary>
        public void Download()
        {
            try
            {
                lock (_locker)
                {
                    // Suppress exception: "The request was aborted: Could not create SSL/TLS secure channel."
                    // TODO: probably, TLS1.2 will be enough (ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;)
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    _fileDownloaderWebClient = new WebClient();

                    _fileDownloaderWebClient.DownloadFileCompleted += client_DownloadFileCompleted;
                    _fileDownloaderWebClient.DownloadProgressChanged +=
                        (sender, e) =>
                        {
                            _lastDownloadedBytesTime = DateTime.Now;
                            NotifyDownloaded(e.BytesReceived, e.TotalBytesToReceive);
                        };

                    _isTimeout = false;
                    _fileDownloaderWebClient.DownloadFileAsync(new Uri(SourceFilePath), DestinationFilePath);
                    
                    // Timer: check connection status 
                    StartConnectionCheckTimer();
                }
            }
            catch (Exception ex)
            {
                throw new UpdaterExceptionUpdateDownload("Update download error.", ex);
            }
        }

        /// <summary>
        /// Stop download
        /// </summary>
        public void CancelDownload()
        {
            lock (_locker)
            {
                _fileDownloaderWebClient?.CancelAsync();
            }
        }

        protected void client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            _lastDownloadedBytesTime = DateTime.Now;

            lock (_locker)
            {
                StopConnectionCheckTimer();

                _fileDownloaderWebClient = null;
            }

            UpdaterExceptionUpdateDownload error = null;
            if (e.Error != null)
            {
                if (e.Cancelled)
                {
                    if (_isTimeout)
                        error = new UpdaterExceptionUpdateDownloadTimeout();
                    else
                        error = new UpdaterExceptionUpdateDownload("Download cancelled by user.", e.Error);
                }
                else
                    error = new UpdaterExceptionUpdateDownload("Download failed.", e.Error);
            }
            NotifyDownloadFinished(e.Cancelled, error);
        }

        #region Event invocators
        protected virtual void NotifyDownloaded(long downloadedbytes, long totalbytes)
        {
            var handler = Downloaded;
            handler?.Invoke(downloadedbytes, totalbytes);
        }
        
        protected virtual void NotifyDownloadFinished(bool isCanceled, Exception ex)
        {
            var handler = DownloadFinished;
            handler?.Invoke(isCanceled, ex);
        }
        #endregion //Event invocators
    }
}
