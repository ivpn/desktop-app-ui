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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IVPN.Exceptions;
using IVPN.Interfaces;
using IVPN.Lib;
using IVPN.Models;
using IVPN.Models.PrivateEmail;
using IVPN.RESTApi;
using IVPNCommon.Api;

namespace IVPN.ViewModels
{
    public class PrivateEmailsManagerViewModel : ViewModelBase, IOperationStartStopNotifier
    {
        private readonly AppState __AppState;
        private readonly IApplicationServices __AppServices;

        public event OnOperationExecutionEventDelegate OnWillExecute = delegate {};
        public event OnOperationExecutionEventDelegate OnDidExecute = delegate { };

        public delegate void OnNewEmailGeneratedDelegate (PrivateEmailInfo emailInfo);
        public event OnNewEmailGeneratedDelegate OnNewEmailGenerated = delegate { };

        public ObservableCollection<PrivateEmailInfo> PrivateEmails 
        { 
            get { return __PrivateEmails; }
            private set 
            {
                RaisePropertyWillChange ();
                __PrivateEmails = value;
                RaisePropertyChanged ();
            }
        }
        private ObservableCollection<PrivateEmailInfo> __PrivateEmails;

        public PrivateEmailsManagerViewModel (AppState appState, IApplicationServices appServices)
        {
            __AppState = appState;
            __AppServices = appServices;
            PrivateEmails = new ObservableCollection<PrivateEmailInfo> ();  
        }

        private async Task<PrivateEmailInfo> GenerateNewEmailRequest(OnErrorDelegate onErrorDelegate = null)
        {
            try
            {
                CancellationTokenSource src = new CancellationTokenSource ();
                var response = await ApiServices.Instance.PrivateEmailGenerateAsync (src.Token);


                PrivateEmailInfo email = new PrivateEmailInfo(response.Email, response.ForwardToEmail, response.Notes);
                ObservableCollection<PrivateEmailInfo> emails = PrivateEmails;
                emails.Add(email);
                PrivateEmails = emails;

                OnNewEmailGenerated(email);
                return email;

            } catch (OperationCanceledException) {
                return null;
            }
            catch (TimeoutException){
                DoNotifyError(onErrorDelegate, __AppServices.LocalizedString("Error_ApiRequestTimeout"));
                return null;
            }
            catch (WebException ex)
            {
                Logging.Info($"REST request exception : {ex}");
                DoNotifyError(onErrorDelegate,
                    __AppServices.LocalizedString("Error_RestServer_ConnectionError_Title"),
                    __AppServices.LocalizedString("Error_RestServer_ConnectionError"));
                return null;
            }
            catch (Exception ex)
            {
                Logging.Info($"REST request exception : {ex}");
                DoNotifyError(onErrorDelegate, __AppServices.LocalizedString("Error_RestServer_Communication")
                            + Environment.NewLine
                            + Environment.NewLine
                            + $"{IVPNException.GetDetailedMessage(ex)}");
                return null;
            }
        }

        public async Task<PrivateEmailInfo> GenerateNewEmail (OnErrorDelegate onErrorDelegate = null)
        {
            OnWillExecute (this);

            try 
            {
                return await GenerateNewEmailRequest (onErrorDelegate);
            }
            finally 
            {
                OnDidExecute (this);
            }
        }

        public async Task ReloadEmailsInfo()
        { 
            OnWillExecute (this);
            try
            {
                CancellationTokenSource src = new CancellationTokenSource ();
                var ret = await ApiServices.Instance.PrivateEmailListAsync (src.Token);
                PrivateEmails = new ObservableCollection<PrivateEmailInfo>(ret);
            }
            catch (OperationCanceledException){
            }
            catch (TimeoutException){
                NotifyError(__AppServices.LocalizedString("Error_ApiRequestTimeout"));
            }
            catch (WebException ex)
            {
                Logging.Info($"REST request exception : {ex}");
                NotifyError(
                    __AppServices.LocalizedString("Error_RestServer_ConnectionError_Title"),
                    __AppServices.LocalizedString("Error_RestServer_ConnectionError"));
            }
            catch (Exception ex)
            {
                Logging.Info($"REST request exception : {ex}");
                NotifyError(__AppServices.LocalizedString("Error_RestServer_Communication")
                            + Environment.NewLine
                            + Environment.NewLine
                            + $"{IVPNException.GetDetailedMessage(ex)}");
            }
            finally
            {
                OnDidExecute (this);
            }
        }

