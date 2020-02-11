using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AppUpdater.Gui;
using Microsoft.Win32;

namespace AppUpdater
{
    /// <summary>
    /// Application update functionality 
    /// 
    /// How to use example:
    /// 
    ///     Initialize signature check functionality (if signature check required)
    ///         AppUpdater.Updater.SetSignatureCheckParameters(path_to_openssh_tool, public_dsa_key);
    ///     Initialize updater (start background thread)
    ///         AppUpdater.Updater.Initialize("https://path_to_your_appcast.xml", check_for_update_interval_in_seconds);
    /// 
    ///     Manually check for an update:
    ///         AppUpdater.Updater.CheckForUpdate();
    /// 
    /// </summary>
    
    // Example of appcast file:
    //<rss xmlns:sparkle="http://www.andymatuschak.org/xml-namespaces/sparkle" version="2.0">
    //    <channel>
    //        <item>
    //            <sparkle:releaseNotesLink>
    //                https://.../releasenotes.html
    //            </sparkle:releaseNotesLink>
    // 
    //            <enclosure  url="https:// ... .msi" 
    //                        sparkle:version="2.6.6" 
    //                        length="55597826" 
    //                        sparkle:dsaSignature="MC4CFQDlkzQ2dYsLoEM8egoDb4RjoM1bCQIVAJtcutrPecFCBemdZsRX62sX+aKw"/>
    //        </item>
    //    </channel>
    //</rss>     
    
    // OpenSSL signature creation command example:
    //      openssl dgst -sha1 -binary < testdata.txt | openssl dgst -dss1 -sign dsa_priv.pem | openssl enc -base64
    //
    // OpenSSL signature verification example:
    //      echo MCwCFDNN3l4TcBHQcTuG58s66FTdgJaCAhRqETUsvU8j+X5/HTIsRwcu1Ag0lg== | openssl enc -base64 -d > sigfile.bin
    //      openssl dgst -sha1 -binary testdata.txt | openssl dgst -dss1 -verify dsa_pub.pem -signature sigfile.bin

    public class Updater
    {
        #region Public variables
        /// <summary>
        /// Path to appcast file
        /// </summary>
        public static string AppcastFilePath { get; private set; }

        /// <summary>        
        /// Appcast files for automatic and manual updates are divided.
        /// 
        /// (if you do not want do divide automatic/manual updates - leave it empty)
        /// 
        /// If appcast file for manual update is available AND version of manual update is newer - we should use appcast with manual update.
        /// Othervise - we are using 'original' appcast file for automatic updates.
        /// </summary>
        public static string AppcastFilePathForManualUpdate { get; private set; }

        /// <summary>
        /// Check interval (in seconds). Minimum - 1 hour
        /// </summary>
        public static ulong CheckIntervalSec { get; private set; }
        /// <summary>
        /// Last update check time
        /// </summary>
        public static DateTime LastCheckTime { get; private set; }
        public static string CompanyName { get; private set; }
        public static string AppName { get; private set; }
        public static string AppVersion { get; private set; }

        #endregion //Public variables

        #region Private variables
        private static Thread __UpdaterThread;
        private static readonly object Locker = new object();
        private static string __VersionToSkip;

        /// <summary>
        /// File downloader
        /// </summary>
        private static Downloader __Downloader;
        private static string __TmpFile;

        /// <summary>
        /// DSA signature check parameters
        /// </summary>
        private static string __OpenSslPath;
        private static byte[] __DsaPublicKey;

        /// <summary>
        /// Registry path
        /// </summary>
        private static string __RegistryKey;
        
        /// <summary> 
        /// Internal event 
        /// </summary>
        private static readonly ManualResetEvent ContinueEvent = new ManualResetEvent(false);
        /// <summary> 
        /// true - download or install cancelled 
        /// </summary>
        private static bool __IsCancelled;

        /// <summary>
        /// Latest (current) appcast
        /// </summary>
        private static Appcast __CurrentAppcast;

        /// <summary>
        /// true - update in progress
        /// </summary>
        public static bool IsUpdateInProgress { get; private set; }

        /// <summary>
        /// false   - use internal GUI for user iteraction;
        /// true    - will not be used internal GUI: 
        ///     user iteraction interface should be implemented in external sources.
        /// 
        ///     controlling process is based on reaction on events:
        ///         NewUpdateAvailable: 'Continue()/Cancel()/SkipThisVersion()'
        ///         DownloadFinished: 'Continue()/Cancel()'
        /// </summary>
        private static bool __IsUseExternalGui;
        #endregion //Private variables

