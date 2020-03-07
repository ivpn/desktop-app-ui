using System.Net;
using IVPN.Models.Session;

namespace IVPNCommon.Api
{
    #region Proofs
    public class ApiResponseGeoLookup
    {
        public string IpAddress { get; }

        public string CountryCode { get; }

        public string Country { get; }

        public string City { get; }

        public bool IsIvpnServer { get; }
    
        public ApiResponseGeoLookup(string ipAddress, string countryCode, string country, string city, bool isIvpnServer)
        {
            IpAddress = ipAddress;
            CountryCode = !string.IsNullOrEmpty(countryCode) ? countryCode.ToLower() : countryCode;
            Country = country;
            City = city;
            IsIvpnServer = isIvpnServer;
        }
    }
    #endregion // Proofs
}
