using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IVPN.Exceptions;
using IVPN.Interfaces;
using IVPN.Models.Configuration;
using IVPN.RESTApi;
using IVPNCommon.Api;

namespace IVPN.Models
{
    public class WireguardKeysManager : ModelBase
    {
        public const int HardExpirationIntervalDays = 40;

        private readonly SemaphoreSlim __LockerSemaphore = new SemaphoreSlim(1,1);
        private readonly Func<bool> __IsCanUpdateKey;
        private readonly AppSettings __AppSettings;
        private readonly ILocalizedStrings __LocalizedStrings;

        private readonly Timer __CheckTimer;

        #region Public functionality
        public delegate void OnProgressDelegate(string progress);
        public event OnProgressDelegate OnProgress = delegate {};

        public delegate void OnStartStopDelegate();
        public event OnStartStopDelegate OnStarted = delegate { };
        public event OnStartStopDelegate OnStopped = delegate { };

        // Next regeneration date-time
        public DateTime KeysExpiryDate => __AppSettings.WireGuardKeysTimestamp.AddDays(RegenerationIntervalDays);
#if BETA_WG_GENERATION_1MIN
#warning "!!!!!!!!!!!!!!!!!!! BETA_WG_GENERATION_1MIN !!!!!!!!!!!!!!!!!!!!!!!"
        // Delay before next try to regenerate keys (when previous check was failed)
        public const double RetryCheckDelayOnFailMins = 1;

        public double RegenerationIntervalDays => __AppSettings.WireGuardKeysRegenerationIntervalHours / 24.0 / 24.0 / 60;
#elif BETA_WG_GENERATION_10MINS
#warning "!!!!!!!!!!!!!!!!!!! BETA_WG_GENERATION_10MINS !!!!!!!!!!!!!!!!!!!!!!!"
        // Delay before next try to regenerate keys (when previous check was failed)
        public const double RetryCheckDelayOnFailMins = 10;

        public double RegenerationIntervalDays => __AppSettings.WireGuardKeysRegenerationIntervalHours / 24.0 / 24.0 / 6;
#elif BETA_WG_GENERATION_HOURS
#warning "!!!!!!!!!!!!!!!!!!! BETA_WG_GENERATION_HOURS !!!!!!!!!!!!!!!!!!!!!!!"
        // Delay before next try to regenerate keys (when previous check was failed)
        public const double RetryCheckDelayOnFailMins = 60;

        public double RegenerationIntervalDays => __AppSettings.WireGuardKeysRegenerationIntervalHours / 24.0 / 24.0;
#else
        // Delay before next try to regenerate keys (when previous check was failed)
        public const double RetryCheckDelayOnFailMins = 60;

        public double RegenerationIntervalDays => __AppSettings.WireGuardKeysRegenerationIntervalHours / 24.0;
#endif

        public WireguardKeysManager(Func<bool> isCanUpdateKey, AppSettings appSettings, ILocalizedStrings appServices)
        {
            __IsCanUpdateKey = isCanUpdateKey;
            __AppSettings = appSettings;
            __LocalizedStrings = appServices;

            // try regenerate keys by a timer
            __CheckTimer = new Timer(async (object state) =>
            {
                try
                {
                    await RegenerateKeysIfNecessary();
                }
                catch (Exception ex)
                {
                    Logging.Info($"WARNING (WireguardKeysManager): automatic keys regeneration failed: {ex}");
                }
            });

            // check if we should regenerate keys when settings changed ('WireGuardKeysRegenerationIntervalHours' or 'WireGuardKeysTimestamp')
            __AppSettings.PropertyChanged += __AppSettings_PropertyChanged;
        }