        #region Public functionality

        /// <summary>
        /// Initialize update-check mechanism.
        /// Will be started separate thread to check in background.
        /// </summary>
        /// <param name="appcastFilePath">URL of appcast file</param>
        /// <param name="checkIntervalSec">Update check interval (seconds). Min value - 1 hour</param>
        /// <param name="isUseExternalGui">
        /// false   - use internal GUI for user iteraction;
        /// true    - will not be used internal GUI: 
        ///     user iteraction interface should be implemented in external sources.
        /// 
        ///     controlling process is based on reaction on events:
        ///         NewUpdateAvailable: 'Continue()/Cancel()/SkipThisVersion()'
        ///         DownloadFinished: 'Continue()/Cancel()'
        /// </param>
        public static void Initialize(string appcastFilePath, string appcastFilePathForManualUpdate = null, ulong checkIntervalSec = 60 * 60 * 12, bool isUseExternalGui = false)
        {
            Initialize(appcastFilePath, appcastFilePathForManualUpdate, checkIntervalSec, isUseExternalGui, null, null, null);
        }

        public static void Initialize(string appcastFilePath, string appcastFilePathForManualUpdate,
            ulong checkIntervalSec, 
            bool isUseExternalGui, 
            string appVersion, 
            string companyName, 
            string appName)
        {
            if (IsInitialized)
                return;

            lock (Locker)
            {
                if (IsInitialized)
                    return;

                if (string.IsNullOrEmpty(appcastFilePath))
                    throw new ArgumentException("appcastFilePath argument not defined");

                AppcastFilePath = appcastFilePath;
                AppcastFilePathForManualUpdate = appcastFilePathForManualUpdate;

                CheckIntervalSec = checkIntervalSec;
                __IsUseExternalGui = isUseExternalGui;

                if (CheckIntervalSec<60*60)
                    throw new ArgumentException("CheckIntervalSec argument error. Min value: 1 hour");

                if (!string.IsNullOrEmpty(appVersion)
                    && !string.IsNullOrEmpty(companyName)
                    && !string.IsNullOrEmpty(appName))
                {
                    CompanyName = companyName.Trim();
                    AppName = appName.Trim();
                    AppVersion = appVersion.Trim();
                }
                else
                {
                    // Get application info
                    Assembly assembly = Assembly.GetEntryAssembly();
                    var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                    CompanyName = versionInfo.CompanyName.Trim();
                    AppName = assembly.GetName().Name.Trim();
                    AppVersion = assembly.GetName().Version.ToString().Trim();

                    if (string.IsNullOrEmpty(CompanyName))
                        CompanyName = assembly.GetName().Name;
                    if (string.IsNullOrEmpty(AppName))
                        AppName = assembly.GetName().Name;
                }

                // Initialize registry key to save values
                __RegistryKey = string.Format("SOFTWARE\\{0}\\{1}\\Update\\", CompanyName, AppName);
                
                if (string.IsNullOrEmpty(CompanyName))
                    throw new ArgumentException("Unable to obtain company name of application");
                if (string.IsNullOrEmpty(AppName))
                    throw new ArgumentException("Unable to obtain application name of application");

                // Read last info from registry
                ReadRegistry();

                // delete rubbish from previous session
                DeleteTempFile();

                // If no external functions defined - initialize internal GUI implementation
                if (__IsUseExternalGui == false)
                    GuiController.InitializeInternalGui();

                // Start checker thread
                __UpdaterThread = new Thread(UpdaterThread) {Name = "AppUpdater", IsBackground = true};
                __UpdaterThread.Start();
            }
        }

        /// <summary>
        /// Uninitialize updater
        /// </summary>
        public static void UnInitialize()
        {
            lock (Locker)
            {
                try
                {
                    __IsCancelled = true;
                    ContinueEvent.Set();
                    
                    if (__UpdaterThread != null)
                    {
                        if (__UpdaterThread.IsAlive)
                            __UpdaterThread.Interrupt();

                        __UpdaterThread.Join(100);
                        if (__UpdaterThread.IsAlive)
                            __UpdaterThread.Abort();
                    }
                }
                finally
                {
                    __UpdaterThread = null;
                }
            }
        }

        // check if updater initialized
        public static bool IsInitialized
        {
            get { return __UpdaterThread != null; }
        }

