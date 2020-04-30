using System;
using System.Threading;
using System.Threading.Tasks;
using IVPN.Interfaces;
using IVPN.Models.Configuration;

namespace IVPN.Models
{
    public class WireguardKeysManager : ModelBase
    {
        public const int HardExpirationIntervalDays = 40;

        private readonly SemaphoreSlim __LockerSemaphore = new SemaphoreSlim(1,1);
        private readonly Func<bool> __IsCanUpdateKey;
        private readonly IService __Service;

        #region Public functionality
        public delegate void OnProgressDelegate(string progress);
        public event OnProgressDelegate OnProgress = delegate {};

        public delegate void OnStartStopDelegate();
        public event OnStartStopDelegate OnStarted = delegate { };
        public event OnStartStopDelegate OnStopped = delegate { };

        public WireguardKeysManager(IService service, Func<bool> isCanUpdateKey)
        {
            __Service = service;
            __IsCanUpdateKey = isCanUpdateKey;
        }

        /// <summary>
        /// Generates the new key.
        /// If there was previous key defined - it will be removed firstly
        /// </summary>
        /// <returns>The new key async.</returns>
        public async Task GenerateNewKeysAsync()
        {
            await __LockerSemaphore.WaitAsync();
            try
            {
                OnStarted();
                await __Service.WireGuardGeneratedKeys(false);
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
            if (!__IsCanUpdateKey())
                return true;

            await __LockerSemaphore.WaitAsync();
            try
            {
                OnStarted();
                await __Service.WireGuardGeneratedKeys(true);
                return true;
            }
            finally
            {
                OnStopped();
                __LockerSemaphore.Release();
            }
        }
        #endregion //Public functionality
    }
}
