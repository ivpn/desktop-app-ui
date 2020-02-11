using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IVPN;
using IVPN.Exceptions;
using IVPN.Models.PrivateEmail;
using IVPN.Models.Session;
using IVPN.RESTApi;
using IVPN.RESTApi.Core;
using Newtonsoft.Json;

namespace IVPNCommon.Api
{
    public class ApiServices
    {
        public const int DefaultTimeout = Consts.DefaultTimeout;

        // Singleton
        private ApiServices() { }
        public static ApiServices Instance { get; } = new ApiServices();

        // Credentials (Session or user login/pass)
        IVPN.Models.Configuration.ICredentials __Credentials;

        // Alternate hostIPs (in use if DNS access is blocked)
        private readonly IVPNApiHostIPs __HostIPs = new IVPNApiHostIPs();
        public event IVPNApiHostIPs.AlternateHostChangedDelegate AlternateHostChanged = delegate { };
        public event IVPNApiHostIPs.AlternateHostsListUpdatedDelegate AlternateHostsListUpdated = delegate { };

        public void Initialize(IVPN.Models.Configuration.ICredentials credentials, IPAddress currentAlternateIp)
        {
            __Credentials = credentials;
            __HostIPs.SetCurAlternateHost(currentAlternateIp);

            // forward event
            __HostIPs.AlternateHostChanged += (IPAddress ip) =>
            {
                AlternateHostChanged(ip);
            };

            __HostIPs.AlternateHostsListUpdated += () =>
            {
                AlternateHostsListUpdated();
            };
        }

        public void SetAlternateHostIPs(List<string> ipList)
        {
            __HostIPs.SetAlternateHostIPs(ipList);
        }

        public void ResetCurAlternateHost()
        {
            __HostIPs.SetCurAlternateHost(null);
        }

        public IPAddress GetCurrAlternateIP()
        {
            __HostIPs.GetHostsInfo(out var iPAddress, out _);
            return iPAddress;
        }

        #region Private functionality

        private string CheckCredentials()
        {
            if (__Credentials == null)
                throw new IVPNInternalException("ApiServices: Credentials object not defined"); 

            string session = __Credentials.GetSessionToken();

            if (string.IsNullOrEmpty(session))
                throw new RestException("(ApiServices) Unable to make API request: credentials are empty");

            return session;
        }

        private async Task<T> DoRequestAsync<T>(IVPNRestRequest<T> req, CancellationToken cancellationToken, int timeoutMs = Consts.DefaultTimeout)
        {
            try
            {
                __HostIPs.GetHostsInfo(out IPAddress alternateHost, out List<IPAddress> alternateHostIPs);

                // do API call
                HttpResponseEx ret = await req.RequestAsync(alternateHostIPs, alternateHost, cancellationToken, timeoutMs);
                var httpResp = ret.HttpResponse;


                // deserialize response
                //
                // Here we are not checking HTTP code from response. 
                // There is a chance to receive API response with HTTPRetCode != OK (200). 'ResponseWithStatus' object
                T respObj;
                try
                {
                    respObj = JsonConvert.DeserializeObject<T>(httpResp.Data);
                }
                catch (Exception ex)
                {
                    Logging.Info($"[ERROR] (API) Response deserialization error: {ex.Message} (HTTP={(int)httpResp.HttpRetCode}-{httpResp.HttpRetCode})");
                    throw new IVPNRestRequestApiException(httpResp.HttpRetCode);                    
                }
                //if (respObj == default)
                if (respObj==null || respObj.Equals(default)) 
                {
                    Logging.Info($"[ERROR] (API) Deserialized response object is empty (HTTP={(int)httpResp.HttpRetCode}-{httpResp.HttpRetCode})");
                    throw new IVPNRestRequestApiException(httpResp.HttpRetCode);                    
                }
                
                // check is response OK (for ResponseWithStatus objects)
                ResponseWithStatus respWithStatus = respObj as ResponseWithStatus;
                if (respWithStatus != null && respWithStatus.Status != ApiStatusCode.Success)
                    throw new IVPNRestRequestApiException(httpResp.HttpRetCode, respWithStatus.Status, respWithStatus.Message, respWithStatus);

                // if we are here - ensure that HTTP response code is OK (200)
                if (httpResp.HttpRetCode != HttpStatusCode.OK)
                    throw new IVPNRestRequestApiException(httpResp.HttpRetCode);

                // Save last valid host IP
                __HostIPs.SetCurAlternateHost(ret.HostIP);

                return respObj;
            }
            catch (Exception ex)
            {
                Logging.Info($"REST request '{req.GetType().Name}' error:{ex.Message}");
                throw;
            }
        }       
        #endregion // Private functionality

        #region General
        public async Task<ApiResponseGeoLookup> GeoLookup(CancellationToken cancellationToken, int timeoutMs = Consts.DefaultTimeout)
        {
            RestRequestGeoLookup.GeoLookupResponse response;
            try
            {
                response = await DoRequestAsync(new RestRequestGeoLookup(), cancellationToken, timeoutMs);
            }
            catch (Exception ex)
            {
                Logging.Info($"REST request error: {ex}");
                throw;
            }

            return new ApiResponseGeoLookup(response.IpAddress, response.CountryCode, response.Country, response.City, response.IsIvpnServer);
        }

        #endregion // General

