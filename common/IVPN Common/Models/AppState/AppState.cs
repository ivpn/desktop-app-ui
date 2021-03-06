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
using System.IO;
using System.Text;
using IVPN.Interfaces;
using IVPN.Models.Session;
using Newtonsoft.Json;

namespace IVPN.Models
{

    /// <summary>
    /// Application state.
    /// Save\restore data between application start.
    /// 
    /// IMPORTANT! Be carefull with data serialization!
    /// Mark '[JsonIgnore]' all public properties which should be excluded from serialization
    /// </summary>
    public class AppState : ISessionKeeper
    {
        public const string AppStateFile = "app.state";

        public delegate void OnAccountStatusChangedDelegate(AccountStatus accountStatus);
        public event OnAccountStatusChangedDelegate OnAccountStatusChanged = delegate {};

        public event OnSessionChangedDelegate OnSessionChanged = delegate { };

        /// <summary> Gets the user account. </summary>
        public AccountStatus AccountStatus
        {
            get => __AccountStatus;
            // keep 'set' public to be able to deserialize
            set
            {
                __AccountStatus = value;

                if (__IsLoaded && __AccountStatus!=null)
                {
                    Save();

                    // notify event: account info changed
                    OnAccountStatusChanged(AccountStatus);
                }
            }
        }
        private AccountStatus __AccountStatus;

        #region Not serializable properties
        private bool __IsLoaded;

        [JsonIgnore]
        public SessionInfo Session { get; private set; }
        [JsonIgnore]
        public SessionManager SessionManager { get; private set; }

        [JsonIgnore]
        public List<string> Capabilities
        {
            get 
            {
                var acc = AccountStatus;
                if (acc == null)
                    return new List<string>();
                return acc.Capabilities;
            }
        }

        #endregion //Not serializable properties

        /// <summary> Gets a value indicating whether is user authenticated. </summary>
        public bool IsLoggedIn() { return !string.IsNullOrEmpty(Session?.Session); }
               
        #region Singleton
        private AppState() { }
        private static AppState __SingletonInstance;
        #endregion // Singleton

        public void SetSession(SessionInfo session)
        {
            var oldSession = Session;
            Session = session;
            if (string.IsNullOrEmpty(Session?.AccountID)
                ||
                (oldSession != null && !string.Equals(oldSession?.AccountID, Session?.AccountID))
               )
            {
                AccountStatus = null;
            }
            OnSessionChanged(Session);

            if (string.IsNullOrEmpty(oldSession?.Session) && !string.IsNullOrEmpty(Session?.Session))
                SessionManager.RequestStatusCheck();
        }

        public void SetAccountStatus(string sessionToken, AccountStatus accountStatus)
        {
            if (string.IsNullOrEmpty(Session?.Session) || !string.Equals(sessionToken, Session?.Session))
                return;

            AccountStatus = accountStatus;
        }

        #region Private functionality
        #endregion // Private functionality

        #region Save/Load
        public static AppState Instance()
        {
            if (__SingletonInstance == null)
                throw new Exception("AppState not initialized");

            return __SingletonInstance;
        }

        /// <summary>
        /// Get appState singleton instance
        /// OR Load the application state object OR create a new one
        /// </summary>
        public static AppState Initialize(IService service)
        {
            if (__SingletonInstance != null)
                return __SingletonInstance;

            // Initialization

            // Create folder where state will save
            try
            {
                if (!Directory.Exists(Platform.UserSettingsDirectory))
                    Directory.CreateDirectory(Platform.UserSettingsDirectory);
            }
            catch (Exception ex)
            {
                // Ignore all file system exceptions
                Logging.Info(string.Format("Error creaing UserSettingsDirectory AppState: {0} ({1})", ex, Platform.UserSettingsDirectory)); 
            }

            // Create new empty instance  OR load previus state
            string fname = Path.Combine(Platform.UserSettingsDirectory, AppStateFile);
            if (!File.Exists(fname))
                __SingletonInstance = new AppState();
            else
            {
                byte[] bytes = File.ReadAllBytes(fname);
                string serializedData = Encoding.UTF8.GetString(bytes);

                try
                {
                    __SingletonInstance = JsonConvert.DeserializeObject<AppState>(serializedData);
                }
                catch (Exception ex)
                {
                    // Ignore all deserialization errors
                    Logging.Info(string.Format("Error loading AppState: {0}", ex));
                }
            }
            if (__SingletonInstance == null)
                __SingletonInstance = new AppState();

            __SingletonInstance.__IsLoaded = true;
            
            // Init session manager
            __SingletonInstance.SessionManager = SessionManager.CreateSessionManager(__SingletonInstance, service);
            return __SingletonInstance;
        }

        public static bool TryToRemoveStateFile()
        {
            try
            {
                string fname = Path.Combine(Platform.UserSettingsDirectory, AppStateFile);
                if (File.Exists(fname))
                    File.Delete(fname);

                return true;
            }
            catch (Exception ex)
            {
                Logging.Info("[ERROR] Failed to remove AppState file: " + ex);
                return false;
            }
        }

        public void Reset()
        {
            SetSession(null);
            AccountStatus = null;
            TryToRemoveStateFile();
        }

        public void Save()
        {
            try
            {
                string fname = Path.Combine(Platform.UserSettingsDirectory, AppStateFile);

                string serializedData = JsonConvert.SerializeObject(this);
                File.WriteAllBytes(fname, Encoding.UTF8.GetBytes(serializedData));
            }
            catch (Exception ex)
            {
                // Ignore all file system exceptions
                Logging.Info(string.Format("Error saving AppState: {0}", ex));
            }
        }
        #endregion // Save/Load
    }
}
