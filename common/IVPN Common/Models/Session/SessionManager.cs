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
        }

        /// <summary> New session </summary>
        public async Task CreateNewSessionAsync(string accountID, bool isForceDeleteAllSessions = false)
        {            
            try
            {
                var resp = await __Service.Login(accountID, isForceDeleteAllSessions);
                if (resp.APIStatus == 0)
                    throw new Exception("Internal error: Failed to create session");
                else if (resp.APIStatus != (int)ApiStatusCode.Success)
                {
                    if (resp.APIStatus == (int)ApiStatusCode.SessionTooManySessions)
                        OnSessionRequestError(resp.APIStatus, resp.APIErrorMessage, resp.Account);

                    throw new IVPNRestRequestApiException(HttpStatusCode.OK, (ApiStatusCode)resp.APIStatus, resp.APIErrorMessage);
                }

                var acc = new AccountStatus(
                resp.Account.Active,
                IVPN_Helpers.DataConverters.DateTimeConverter.FromUnixTime(resp.Account.ActiveUntil),
                resp.Account.IsRenewable,
                resp.Account.WillAutoRebill,
                resp.Account.IsFreeTrial,
                resp.Account.Capabilities);

                OnAcountStatusReceived(acc);
            }
            catch (Exception ex)
            {
                Logging.Info($"{ex}");
                throw;
            }
        }

        /// <summary> Delete current session </summary>
        public async Task DeleteCurrentSessionAsync(CancellationToken? cancellationToken = null)
        {
            try
            {
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
            Task.Run(async () =>
            {
                try
                {
                    await CheckStatus();
                }
                catch (Exception ex)
                {
                    Logging.Info($"{ex}");
                } 
            });
        }

        /// <summary>
        /// Immediately check accountStatus
        /// </summary>
        public async Task<AccountStatus> CheckStatusNowAsync(int timeoutMs)
        {
            try
            {
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
        }
        #endregion //Public functionality

        #region Private functionality

        protected async Task<AccountStatus> CheckStatus(int timeoutMs = ApiServices.DefaultTimeout)
        {
            try
            {
                if (!__SessionKeeper.IsLoggedIn())
                    return null; // User not loged-in () no registered sesion - nonthing to check

                var resp = await __Service.SessionStatus();
                if (resp.APIStatus == 0)
                    throw new Exception("Internal error: Failed to create session");
                else if (resp.APIStatus != (int)ApiStatusCode.Success)
                {
                    if (resp.APIStatus == (int)ApiStatusCode.SessionNotFound)
                        OnSessionRequestError(resp.APIStatus, resp.APIErrorMessage, resp.Account);

                    throw new IVPNRestRequestApiException(HttpStatusCode.OK, (ApiStatusCode)resp.APIStatus, resp.APIErrorMessage);
                }

                var acc = new AccountStatus(
                    resp.Account.Active,
                    IVPN_Helpers.DataConverters.DateTimeConverter.FromUnixTime(resp.Account.ActiveUntil),
                    resp.Account.IsRenewable,
                    resp.Account.WillAutoRebill,
                    resp.Account.IsFreeTrial,
                    resp.Account.Capabilities);

                OnAcountStatusReceived(acc);
                return acc;
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
