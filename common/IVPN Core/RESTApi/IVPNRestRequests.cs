using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace IVPN.RESTApi
{
    internal static class RestUriConsts
    {
        internal const string ServerDnsName = "api.ivpn.net";
        internal const string UriServer = @"https://" + ServerDnsName + "/";
        internal const string UriBase = UriServer + @"v4/";

        // geoLookup
        internal const string UriGeoLookup = UriBase + @"geo-lookup";
        // servers
        internal const string UriGetServers = UriBase + @"servers.json";

        // sesions
        internal const string UriSessionNew     = UriBase + @"session/new";
        internal const string UriSessionStatus  = UriBase + @"session/status";
        internal const string UriSessionDelete  = UriBase + @"session/delete";

        // WireGuard
        internal const string UriWireguardKeySet          = UriBase + @"session/wg/set";

        // private-email
        internal const string UriPrivateEmailList       = UriBase + @"session/pe/list";
        internal const string UriPrivateEmailGenerate   = UriBase + @"session/pe/generate";
        internal const string UriPrivateEmailUpdateNote = UriBase + @"session/pe/update";
        internal const string UriPrivateEmailDelete     = UriBase + @"session/pe/delete";
    }

    /// <summary>
    /// Title : Provides a list of all available servers (version 3): OpenVPN and WireGuard servers
    /// URL : https://api.ivpn.net/v4/servers.json
    ///
    /// Method : GET
    /// URL Params : -
    /// Data Params : { }
    /// Response:
    ///{
    ///    "wireguard": [
    ///    {
    ///            "gateway": "aa.bbb.com",
    ///            "country_code": "US",
    ///            "country": "United States",
    ///            "city": "Los Angeles, CA",
    ///            "hosts": [
    ///            {
    ///                "host": "145.239.239.55",
    ///                "public_key": "fq5ijOijHkhkJWiWT3bC7jRGFfDQo+2EL5aCgGgW5Qw=",
    ///                "local_ip": "10.0.0.1/24"
    ///                }
    ///            ]
    ///        }
    ///    ],
    ///    "openvpn": [
    ///    {
    ///        "gateway": "us-ca.gw.ivpn.net",
    ///        "country_code": "US",
    ///        "country": "United States",
    ///        "city": "Los Angeles, CA",
    ///        "ip_addresses": [
    ///        "173.254.196.58",
    ///        "69.12.80.146",
    ///        "209.58.130.196",
    ///        "173.254.204.202"
    ///            ]
    ///        }
    ///    ],
    ///     "config":
    ///     {
    ///         "api": {
    ///             "ips": [
    ///                 "192.99.193.251",
    ///                 "167.114.18.34"
    ///             ]
    ///         },
    ///         "antitracker":{
    ///             "default":{
    ///                 "ip":"10.0.254.2",
    ///                 "multihop-ip":"10.0.254.102"
    ///             },
    ///             "hardcore":{
    ///                 "ip":"10.0.254.3",
    ///                 "multihop-ip":"10.0.254.103"
    ///             }
    ///         }
    ///     }
    ///}
    /// </summary>
    public class RestRequestGetServers : IVPNRestRequest<RestRequestGetServers.ServersInfoResponse>
    {
        public class ServersInfoResponse
        {
            public class WireGuardServerInfoResponse
            {
                public class HostInfo
                {
                    [JsonProperty("host")]
                    public string Host { get; set; }
                    [JsonProperty("public_key")]
                    public string PublicKey { get; set; }
                    [JsonProperty("local_ip")]
                    public string LocalIp { get; set; }
                }

                [JsonProperty("gateway")]
                public string Gateway { get; set; }
                [JsonProperty("country_code")]
                public string CountryCode { get; set; }
                [JsonProperty("country")]
                public string Country { get; set; }
                [JsonProperty("city")]
                public string City { get; set; }

                [JsonProperty("hosts")]
                public HostInfo[] Hosts { get; set; }
            }

            public class OpenVpnServerInfoResponse
            {
                [JsonProperty("city")]
                public string City { get; set; }
                [JsonProperty("country")]
                public string Country { get; set; }
                [JsonProperty("country_code")]
                public string CountryCode { get; set; }
                [JsonProperty("gateway")]
                public string Gateway { get; set; }
                [JsonProperty("ip_addresses")]
                public string[] IpAddresses { get; set; }
            }

            public class ConfigInfoResponse
            {
                public class ApiInfo
                {
                    [JsonProperty("ips")]
                    public string[] IPs;
                }

                public class AntiTrackerInfo
                {
                    public class DnsInfo
                    {
                        public string Ip { get; set; }

                        [JsonProperty("multihop-ip")]
                        public string MultihopIp { get; set; }
                    }
                    [DataMember]
                    [JsonProperty("default")]
                    public DnsInfo Default { get; set; }
                    [DataMember]
                    [JsonProperty("hardcore")]
                    public DnsInfo Hardcore { get; set; }
                }

                [DataMember]
                [JsonProperty("antitracker")]
                public AntiTrackerInfo AntiTracker { get; set; }

                [DataMember]
                [JsonProperty("api")]
                public ApiInfo Api { get; set; }
            }

            [JsonProperty("wireguard")]
            public WireGuardServerInfoResponse[] WireGuardServers { get; set; }
            [JsonProperty("openvpn")]
            public OpenVpnServerInfoResponse[] OpenVpnServers { get; set; }
            [JsonProperty("config")]
            public ConfigInfoResponse Config { get; set; }
        }

        public RestRequestGetServers() : base(RestUriConsts.UriGetServers) { }
    }

    public class RestRequestGeoLookup : IVPNRestRequest<RestRequestGeoLookup.GeoLookupResponse>
    {
        public class GeoLookupResponse
        {
            [JsonProperty("ip_address")]
            public string IpAddress { get; set; }

            [JsonProperty("country_code")]
            public string CountryCode { get; set; }

            [JsonProperty("country")]
            public string Country { get; set; }

            [JsonProperty("city")]
            public string City { get; set; }

            [JsonProperty("is_ivpn_server")]
            public bool IsIvpnServer { get; set; }
        }

        public RestRequestGeoLookup() : base(RestUriConsts.UriGeoLookup, HttpMethodsEnum.GET) { }
    }

    #region WireGuard
    /// <summary>
    /// {{api_host}}/v4/session/wg/set
    ///
    /// {
    ///     "session_token": "{{session_token}}",
    ///     "public_key": "{{wg_public_key}}"
    /// }
    /// </summary>
    public class RestRequestWireGuardKeySet : IVPNRestRequest<RestRequestWireGuardKeySet.WireGuardKeySetResponse>
    {
        public class WireGuardKeySetResponse : ResponseWithStatus
        {
            [JsonProperty("ip_address")]
            public string IpAddress { get; set; }
        }

        public RestRequestWireGuardKeySet(string sessionToken, string publicWgKey, string connected_public_key)
            : base(RestUriConsts.UriWireguardKeySet, HttpMethodsEnum.POST)
        {
            Dictionary<string, object> jsonParams;

            if (string.IsNullOrEmpty(connected_public_key))
                jsonParams = new Dictionary<string, object> { { "session_token", sessionToken }, { "public_key", publicWgKey } };
            else
                jsonParams = new Dictionary<string, object> { { "session_token", sessionToken }, { "public_key", publicWgKey }, { "connected_public_key", connected_public_key } };

            PostData = JsonConvert.SerializeObject(jsonParams);
        }
    }

    #endregion //WireGuard

    #region Private Email
    /// <summary>
    /// </summary>
    public class RestRequestPrivateEmailList : IVPNRestRequest<RestRequestPrivateEmailList.PrivateEmailListResponse>
    {
        public class PrivateEmailListResponse : ResponseWithStatus
        {
            public class EmailInfo
            {
                [JsonProperty("source")]
                public string Email { get; set; }
                [JsonProperty("type")]
                public string Type { get; set; }
                [JsonProperty("note")]
                public string Note { get; set; }
            }

            [JsonProperty("list")]
            public EmailInfo[] Emails { get; set; }
        }

        public RestRequestPrivateEmailList(string sessionToken)
            : base(RestUriConsts.UriPrivateEmailList, HttpMethodsEnum.POST)
        {
            var jsonParams = new Dictionary<string, string> { { "session_token", sessionToken } };
            PostData = JsonConvert.SerializeObject(jsonParams);
        }
    }

    /// <summary>
    /// </summary>
    public class RestRequestPrivateEmailGenerate : IVPNRestRequest<RestRequestPrivateEmailGenerate.PrivateEmailGenerateResponse>
    {
        public class PrivateEmailGenerateResponse : ResponseWithStatus
        {
            [JsonProperty("generated")]
            public string Generated { get; set; }
            [JsonProperty("forwarded_to")]
            public string ForwardedTo { get; set; }
        }

        public RestRequestPrivateEmailGenerate(string sessionToken)
            : base(RestUriConsts.UriPrivateEmailGenerate, HttpMethodsEnum.POST)
        {
            var jsonParams = new Dictionary<string, string> { { "session_token", sessionToken } };
            PostData = JsonConvert.SerializeObject(jsonParams);
        }
    }

    /// <summary>
    /// </summary>
    public class RestRequestPrivateEmailUpdateNote : IVPNRestRequest<RestRequestPrivateEmailUpdateNote.PrivateEmailUpdateNoteResponse>
    {
        public class PrivateEmailUpdateNoteResponse : ResponseWithStatus { }

        public RestRequestPrivateEmailUpdateNote(string sessionToken, string email, string note)
            : base(RestUriConsts.UriPrivateEmailUpdateNote, HttpMethodsEnum.POST)
        {
            var jsonParams = new Dictionary<string, string> {
                { "session_token", sessionToken },
                { "email", email },
                { "note", note} };

            PostData = JsonConvert.SerializeObject(jsonParams);
        }
    }

    /// <summary>
    /// </summary>
    public class RestRequestPrivateEmailDelete : IVPNRestRequest<RestRequestPrivateEmailDelete.PrivateEmailDeleteResponse>
    {
        public class PrivateEmailDeleteResponse : ResponseWithStatus { }

        public RestRequestPrivateEmailDelete(string sessionToken, string email)
            : base(RestUriConsts.UriPrivateEmailDelete, HttpMethodsEnum.POST)
        {
            var jsonParams = new Dictionary<string, string> { { "session_token", sessionToken }, { "email", email } };
            PostData = JsonConvert.SerializeObject(jsonParams);
        }
    }
#endregion // Private Email

}
