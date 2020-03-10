using IVPN.Interfaces;
using IVPN.Models;
using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace IVPN
{
    public class ApplicationServices: IApplicationServices
    {
        private const string IVPN_WINDOWS_SERVICE_NAME = "IVPN Client";

        private readonly ILocalizedStrings __LocalizedStrings;

        public ApplicationServices(ILocalizedStrings localizedStrings)
        {
            __LocalizedStrings = localizedStrings;
        }

        public string LocalizedString(string key, string defaultText = null)
        {
            return __LocalizedStrings.LocalizedString(key, defaultText);
        }

        public bool IsExiting => App.IsExiting;

        public bool IsServiceExists
        {
            get
            {
#if DEBUG
                return true;
#else
                ServiceController[] services = ServiceController.GetServices();
                var service = services.FirstOrDefault(s => s.ServiceName == IVPN_WINDOWS_SERVICE_NAME);
                return service != null;
#endif
            }
        }

        public bool IsServiceStarted
        {
            get {
#if DEBUG
                    return true;
#else
                ServiceController sc = new ServiceController(IVPN_WINDOWS_SERVICE_NAME);

                return sc.Status == ServiceControllerStatus.Running;
#endif
            }
        }
        
        public async Task<ServiceStartResult> StartService()
        {
            Exception exceptionOnServiceStart = null;
            await Task.Run(() =>
            {
                ServiceController sc = new ServiceController(IVPN_WINDOWS_SERVICE_NAME);

                try
                {
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                }
                catch (Exception ex)
                {
                    exceptionOnServiceStart = ex;
                }
            });

            if (exceptionOnServiceStart != null)
            {
                return new ServiceStartResult(true, exceptionOnServiceStart.Message);
            }

            return new ServiceStartResult(false);
        }

        public async Task<ServiceAttachResult> AttachToService()
        {
            // On Windows, IVPN Client Proxy knows about the port used to connect to service
            // So we just can pass -1 here, and let the IVPNClientProxy figure it out by itself
            return await Task.Run(() => new ServiceAttachResult(-1, 0));
        }
    }
}
