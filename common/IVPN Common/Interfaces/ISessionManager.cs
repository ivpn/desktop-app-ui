using System.Threading;
using System.Threading.Tasks;
using IVPN.Models.Session;
using IVPN.RESTApi;
using IVPNCommon.Api;

namespace IVPN.Interfaces
{
    public delegate void OnAccountStatusReceivedDelegate(AccountStatus sessionStatus);
    public delegate void OnSessionRequestErrorDelegate(int apiStatus, string apiErrMes, Responses.AccountInfo account);

    public interface ISessionManager
    {
        event OnAccountStatusReceivedDelegate OnAcountStatusReceived;
        event OnSessionRequestErrorDelegate OnSessionRequestError;

        /// <summary> Request account check in background </summary>        
        void RequestStatusCheck();

        /// <summary> Check acount immediatelly </summary>
        Task<AccountStatus> CheckStatusNowAsync(int timeoutMs);

        /// <summary> New session </summary>
        Task CreateNewSessionAsync(string accountID, bool isForceDeleteAllSessions = false);

        /// <summary> Delete current session </summary>
        Task DeleteCurrentSessionAsync(CancellationToken? cancellationToken = null);
    }
}