        /// <summary>
        /// Call this function before 'Initialize' if you want to check DSA signature of an update.
        /// </summary>
        /// <param name="opensslPath">Path to external OpenSSL tool</param>
        /// <param name="dsaPublicKeyData">Public DSA-key file data</param>
        public static void SetSignatureCheckParameters(string opensslPath, byte[] dsaPublicKeyData)
        {
            if (string.IsNullOrEmpty(opensslPath))
                throw new ArgumentException("opensslPath not defined");
            if (File.Exists(opensslPath)==false)
                throw new ArgumentException("File '" + opensslPath + "' not exists");

            if (dsaPublicKeyData == null || dsaPublicKeyData.Length<=0)
                if (string.IsNullOrEmpty(opensslPath))
                    throw new ArgumentException("dsaPublicKeyData not defined");

            lock (Locker)
            {
                __OpenSslPath = opensslPath;
                __DsaPublicKey = dsaPublicKeyData;
            }
        }

        /// <summary>
        /// Check for updates. 
        /// If update found - show dialog with release-notes and buttons to continue update process.
        /// </summary>
        public static Task CheckForUpdateAsync(bool checkInBackground = false)
        {
            if (IsUpdateInProgress)
                return null;

            return Task.Factory.StartNew(() =>
            {
                lock (Locker)
                {
                    if (IsUpdateInProgress)
                        return;

                    __VersionToSkip = null;
                    WriteRegistry();

                    DoCheckUpdate(true, checkInBackground);
                }
            });
        }
        
        /// <summary>
        /// Download or start installer
        /// IMPORTANT: Should be used ONLY in case of own implementation GUI for user iteraction
        /// </summary>
        public static void Continue()
        {
            __IsCancelled = false;
            ContinueEvent.Set();
        }

        /// <summary>
        /// Cancel download or cancel installer start
        /// IMPORTANT: Should be used ONLY in case of own implementation GUI for user iteraction
        /// </summary>
        public static void Cancel()
        {
            Downloader d = __Downloader;
            if (d != null)
                d.CancelDownload();

            __IsCancelled = true;
            ContinueEvent.Set();
        }

        /// <summary>
        /// Skip current version
        /// IMPORTANT: Should be used ONLY in case of own implementation GUI for user iteraction 
        /// </summary>
        public static void SkipThisVersion()
        {
            Appcast ac = __CurrentAppcast;
            if (ac != null)
            {
                __VersionToSkip = ac.Version;
                WriteRegistry();
            }

            Cancel();
        }

        #endregion //Public functionality

        #region Private functionality

        /// <summary>
        /// Background thread: periodically checks for updates 
        /// </summary>
        private static void UpdaterThread()
        {
            try
            {
                try
                {
                    for (;;)
                    {
                        double sleepToNextCheckMs = (CheckIntervalSec - (DateTime.Now - LastCheckTime).TotalSeconds) * 1000;

                        // max wait time - 10 mins. It could be useful, for example, when PC was in sleep mode a long time
                        if (sleepToNextCheckMs > 60 * 10 * 1000) 
                            sleepToNextCheckMs = 60 * 10 * 1000;

                        if (sleepToNextCheckMs > 0)
                            Thread.Sleep((int) sleepToNextCheckMs);
                        
                        lock (Locker)
                        {
                            if ((DateTime.Now - LastCheckTime).TotalSeconds < CheckIntervalSec)
                                continue;

                            try
                            {
                                DoCheckUpdate(false, true);
                            }
                            catch (Exception)
                            {
                                LastCheckTime = DateTime.Now;
                                WriteRegistry();
                            }
                        }

                        // waiting untill update is downloading
                        while (IsUpdateInProgress)
                            Thread.Sleep(100);
                    }
                }
                catch (ThreadInterruptedException)
                {
                    // thread interrupted
                }
            }
            catch (Exception ex)
            {
                // Unexpected exception
                OnError(ex);
            }

            lock (Locker)
            {
                __UpdaterThread = null;    
            }
        }

