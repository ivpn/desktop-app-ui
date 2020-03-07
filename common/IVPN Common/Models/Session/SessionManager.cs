using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IVPN.Interfaces;
using IVPN.RESTApi;
using IVPNCommon.Api;

namespace IVPN.Models.Session
{
    /// <summary>
    /// Checks session status in background
    /// </summary>
    public class SessionManager : ISessionManager
    {
        #region Singleton
        private static SessionManager __Instance;
        public static SessionManager CreateSessionManager(ISessionKeeper sessionKeeper, IService service)
        {
            if (__Instance == null)
                __Instance = new SessionManager(sessionKeeper, service);
            return __Instance;
        }
        #endregion //Singleton

        #region Internal variables
        // 6 hours
        private const double DefaultCheckIntervalMs = 1000 * 60 * 60 * 6;
        // 5 seconds
        private const double MinCheckIntervalMs = 1000 * 5;

        private DateTime __LastCheckTime = DateTime.MinValue;
        private bool __IsStatusReceived;
        private readonly System.Timers.Timer __TimerAccountCheck = new System.Timers.Timer(DefaultCheckIntervalMs) { Enabled = false, AutoReset = false };

        private bool __IsActive;

        // If request is in process - this value is not NULL
        private CancellationTokenSource __RequestCancellationSource;

        // Settings object
        private readonly ISessionKeeper __SessionKeeper;
        private readonly IService __Service;
        #endregion //Internal variables

        #region Public functionality
        public event OnAccountStatusReceivedDelegate OnAcountStatusReceived = delegate { };
        public event OnSessionRequestErrorDelegate OnSessionRequestError = delegate { };

        private SessionManager(ISessionKeeper sessionKeeper, IService service)
        {
            __SessionKeeper = sessionKeeper;
            __Service = service;

            __TimerAccountCheck.Elapsed += async (object sender, System.Timers.ElapsedEventArgs e) => { await StatusTimerElapsed(); };

            if (__SessionKeeper.IsLoggedIn())
                Initialize();

            __SessionKeeper.OnSessionChanged += (SessionInfo sessionInfo) => 
            {
                __IsStatusReceived = false;

                if (__SessionKeeper.IsLoggedIn())
                    Initialize();
                else
                    UnInitialize();
            };

            ApiServices.Instance.AlternateHostsListUpdated += () =>
            {
                if (!__IsStatusReceived)
                    RequestStatusCheck();
            };
        }

        private void Initialize()
        {
            StartBackgroundChecker();

            Logging.Info("Session status check is enabled");
        }

        public void UnInitialize()
        {
            StopBackgroundChecker();

            Logging.Info("Session status check is DISABLED");
        }

        /// <summary> New session </summary>
        public async Task CreateNewSessionAsync(string accountID, bool isForceDeleteAllSessions = false)
        {            
            try
            {
                StopBackgroundChecker();

                var resp = await __Service.Login(accountID, isForceDeleteAllSessions);
                if (resp.APIStatus == 0)
                    throw new Exception("Internal error: Failed to create session");
                else if (resp.APIStatus != (int)ApiStatusCode.Success)
                    throw new IVPNRestRequestApiException(HttpStatusCode.OK, (ApiStatusCode)resp.APIStatus, resp.APIErrorMessage);

                var acc = new AccountStatus(
                resp.Account.Active,
                IVPN_Helpers.DataConverters.DateTimeConverter.FromUnixTime(resp.Account.ActiveUntil),
                resp.Account.IsRenewable,
                resp.Account.WillAutoRebill,
                resp.Account.IsFreeTrial,
                resp.Account.Capabilities);

                OnAcountStatusReceived(acc);
            }
            catch (IVPNRestRequestApiException ex)
            {
                Logging.Info($"{ex}");

                if (ex.ApiStatusCode == ApiStatusCode.SessionTooManySessions)
                    OnSessionRequestError(ex);
                throw;
            }
            catch (Exception ex)
            {
                Logging.Info($"{ex}");
                throw;
            }
            finally
            {
                StartBackgroundChecker();
            }
        }

        /// <summary> Delete current session </summary>
        public async Task DeleteCurrentSessionAsync(CancellationToken? cancellationToken = null)
        {
            try
            {
                StopBackgroundChecker();
                
                try
                {
                    await __Service.Logout();
                }
                catch (Exception ex)
                {
                    Logging.Info($"Failed to delete session from IVPN server: {ex}");
                }                
            }
            catch (Exception ex)
            {
                Logging.Info($"{ex}");
                throw;
            }
        }