        #region Sessions
        /// <summary> </summary>
        public async Task<ApiSessionStatusAuthenticate> SessionNewAsync(string username,
                                                                            string password,
                                                                            CancellationToken cancellationToken,
                                                                            bool isForceDeleteAllSessions = false,
                                                                            string wireguardPublicKey = null,
                                                                            int timeoutMs = Consts.DefaultTimeout)
        {
            var resp = await DoRequestAsync(new RestRequestSessionNew(username, password, isForceDeleteAllSessions, wireguardPublicKey), cancellationToken, timeoutMs);

            SessionStatus account = new SessionStatus(                    
                    resp.ServiceStatus.IsActive,
                    IVPN_Helpers.DataConverters.DateTimeConverter.FromUnixTime(resp.ServiceStatus.ActiveUtil),
                    resp.ServiceStatus.IsRenewable,
                    resp.ServiceStatus.WillAutoRebill,
                    resp.ServiceStatus.IsOnFreeTrial,
                    resp.ServiceStatus.Capabilities);

            IPAddress wgIP = null;
            if (resp.WireGuard != null && !string.IsNullOrEmpty(resp.WireGuard.IpAddress))
                IPAddress.TryParse(resp.WireGuard.IpAddress, out wgIP);

            return new ApiSessionStatusAuthenticate(resp.SessionToken, resp.VpnUsername, resp.VpnPassword, account, wgIP);
        }

        /// <summary> </summary>
        public async Task<SessionStatus> SessionStatusAsync(CancellationToken cancellationToken,
                                                                 int timeoutMs = Consts.DefaultTimeout)
        {
            ServiceStatusResponse serviceStatus;

            string session = CheckCredentials();
            var response = await DoRequestAsync(new RestRequestSessionStatus(session), cancellationToken, timeoutMs);
            serviceStatus = response.ServiceStatus;
            

            return new SessionStatus(
                serviceStatus.IsActive,
                IVPN_Helpers.DataConverters.DateTimeConverter.FromUnixTime(serviceStatus.ActiveUtil),
                serviceStatus.IsRenewable,
                serviceStatus.WillAutoRebill,
                serviceStatus.IsOnFreeTrial,
                serviceStatus.Capabilities);
        }

        /// <summary> </summary>
        public async Task SessionDeleteAsync(CancellationToken cancellationToken,
                                                            int timeoutMs = Consts.DefaultTimeout)
        {
            try
            {
                string session = CheckCredentials();
                if (string.IsNullOrEmpty(session))
                    throw new RestException("(ApiServices) Unable to delete session. Session is not defined.");
                
                await DoRequestAsync(new RestRequestSessionDelete(session), cancellationToken, timeoutMs);
            }
            catch (Exception ex)
            {
                Logging.Info($"REST request error: {ex}");
                throw;
            }
        }
        #endregion //Sessions

        #region WireGuard

        public async Task<IPAddress> WireguardKeySet(string publicKey, string old_key,
                                                                   CancellationToken cancellationToken,
                                                                   int timeoutMs = Consts.DefaultTimeout)
        {
            string session = CheckCredentials();

            if (string.IsNullOrEmpty(session))
                throw new IVPNInternalException("Unable to perform API call 'WireguardKeySet'. Session is not defined");

            var resp = await DoRequestAsync(new RestRequestWireGuardKeySet(session, publicKey, old_key), cancellationToken, timeoutMs);
            return IPAddress.Parse(resp.IpAddress);
        }

        #endregion //WireGuard

        #region Private emails
        public async Task<List<PrivateEmailInfo>> PrivateEmailListAsync(CancellationToken cancellationToken,
                                                                              int timeoutMs = Consts.DefaultTimeout)
        {
            List<PrivateEmailInfo> emails = new List<PrivateEmailInfo>();
            try
            {
                string session = CheckCredentials();

                var response = await DoRequestAsync(new RestRequestPrivateEmailList(session), cancellationToken, timeoutMs);
                foreach (var email in response.Emails)
                    emails.Add(new PrivateEmailInfo(email.Email, "", email.Note));
            }
            catch (Exception ex)
            {
                Logging.Info($"REST request error: {ex}");
                throw;
            }

            return emails;
        }

        public async Task<PrivateEmailInfo> PrivateEmailGenerateAsync(CancellationToken cancellationToken,
                                                                                      int timeoutMs = Consts.DefaultTimeout)
        {
            try
            {
                string session = CheckCredentials();

                var resp = await DoRequestAsync(new RestRequestPrivateEmailGenerate(session), cancellationToken, timeoutMs);
                return new PrivateEmailInfo(resp.Generated, resp.ForwardedTo, "");

            }
            catch (Exception ex)
            {
                Logging.Info($"REST request error: {ex}");
                throw;
            }
        }

        public async Task PrivateEmailUpdateNoteAsync(string email,
                                                                     string notes,
                                                                     CancellationToken cancellationToken,
                                                                     int timeoutMs = Consts.DefaultTimeout)
        {
            try
            {
                string session = CheckCredentials();
                await DoRequestAsync(new RestRequestPrivateEmailUpdateNote(session, email, notes), cancellationToken, timeoutMs);
            }
            catch (Exception ex)
            {
                Logging.Info($"REST request error: {ex}");
                throw;
            }
        }

        public async Task<ApiStatusCode> PrivateEmailDeleteAsync(string email,
                                                                 CancellationToken cancellationToken,
                                                                 int timeoutMs = Consts.DefaultTimeout)
        {
            try
            {
                string session = CheckCredentials();
                var resp = await DoRequestAsync(new RestRequestPrivateEmailDelete(session, email), cancellationToken, timeoutMs);
                return resp.Status;
            }
            catch (Exception ex)
            {
                Logging.Info($"REST request error: {ex}");
                throw;
            }
        }
#endregion //Private emails

        
    }    
}