        /// <summary>
        /// Perform Update
        /// </summary>
        /// <param name="isManualCheck">TRUE - user iteraction (called by user manually)
        /// In this case, we should ignore "versionToSkip" property
        /// </param>
        /// <param name="checkInBackground"> Do not show "Checking for updates ..." dialog </param>
        private static void DoCheckUpdate(bool isManualCheck = false, bool checkInBackground = false)
        {
            lock (Locker)
            {
                bool isUpdateReady = false;
                try
                {
                    IsUpdateInProgress = true;

                    __CurrentAppcast = null;
                    __IsCancelled = false;
                    ContinueEvent.Reset();

                    // check if downloading not finished
                    if (__Downloader != null)
                        return;

                    // delete rubbish from previous session
                    DeleteTempFile();

                    if (checkInBackground == false)
                        OnCheckingForUpdate();

                    // read latest appcast
                    try
                    {
                        if (isManualCheck && !string.IsNullOrEmpty(AppcastFilePathForManualUpdate))
                        {
                            // Appcast files for automatic and manual updates are divided.
                            // 
                            // If appcast file for manual update is available AND version of manual update is newer - we should use appcast with manual update.
                            // Othervise - we are using 'original' appcast file for automatic updates.
                            try
                            {
                                Appcast manualAppcast = null;
                                bool isManualAppcastDownloaded = false;
                                Task.Run(() =>
                                {
                                    try { manualAppcast = Appcast.Read(AppcastFilePathForManualUpdate); }
                                    catch { manualAppcast = null; }
                                    finally { isManualAppcastDownloaded = true; }
                                });
                                
                                var defaultAppcast = Appcast.Read(AppcastFilePath);
                                SpinWait.SpinUntil(() => isManualAppcastDownloaded == true, 3000);

                                if (manualAppcast != null && defaultAppcast != null && CompareVersions(defaultAppcast.Version, manualAppcast.Version))
                                    __CurrentAppcast = manualAppcast;
                                else
                                    __CurrentAppcast = defaultAppcast;
                            }
                            catch { } // ignore all
                        }
                        
                        if (__CurrentAppcast==null)
                            __CurrentAppcast = Appcast.Read(AppcastFilePath);                        
                    }
                    catch (UpdaterExceptionAppcastDownload ex)
                    {
                        if (isManualCheck)
                            OnError(ex);
                            //OnNothingToUpdate();
                        return;
                    }
                    catch (Exception)
                    {
                        if (isManualCheck)
                            OnNothingToUpdate();
                        return;
                    }
                    finally
                    {
                        LastCheckTime = DateTime.Now;
                        WriteRegistry();
                    }

                    // if called function Cancel()
                    OnAppcastDownloadFinished(__CurrentAppcast);
                    
                    // if called function Cancel()
                    if (__IsCancelled)
                        return;

                    // check if we should skip this version
                    if (string.IsNullOrEmpty(__VersionToSkip) == false
                        && __CurrentAppcast.Version.Equals(__VersionToSkip))
                        return;

                    if (!CompareVersions(__CurrentAppcast.Version))
                    {
                        if (isManualCheck)
                            OnNothingToUpdate();
                        return;
                    }
                    
                    // Notification about new update
                    OnNewUpdateAvailable(__CurrentAppcast);
                    
                    // waiting for call 'Continue()/Cancel()/SkipThisVersion()' function
                    ContinueEvent.WaitOne();
                
                    LastCheckTime = DateTime.Now;
                    WriteRegistry();
                
                    // if called function Cancel()
                    if (__IsCancelled)
                        return;

                    // check if we should skip this version
                    // if was called function SkipThisVersion()
                    if (string.IsNullOrEmpty(__VersionToSkip) == false
                        && __CurrentAppcast.Version.Equals(__VersionToSkip))
                        return;

                    isUpdateReady = true;
                }
                finally
                {
                    if (isUpdateReady == false)
                    {
                        __CurrentAppcast = null;
                        IsUpdateInProgress = false;
                    }
                }

                // delete rubbish from previous session
                DeleteTempFile();

                __TmpFile = Path.GetTempFileName();
                WriteRegistry();

                // save current appcast
                __Downloader = new Downloader(__CurrentAppcast.UpdateLink, __TmpFile);
                __Downloader.Downloaded += _downloader_Downloaded;
                __Downloader.DownloadFinished += _downloader_DownloadFinished;
                __Downloader.Download();
            }
        }

