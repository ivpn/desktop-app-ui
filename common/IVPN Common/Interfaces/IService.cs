using IVPN.Models;
using System;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IVPNCommon.Interfaces;

namespace IVPN.Interfaces
{
    public delegate void VPNDisconnected(bool failure, DisconnectionReason reason, string reasonDescription);
    public delegate void VPNConnected(ConnectionInfo connectionInfo);

    public interface IService: INotifyPropertyChanged
    {
        event VPNDisconnected Disconnected;
        event VPNConnected Connected;

        event EventHandler ServiceInitialized;
        event EventHandler ServiceExited;

        Task SetCredentials(
            string AccountID,
            string Session,
            string OvpnUser,
            string OvpnPass,
            string WgPublicKey,
            string WgPrivateKey,
            string WgLocalIP,
            Int64 WgKeyGenerated);


        Task<ConnectionResult> Connect(IProgress<string> progress,
                                 CancellationToken cancellationToken,
                                 ConnectionTarget connectionTarget);

        void Suspend();
        void Resume();

        void Disconnect();
        void Exit();

        IServers Servers { get; }

        ServiceState State { get; }

        bool IsSuspended { get; }

        bool KillSwitchIsEnabled { get; set; }

        bool KillSwitchAllowLAN { set; }

        bool KillSwitchAllowLANMulticast { set; }

        bool KillSwitchIsPersistent { get;  set; }

        ConnectionTarget ConnectionTarget { get; }

        Task PauseOn();
        Task PauseOff();

        Task<Responses.SessionNewResponse> Login(string accountID, bool forceDeleteAllSesssions);
        Task Logout();
        Task<Responses.SessionStatusResponse> SessionStatus();

        Task WireGuardGeneratedKeys(bool generateIfNecessary);
        Task WireGuardKeysSetRotationInterval(Int64 interval);

        Task<bool> SetDns(IPAddress dns);

        /// <summary>
        /// Register connection progress object
        /// All registered objects will be notified about progress during connection
        /// </summary>
        /// <param name="progress"></param>
        void RegisterConnectionProgressListener(IProgress<string> progress);
        
    }
}
