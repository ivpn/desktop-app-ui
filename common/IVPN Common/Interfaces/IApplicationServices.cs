using IVPN.Models;
using System.Threading.Tasks;

namespace IVPN.Interfaces
{
    public interface IApplicationServices : ILocalizedStrings
    {
        bool IsExiting { get; }
        bool IsServiceStarted { get; }
        /// <summary>
        /// Check is service installed (applicable for Windows implementation)
        /// If success - must return True
        /// </summary>
        bool IsServiceExists { get; }

        Task<ServiceStartResult> StartService();

        Task<ServiceAttachResult> AttachToService();
    }
}