        static void _downloader_DownloadFinished(bool isCanceled, Exception ex)
        {
            try
            {
                lock (Locker)
                {
                    ContinueEvent.Reset();

                    OnDownloadFinished(isCanceled, ex);
                    if (isCanceled || ex != null)
                    {
                        LastCheckTime = DateTime.Now;
                        WriteRegistry();

                        DeleteTempFile();
                        __Downloader = null;
                    }

                    // start installer
                    if (__Downloader != null 
                        && !string.IsNullOrEmpty(__Downloader.DestinationFilePath)
                        && !string.IsNullOrEmpty(__Downloader.SourceFilePath)
                        )
                    {
                        // waiting for call 'Continue()/Cancel()' function
                        ContinueEvent.WaitOne();

                        if (__IsCancelled)
                        {
                            DeleteTempFile();
                        }
                        else
                        {
                            __TmpFile = Path.Combine(Path.GetDirectoryName(__Downloader.DestinationFilePath), Path.GetFileName(__Downloader.SourceFilePath));

                            if (File.Exists(__TmpFile))
                                File.Delete(__TmpFile);

                            // set correct extenfion
                            File.Move(__Downloader.DestinationFilePath, __TmpFile);
                            WriteRegistry();

                            // Check DSA signature
                            if (string.IsNullOrEmpty(__OpenSslPath) == false && __DsaPublicKey != null)
                            {
                                Appcast appcast = __CurrentAppcast;
                                if (appcast == null)
                                {
                                    OnError(new UpdaterExceptionSignatureError());
                                    return;
                                }

                                if (CheckSignature(__TmpFile, appcast.Signature) == false)
                                {
                                    OnError(new UpdaterExceptionSignatureError());
                                    return;
                                }
                            }

                            // start installer!
                            Process.Start(__TmpFile);
                        }
                    }

                    LastCheckTime = DateTime.Now;
                    WriteRegistry();
                }
            }
            finally
            {
                __CurrentAppcast = null;
                __Downloader = null;
                IsUpdateInProgress = false;
            }
        }

        private static void _downloader_Downloaded(long downloadedBytes, long totalBytes)
        {
            OnDownloaded(downloadedBytes, totalBytes);
        }
        
        private static void DeleteTempFile()
        {
            if (__TmpFile != null && File.Exists(__TmpFile))
                File.Delete(__TmpFile);

            __TmpFile = null;
            WriteRegistry();
        }

        /// <summary>
        /// Compare current application version and version from argument
        /// </summary>
        /// <param name="newVer">New application version</param>
        /// <returns>TRUE - new version available</returns>
        private static bool CompareVersions(string newVer)
        {
            return CompareVersions(AppVersion, newVer);
        }

        private static bool CompareVersions(string oldVer, string newVer)
        {
            char[] seperators = { '.' };

            newVer = newVer.Trim();
            oldVer = oldVer.Trim();

            string[] newVerStrings = newVer.Split(seperators);
            string[] curVerStrings = oldVer.Split(seperators);

            try
            {
                for (int i = 0; i < newVerStrings.Length && i < curVerStrings.Length; i++)
                {
                    if (UInt32.Parse(newVerStrings[i]) > UInt32.Parse(curVerStrings[i]))
                        return true;
                    if (UInt32.Parse(newVerStrings[i]) < UInt32.Parse(curVerStrings[i]))
                        return false;
                }

                if (newVerStrings.Length > curVerStrings.Length)
                {
                    for (int i = curVerStrings.Length; i < newVerStrings.Length; i++)
                    {
                        if (UInt32.Parse(newVerStrings[i]) > 0)
                            return true;
                    }
                }
            }
            catch
            {
                // ignore comparison errors
                // In case of errors - return FALSE
            }
            return false;
        }

        /// <summary>
        /// Write data to registry
        /// </summary>
        private static void WriteRegistry()
        {
            try
            {
                if (string.IsNullOrEmpty(__RegistryKey))
                    return;

                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(__RegistryKey))
                {
                    if (key != null)
                    {
                        if (__VersionToSkip == null)
                            key.DeleteValue("VersionToSkip", false);
                        else
                            key.SetValue("VersionToSkip", __VersionToSkip);

                        if (__TmpFile == null)
                            key.DeleteValue("TempFile", false);
                        else
                            key.SetValue("TempFile", __TmpFile);

                        key.SetValue("LastCheckTime", System.Xml.XmlConvert.ToString(LastCheckTime, System.Xml.XmlDateTimeSerializationMode.RoundtripKind));

                        key.Close();
                    }   
                }
            }
            catch (Exception)
            {
                // ignore all exceptions
            }
        }

