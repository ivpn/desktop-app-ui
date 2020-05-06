using System.Threading;
using System.Threading.Tasks;

namespace IVPN.Interfaces
{
    public delegate void OnAccountStatusReceivedDelegate(AccountStatus sessionStatus);
    public delegate void OnSessionRequestErrorDelegate(int apiStatus, string apiErrMes, Responses.AccountInfo account);

    public interface ISessionManager
    {
        event OnSessionRequestErrorDelegate OnSessionRequestError;

        /// <summary> Request account check in background </summary>        
        void RequestStatusCheck();

        /// <summary> New session </summary>
        Task CreateNewSessionAsync(string accountID, bool isForceDeleteAllSessions = false);

        /// <summary> Delete current session </summary>
        Task DeleteCurrentSessionAsync(CancellationToken? cancellationToken = null);
    }
}
