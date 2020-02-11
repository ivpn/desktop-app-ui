using System.Threading;
using System.Threading.Tasks;
using IVPN.Models.Session;
using IVPN.RESTApi;
using IVPNCommon.Api;

namespace IVPN.Interfaces
{
    public delegate void OnSessionStatusReceivedDelegate(SessionStatus sessionStatus);
    public delegate void OnSessionRequestErrorDelegate(IVPNRestRequestApiException ex);

    public interface ISessionManager
    {
        event OnSessionStatusReceivedDelegate OnSessionStatusReceived;
        event OnSessionRequestErrorDelegate OnSessionRequestError;

        /// <summary> Request account check in background </summary>        
        void RequestStatusCheck();

        /// <summary> Check acount immediatelly </summary>
        Task<SessionStatus> CheckStatusNowAsync(int timeoutMs);

        /// <summary> New session </summary>
        Task<ApiSessionStatusAuthenticate> CreateNewSessionAsync(string username,
            string password,
            CancellationToken? cancellationToken = null,
            bool isForceDeleteAllSessions = false,
            string wireguardPublicKey = null);

        /// <summary> Delete current session </summary>
        Task DeleteCurrentSessionAsync(CancellationToken? cancellationToken = null);
    }
}