        /// <summary>
        /// Read data from registry
        /// </summary>
        private static void ReadRegistry()
        {
            try
            {
                if (string.IsNullOrEmpty(__RegistryKey))
                    return;

                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(__RegistryKey))
                {
                    if (key != null)
                    {
                        __VersionToSkip = (string)key.GetValue("VersionToSkip", null);
                        __TmpFile = (string)key.GetValue("TempFile", null);

                        string lastCheckTimeStr = (string)key.GetValue("LastCheckTime", null);
                        if (!string.IsNullOrEmpty(lastCheckTimeStr))
                            LastCheckTime = System.Xml.XmlConvert.ToDateTime(lastCheckTimeStr, System.Xml.XmlDateTimeSerializationMode.RoundtripKind);

                        key.Close();
                    }
                }
            }
            catch (Exception)
            {
                LastCheckTime = DateTime.Now;
            }
        }

        #endregion //Private functionality

        /// <summary>
        /// Check DSA signature of downloaded update
        /// </summary>
        /// <param name="installerPath">Path to downloaded installer</param>
        /// <param name="signature">Signature from update appcast file</param>
        /// <returns>
        /// true - signature is OK (can be started installer)
        /// </returns>
        private static bool CheckSignature(string installerPath, string signature)
        {
            // openssl signature creation command example:
            //      openssl dgst -sha1 -binary < testdata.txt | openssl dgst -sha1 -sign dsa_priv.pem | openssl enc -base64
            //
            // openssl signature verification example:
            //      echo MCwCFDNN3l4TcBHQcTuG58s66FTdgJaCAhRqETUsvU8j+X5/HTIsRwcu1Ag0lg== | openssl enc -base64 -d > sigfile.bin
            //      openssl dgst -sha1 -binary testdata.txt | openssl dgst -sha1 -verify dsa_pub.pem -signature sigfile.bin

            string dsaPubFile = Path.GetTempFileName();
            string signatureFile = Path.GetTempFileName();
            string sha1File = Path.GetTempFileName();

            OnLogMessage("New update verification ...");
            try
            {

                // ==================================================
                // Write signature to file
                File.WriteAllBytes(signatureFile, Convert.FromBase64String(signature));

                // ==================================================
                // Write public DSA key to file
                File.WriteAllBytes(dsaPubFile, __DsaPublicKey);

                // ==================================================
                // get SHA1 of update 
                //      example: openssl dgst -sha1 -binary testdata.txt
                Process p = new Process
                {
                    StartInfo =
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        FileName = __OpenSslPath,
                        Arguments = string.Format("dgst -sha1 -binary \"{0}\"", installerPath)
                    }
                };
                p.Start();

                // save SHA1 to binary file
                using (BinaryWriter writer = new BinaryWriter(File.Open(sha1File, FileMode.Create)))
                {
                    using (BinaryReader br = new BinaryReader(p.StandardOutput.BaseStream))
                    {
                        try
                        {
                            while (true)
                            {
                                writer.Write(br.ReadByte());
                            }
                        }
                        catch (EndOfStreamException) { }
                    }
                }
                p.WaitForExit();

                // ==================================================
                // VERIFY signature
                // Example:
                //      openssl dgst -dss1 -verify dsa_pub.pem -signature sigfile.bin sha1.txt

                // run OpenSSL to verify
                Process openssl = new Process();
                openssl.StartInfo = new ProcessStartInfo(__OpenSslPath, string.Format("dgst -sha1 -verify \"{0}\" -signature \"{1}\" \"{2}\"", dsaPubFile, signatureFile, sha1File));
                openssl.StartInfo.CreateNoWindow = true;
                openssl.StartInfo.UseShellExecute = false;
                openssl.StartInfo.EnvironmentVariables["OPENSSL_CONF"] = "nul";
                openssl.StartInfo.RedirectStandardOutput = true;
                openssl.StartInfo.RedirectStandardError = true;

                // logging all output data
                openssl.OutputDataReceived += (sender, line) =>
                {
                    if (line.Data != null)
                        OnLogMessage(string.Format("OpenSSL: {0}", line.Data));
                };
                openssl.ErrorDataReceived += (sender, line) =>
                {
                    if (line.Data != null)
                        OnLogMessage(string.Format("OpenSSL err.: {0}", line.Data));
                };

                openssl.Start();
                openssl.BeginOutputReadLine();
                openssl.BeginErrorReadLine();

                openssl.WaitForExit(5000);

                if (!openssl.HasExited)
                {
                    OnLogMessage("OpenSSL verification taking too long, killing...");
                    openssl.Kill();
                    return false;
                }

                if (openssl.ExitCode != 0)
                    OnLogMessage(string.Format("OpenSSL exited with errocode: {0}", openssl.ExitCode));

                return openssl.ExitCode == 0;
            }
            catch (Exception ex)
            {
                OnLogMessage("Unexpected exception during update verification:" + ex.Message);
            }
            finally
            {
                File.Delete(dsaPubFile);
                File.Delete(signatureFile);
                File.Delete(sha1File);
            }

            return false;
        }

        #region Events
        /// <summary>
        /// On checking for update
        /// </summary>
        public delegate void CheckingForUpdateDelegate();
        public static event CheckingForUpdateDelegate CheckingForUpdate;

        /// <summary>
        /// On checking for update
        /// </summary>
        public delegate void NothingToUpdateDelegate();
        public static event NothingToUpdateDelegate NothingToUpdate;

        /// <summary>
        /// On new update available
        /// </summary>
        public delegate void NewUpdateAvailableDelegate(Appcast appcast);
        public static event NewUpdateAvailableDelegate NewUpdateAvailable;

        /// <summary>
        /// 'Downloaded' event delegate
        /// </summary>
        /// <param name="downloadedBytes">Downloaded bytes</param>
        /// <param name="totalBytes">Total bytes to download</param>
        public delegate void DownloadedDelegate(long downloadedBytes, long totalBytes);
        public static event DownloadedDelegate Downloaded;

        /// <summary>
        /// Download finished delegate
        /// </summary>
        /// <param name="isCanceled">true - download cancelled by user</param>
        /// <param name="ex">Exception object (if there was an problem)</param>
        public delegate void DownloadFinishedDelegate(bool isCanceled, Exception ex);
        public static event DownloadFinishedDelegate DownloadFinished;

        /// <summary>
        /// Error notification
        /// </summary>
        /// <param name="ex">Exception object (if there was an problem)</param>
        public delegate void ErrorDelegate(Exception ex);
        public static event ErrorDelegate Error;

        /// <summary>
        /// Log message notification
        /// </summary>
        public delegate void LogMessageDelegate(string logMessage);
        public static event LogMessageDelegate LogMessage;

        /// <summary>
        /// Update cancelled by user
        /// </summary>
        public delegate void AppcastDownloadFinishedDelegate(Appcast appcast);
        public static event AppcastDownloadFinishedDelegate AppcastDownloadFinished;

        #region Event notifiers
        private static void OnCheckingForUpdate()
        {
            try
            {
                CheckingForUpdate?.Invoke();
            }
            catch
            {
                // ignore all
            }
        }

        private static void OnNewUpdateAvailable(Appcast appcast)
        {
            try
            {
                NewUpdateAvailable?.Invoke(appcast);
            }
            catch
            {
                // ignore all
            }
        }

        private static void OnDownloaded(long downloadedbytes, long totalbytes)
        {
            try
            {
                Downloaded?.Invoke(downloadedbytes, totalbytes);
            }
            catch
            {
                // ignore all
            }
        }

        private static void OnDownloadFinished(bool iscanceled, Exception ex)
        {
            try
            {
                DownloadFinished?.Invoke(iscanceled, ex);
            }
            catch (Exception exp)
            {
                OnLogMessage("Internal error: "+exp.Message);
            }
        }

        private static void OnError(Exception ex)
        {
            try
            {
                Error?.Invoke(ex);

                OnLogMessage("Error: " + ex.Message);
            }
            catch (Exception exp)
            {
                OnLogMessage("Internal error: " + exp.Message);
            }
        }

        private static void OnLogMessage(string logmessage)
        {
            try
            {
                LogMessage?.Invoke(logmessage);
            }
            catch
            {
                // ignore all
            }
        }

        private static void OnNothingToUpdate()
        {
            try
            {
                NothingToUpdate?.Invoke();
            }
            catch (Exception exp)
            {
                OnLogMessage("Internal error: " + exp.Message);
            }
        }

        private static void OnAppcastDownloadFinished(Appcast appcast)
        {
            try
            {
                AppcastDownloadFinished?.Invoke(appcast);
            }
            catch (Exception exp)
            {
                OnLogMessage("Internal error: " + exp.Message);
            }
        }
        #endregion //Event notifiers
        #endregion //Events


    }
}
