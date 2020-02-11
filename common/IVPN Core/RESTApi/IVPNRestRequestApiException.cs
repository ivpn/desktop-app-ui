using System;
using IVPN.RESTApi.Core;

namespace IVPN.RESTApi
{
    public class IVPNRestRequestApiException : RestException
    {
        /// <summary> HTTP StatussCode of response </summary>
        public System.Net.HttpStatusCode HttpRetCode { get; }
        public ResponseWithStatus ResponseWithStatus { get; }

        /// <summary> Internal (API) status code (from JSON) </summary>
        public ApiStatusCode ApiStatusCode { get; } = ApiStatusCode.NotDefined;

        public string ApiMessage { get; }

        public IVPNRestRequestApiException(System.Net.HttpStatusCode httpRetCode)
            : base($"HTTP:{(int)httpRetCode} - {httpRetCode}")
        {
            HttpRetCode = httpRetCode;
        }

        public IVPNRestRequestApiException(System.Net.HttpStatusCode httpRetCode, ApiStatusCode apiStatusCode, string apiMessage,
            ResponseWithStatus responseWithStatus = null)
            : base($"{(string.IsNullOrEmpty(apiMessage) ? $"{(int)apiStatusCode} - {apiStatusCode}" : (apiMessage + $" {(int)apiStatusCode}"))}")
        {
            HttpRetCode = httpRetCode;
            ResponseWithStatus = responseWithStatus;

            ApiStatusCode = apiStatusCode;
            ApiMessage = apiMessage;
        }
    }
}
