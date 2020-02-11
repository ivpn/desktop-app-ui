using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IVPN.Models.Configuration;
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
    public class AppState
    {
        public const string AppStateFile = "app.state";

        public delegate void OnSessionStatusChangedDelegate(SessionStatus sessionStatus);
        public event OnSessionStatusChangedDelegate OnSessionStatusChanged = delegate {};

        /// <summary> Gets the user account. </summary>
        public SessionStatus SessionStatusInfo
        {
            get => __SessionStatus;
            set
            {
                __SessionStatus = value;

                if (__IsLoaded)
                {
                    Save();

                    // notify event: account info changed
                    OnSessionStatusChanged(SessionStatusInfo);
                }
            }
        }
        private SessionStatus __SessionStatus;

        #region Not serializable properties
        private bool __IsLoaded;

        [JsonIgnore]
        public SessionManager SessionManager { get; private set; }

        [JsonIgnore]
        public AppSettings Settings { get; private set; }

        /// <summary> Gets a value indicating whether is user authenticated. </summary>
        public bool IsAuthenticated() { return Settings.IsUserLoggedIn(); }

        [JsonIgnore]
        public List<string> Capabilities
        {
            get 
            {
                if (SessionStatusInfo == null)
                    return new List<string>();
                
                return SessionStatusInfo.Capabilities;
            }
        }
        #endregion //Not serializable properties

        public void SetAccountInfo(SessionStatus sessionStatus)
        {            
            // change account info
            SessionStatusInfo = sessionStatus;
        }

        #region Singleton
        private AppState() { }
        private static AppState __SingletonInstance;
        #endregion // Singleton

        #region Private functionality

        private void SetSettings(AppSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));

            Settings.OnCredentialsChanged += (credentials) =>
            {
                if (credentials.IsUserLoggedIn())
                    SessionStatusInfo = null;
            };

            SessionManager = SessionManager.CreateSessionManager(settings);
            SessionManager.OnSessionStatusReceived += (SessionStatus sessionStatus) =>
            {
                if (sessionStatus != null)
                    SetAccountInfo(sessionStatus);
            };
        }

        #endregion // Private functionality

        #region Save/Load
        /// <summary>
        /// Get appState singleton instance
        /// OR Load the application state object OR create a new one
        /// </summary>
        public static AppState GetInstance(AppSettings settings)
        {
            if (__SingletonInstance != null)
                return __SingletonInstance;

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
            __SingletonInstance.SetSettings(settings);
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
            SessionStatusInfo = null;
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
