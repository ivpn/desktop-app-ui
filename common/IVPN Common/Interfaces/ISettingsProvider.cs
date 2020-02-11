using System.Runtime.CompilerServices;
using IVPN.Models.Configuration;

namespace IVPN.Interfaces
{
    public interface ISettingsProvider
    {
        void Save(AppSettings settings);

        void Load(AppSettings settings);

        /// <summary>
        /// Reset to defaults
        /// </summary>
        void Reset(AppSettings settings);

        /// <summary>
        /// Some settings should be saved immediately.
        /// For example, "RunOnLogin" for macOS implementation
        /// </summary>
        void OnSettingsChanged(AppSettings settings, [CallerMemberName] string propertyName = "");
    }
}