        void __AppSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // check if we should regenerate keys when settings changed ('WireGuardKeysRegenerationIntervalHours' or 'WireGuardKeysTimestamp')
            if (string.Equals(e.PropertyName, nameof(__AppSettings.WireGuardKeysRegenerationIntervalHours))
                || string.Equals(e.PropertyName, nameof(__AppSettings.WireGuardKeysTimestamp)))
            {
                // change 'update' timer to 1 second (regeneration check will occur in 1 second) 
                __CheckTimer.Change(1000, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Generates the new key.
        /// If there was previous key defined - it will be removed firstly
        /// </summary>
        /// <returns>The new key async.</returns>
        public async Task GenerateNewKeysAsync()
        {
            if (!__AppSettings.IsUserLoggedIn())
                return;

            await __LockerSemaphore.WaitAsync();
            try
            {
                if (!__AppSettings.IsUserLoggedIn())
                    return;

                OnStarted();
                await DoGenerateNewKeysAsync();
            }
            finally
            {
                OnStopped();
                __LockerSemaphore.Release();
            }
        }

        /// <summary>
        ///     Checks if keys should be re-generated and regenerate them (if necessary).
        ///     This method also starting timer for auto-update mechanism.
        /// </summary>
        /// <returns>
        ///     TRUE - if key upgrade not required or upgrade was success
        ///     FALSE - when upgrade was failed
        /// </returns>
        public async Task<bool> RegenerateKeysIfNecessary()
        {
            if (!IsCanUpdateKey())
                return true;

            await __LockerSemaphore.WaitAsync();
            try
            {
                if (!IsCanUpdateKey() || !__AppSettings.IsUserLoggedIn())
                    return true;

                OnStarted();
                return await DoRegenerateKeysIfNecessary();
            }
            finally
            {
                OnStopped();
                __LockerSemaphore.Release();
            }
        }
#endregion //Public functionality

#region Private functionality
        private bool IsCanUpdateKey()
        {
            bool ret = __IsCanUpdateKey();

            if (ret == false) //Stop timer. We do not need it anymore
                __CheckTimer.Change(Timeout.Infinite, Timeout.Infinite);

            return ret;
        }
        /// <summary>
        ///     Checks if keys should be re-generated and regenerate them (if necessary).
        /// </summary>
        /// <remarks>
        ///     All exceptions will be logged and suppressed.
        /// </remarks>
        /// <returns>
        ///     TRUE - if key upgrade not required or upgrade was success
        ///     FALSE - when upgrade was failed
        /// </returns>
        private async Task<bool> DoRegenerateKeysIfNecessary()
        {
            // turn off timer
            __CheckTimer.Change(Timeout.Infinite, Timeout.Infinite);

            if (!IsCanUpdateKey() || !__AppSettings.IsUserLoggedIn())
                return true;

            bool isError = false;
            bool isGenerated = false;
            try
            {
                if (__AppSettings.IsWireGuardCredentialsAvailable() && KeysExpiryDate <= DateTime.Now)
                {
                    OnProgress(__LocalizedStrings.LocalizedString("WG_Label_KeysGenerating", "Generating new keys..."));

                    string[] keys = await VpnProtocols.WireGuard.Keys.GenerateKeysAsync();
                    string newPrivateKey = keys[0];
                    string newPublicKey = keys[1];

                    OnProgress(__LocalizedStrings.LocalizedString("WG_Label_KeysUploading", "Uploading key to IVPN server..."));

                    try
                    {
                        var calcelationSource = new CancellationTokenSource();
                        IPAddress ip = await ApiServices.Instance.WireguardKeySet(
                                                    publicKey: newPublicKey,
                                                    old_key: __AppSettings.WireGuardClientPublicKey,
                                                    calcelationSource.Token);
                                        
                        __AppSettings.SetWireGuardCredentials(newPrivateKey, newPublicKey, false, ip.ToString());
                        isGenerated = true;
                    }
                    catch (RESTApi.IVPNRestRequestApiException restEx)
                    {
                        throw restEx;
                    }
                }
            }
            catch (RESTApi.IVPNRestRequestApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                isError = true;
                Logging.Info("ERROR: failed to regenerate WireGuard keys: " + ex);
            }
            finally
            {
                TimeSpan interval = KeysExpiryDate - DateTime.Now;
                if (isError == false && interval.TotalMinutes >= RetryCheckDelayOnFailMins)
                {
                    // No errors. (Update succes or no regeneration required)
                    if (isGenerated) // If new key was generated
                        __AppSettings.Save();

                    // Start next check on KeysExpiryDate (execute timer only once)
                    __CheckTimer.Change(interval, new TimeSpan(0, 0, 0, 0, -1));
                }
                else
                {
                    // Update failed - try to regenerate in 'RetryCheckDelayOnFailMins'
                    TimeSpan retryInterval = new TimeSpan(0, (int)RetryCheckDelayOnFailMins, 0);
                    if (interval.Ticks > 0 && retryInterval > interval)
                        retryInterval = interval;
                    __CheckTimer.Change(retryInterval, new TimeSpan(0, 0, 0, 0, -1));
                }

                OnProgress("");
            }
            return !isError;
        }

        private async Task DoGenerateNewKeysAsync()
        {
            try
            {
                if (!__AppSettings.IsUserLoggedIn())
                    return;

                var calcelationSource = new CancellationTokenSource();

                string newPrivateKey;
                string newPublicKey;

                try
                {
                    OnProgress(__LocalizedStrings.LocalizedString("WG_Label_KeysGenerating", "Generating new keys..."));

                    string[] newKeys = await VpnProtocols.WireGuard.Keys.GenerateKeysAsync();
                    newPrivateKey = newKeys[0];
                    newPublicKey = newKeys[1];
                }
                catch (Exception ex)
                {
                    throw new IVPNException(__LocalizedStrings.LocalizedString("WG_Error_FailedToGenerateKeys", "Failed to regenerate WireGuard keys") + $" ({ex.Message})");
                }
                
                try
                {
                    OnProgress(__LocalizedStrings.LocalizedString("WG_Label_KeysUploading", "Uploading key to IVPN server..."));

                    try
                    {
                        IPAddress ip = await ApiServices.Instance.WireguardKeySet(
                                                publicKey: newPublicKey,
                                                old_key: "",
                                                calcelationSource.Token);

                        __AppSettings.SetWireGuardCredentials(newPrivateKey, newPublicKey, false, ip.ToString());
                    }
                    catch (RESTApi.IVPNRestRequestApiException restEx)
                    {
                        // TODO: shell use only restEx.Message here? (the message received from API server)

                        if (restEx.ApiStatusCode == ApiStatusCode.WgPublicKeyLimitReached)
                            throw new IVPNException(__LocalizedStrings.LocalizedString("WG_Error_Api425_KeyLimitReached", "Maximum number of WireGuard keys reached. Please delete unused keys using the IVPN Website Client Area."));

                        throw new IVPNException(__LocalizedStrings.LocalizedString("WG_Error_FailedToUplodKey", "Failed to upload key on IVPN server") + $" ({restEx.Message})");
                    }
                }
                catch(Exception)
                {
                    __AppSettings.ResetWireGuardCredentials();
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (ex is System.Net.WebException)
                    throw new IVPNException(__LocalizedStrings.LocalizedString("Error_RestServer_ConnectionError", "There was an error communicating with our servers. Please check your internet connection and try again.") 
                        + Environment.NewLine + ex.Message);

                Logging.Info("WireGuard keys generarion error: " + ex);
                throw;
            }
            finally
            {
                if (!__AppSettings.IsWireGuardCredentialsAvailable())
                    __AppSettings.VpnProtocolType = VpnProtocols.VpnType.OpenVPN;

                __AppSettings.Save();

                OnProgress("");
            }
        }
#endregion //Private functionality
    }
}