        public async Task UpdateNotesRequest (PrivateEmailInfo emailInfo, string notes, OnErrorDelegate onErrorDelegate = null)
        {
            try 
            {
                CancellationTokenSource src = new CancellationTokenSource ();
                await ApiServices.Instance.PrivateEmailUpdateNoteAsync (emailInfo.Email, notes, src.Token);

                int idx = GetEmailIndex (emailInfo);

                if (idx >= 0) 
                {
                    PrivateEmails.RemoveAt (idx);
                    PrivateEmails.Insert (idx, new PrivateEmailInfo (emailInfo.Email, emailInfo.ForwardToEmail, notes));
                    // force property updated event
                    PrivateEmails = PrivateEmails;
                }
            } 
            catch (OperationCanceledException)
            {
            }
            catch (TimeoutException)
            {
                DoNotifyError(onErrorDelegate, __AppServices.LocalizedString("Error_ApiRequestTimeout"));
            }
            catch (WebException ex)
            {
                Logging.Info($"REST request exception : {ex}");
                DoNotifyError(onErrorDelegate,
                    __AppServices.LocalizedString("Error_RestServer_ConnectionError_Title"),
                    __AppServices.LocalizedString("Error_RestServer_ConnectionError"));
            }
            catch (Exception ex)
            {
                Logging.Info($"REST request exception : {ex}");
                DoNotifyError(onErrorDelegate, __AppServices.LocalizedString("Error_RestServer_Communication")
                                               + Environment.NewLine
                                               + Environment.NewLine
                                               + $"{ex}");
            }
        }

        public async Task UpdateNotes (PrivateEmailInfo emailInfo, string notes, OnErrorDelegate onErrorDelegate = null)
        {
            if (string.Equals(emailInfo.Notes, notes))
                return;

            if (notes == null)
                notes = "";
            
            OnWillExecute (this);

            try
            {
                await UpdateNotesRequest (emailInfo, notes, onErrorDelegate);
            }
            finally
            {
                OnDidExecute (this);
            }
        }

        public async Task<bool> DeleteEmailRequest (PrivateEmailInfo emailInfo, OnErrorDelegate onErrorDelegate = null)
        {
            try 
            {
                CancellationTokenSource src = new CancellationTokenSource ();
                await ApiServices.Instance.PrivateEmailDeleteAsync (emailInfo.Email, src.Token);

                int idx = GetEmailIndex (emailInfo);
                if (idx >= 0)
                    PrivateEmails.RemoveAt (idx);
                // force property updated event
                PrivateEmails = PrivateEmails;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (TimeoutException)
            {
                DoNotifyError(onErrorDelegate, __AppServices.LocalizedString ("Error_ApiRequestTimeout"));
                return false;
            }
            catch (WebException ex)
            {
                Logging.Info($"REST request exception : {ex}");
                DoNotifyError(onErrorDelegate,
                    __AppServices.LocalizedString("Error_RestServer_ConnectionError_Title"),
                    __AppServices.LocalizedString("Error_RestServer_ConnectionError"));
                return false;
            }
            catch (Exception ex)
            {
                Logging.Info($"REST request exception : {ex}");
                DoNotifyError(onErrorDelegate, __AppServices.LocalizedString("Error_RestServer_Communication")
                                               + Environment.NewLine
                                               + Environment.NewLine
                                               + $"{ex}");
                return false;
            }
            return true;
        }

        public async Task DeleteEmail(PrivateEmailInfo emailInfo, OnErrorDelegate onErrorDelegate = null)
        {
            if (emailInfo == null)
                return;
            
            OnWillExecute (this);

            try
            {
                await DeleteEmailRequest (emailInfo, onErrorDelegate);
            }
            finally 
            {
                OnDidExecute (this);
            }
        }

        public async Task DeleteEmail(IEnumerable<PrivateEmailInfo> emailsToDelete)
        {
            IEnumerable<PrivateEmailInfo> privateEmailInfos = emailsToDelete as PrivateEmailInfo[] ?? emailsToDelete.ToArray();
            if (!privateEmailInfos.Any())
                return;

            OnWillExecute(this);

            try
            {
                foreach (PrivateEmailInfo emailInfo in privateEmailInfos)
                {
                    if (!await DeleteEmailRequest(emailInfo))
                        break;
                }
            }
            finally
            {
                OnDidExecute(this);
            }
        }

        private int GetEmailIndex(PrivateEmailInfo emailInfo)
        {
            int idx = -1;
            for (int i = 0; i < PrivateEmails.Count; i++) 
            {
                if (PrivateEmails [i].Email.Equals (emailInfo.Email)) 
                {
                    idx = i;
                    break;
                }
            }
            return idx;
        }

        private void DoNotifyError(OnErrorDelegate onErrorDelegate, string errorText, string errorDescription = "")
        {
            if (onErrorDelegate!=null)
                onErrorDelegate(errorText, errorDescription);
            else
                NotifyError(errorText, errorDescription);
        }
    }
}