        /// <summary> Request account check in background </summary>        
        public void RequestStatusCheck()
        {
            try
            {
                StopBackgroundChecker();
                StartBackgroundChecker();
            }
            catch (Exception ex)
            {
                Logging.Info($"{ex}");
            }
        }

        /// <summary>
        /// Immediately check accountStatus
        /// </summary>
        public async Task<AccountStatus> CheckStatusNowAsync(int timeoutMs)
        {
            try
            {
                StopBackgroundChecker();

                var ret = await CheckStatus(timeoutMs);

                if (ret != null)
                    OnAcountStatusReceived(ret);

                return ret;
            }
            catch (Exception ex)
            {
                Logging.Info($"{ex}");
                throw;
            }
            finally
            {
                StartBackgroundChecker();
            }
        }
        #endregion //Public functionality

        #region Private functionality
        private void StartBackgroundChecker()
        {
            if (__IsActive)
                return;

            if (! __SessionKeeper.IsLoggedIn())
                return; // User not loged-in () no registered sesion - nonthing to check

            __IsActive = true;

            // execute first check immediately
            // Do not call 'DoCheckAccountStatus ()' directly to avoid checking from main thread
            __TimerAccountCheck.Interval = 1;
            __TimerAccountCheck.Start();
        }

        private void StopBackgroundChecker()
        {
            __IsActive = false;
            __TimerAccountCheck.Stop();
        }

        private async Task StatusTimerElapsed()
        {
            __TimerAccountCheck.Stop();
            if (!__IsActive)
                return;

            AccountStatus retAccountStatus = null;
            try
            {
                // request not often than once per 'MinCheckIntervalMs'
                if (__IsStatusReceived && (DateTime.Now - __LastCheckTime).TotalMilliseconds < MinCheckIntervalMs)
                    return;

                // request acount status
                try
                {
                    var account = await CheckStatus();
                    if (account != null)
                        retAccountStatus = account;
                }
                catch
                {
                    retAccountStatus = null;
                    // IGNORE ALLL !!!
                }

                if (!__IsActive)
                    return;

                if (retAccountStatus != null)
                    OnAcountStatusReceived(retAccountStatus);
            }
            catch (Exception ex)
            {
                Logging.Info($"{ex}");
            }
            finally
            {
                __TimerAccountCheck.Interval = DefaultCheckIntervalMs;

                if (__IsActive)
                {
                    // calculate next account check time based on 'ActiveUtil'
                    if (retAccountStatus != null)
                    {
                        double accountAliveMsLeft = (retAccountStatus.ActiveUtil - DateTime.Now).TotalMilliseconds;
                        if (accountAliveMsLeft > 0)
                        {
                            if (accountAliveMsLeft < DefaultCheckIntervalMs)
                                __TimerAccountCheck.Interval = accountAliveMsLeft;
                        }
                    }

                    __TimerAccountCheck.Start();
                }
            }
        }

        protected async Task<AccountStatus> CheckStatus(int timeoutMs = ApiServices.DefaultTimeout)
        {
            try
            {
                // Cancel previous request (if it still in progress)
                // PC connections can be changed on a current moment. Should be used new connection for request
                // Info: WebRequest can use not more than 2 simultaneous connections to same host
                __RequestCancellationSource?.Cancel();

                if (!__SessionKeeper.IsLoggedIn())
                    return null; // User not loged-in () no registered sesion - nonthing to check

                // send request to rest server                
                try
                {
                    __RequestCancellationSource = new CancellationTokenSource();
                    AccountStatus acc = await ApiServices.Instance.SessionStatusAsync(__RequestCancellationSource.Token, timeoutMs);

                    if (acc != null)
                        __IsStatusReceived = true;

                    return acc;
                }
                finally
                {
                    __LastCheckTime = DateTime.Now;
                    __RequestCancellationSource = null;
                }
            }
            catch (IVPNRestRequestApiException ex)
            {
                Logging.Info($"{ex}");
                if (ex.ApiStatusCode == ApiStatusCode.SessionNotFound)
                    OnSessionRequestError(ex);
                throw;
            }
            catch (Exception ex)
            {
                Logging.Info($"Error checking account status: {ex}");
                throw;
            }
        }
        #endregion //Private functionality
    }
}
