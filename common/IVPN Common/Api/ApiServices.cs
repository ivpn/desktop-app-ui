using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IVPN;
using IVPN.Exceptions;
using IVPN.Models;
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
        ISessionKeeper __sessionKeeper;

        // Alternate hostIPs (in use if DNS access is blocked)
        private readonly IVPNApiHostIPs __HostIPs = new IVPNApiHostIPs();
        public event IVPNApiHostIPs.AlternateHostChangedDelegate AlternateHostChanged = delegate { };
        public event IVPNApiHostIPs.AlternateHostsListUpdatedDelegate AlternateHostsListUpdated = delegate { };

        public void Initialize(IPAddress currentAlternateIp)
        {
            __sessionKeeper = AppState.Instance(); 
            __HostIPs.SetCurAlternateHost(currentAlternateIp);

            // reset alternate host on logout
            __sessionKeeper.OnSessionChanged += (SessionInfo sessionInfo) =>
            {
                if (!__sessionKeeper.IsLoggedIn())
                    ResetCurAlternateHost();
            };

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

        private string GetSession()
        {
            if (__sessionKeeper == null)
                throw new IVPNInternalException("ApiServices: SessionKeeper object not defined"); 

            string session = __sessionKeeper.Session.Session;

            if (string.IsNullOrEmpty(session))
                throw new RestException("(ApiServices) Unable to make API request: not logged in");

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

        /// <summary> </summary>
        public async Task<AccountStatus> SessionStatusAsync(CancellationToken cancellationToken,
                                                                 int timeoutMs = Consts.DefaultTimeout)
        {
            ServiceStatusResponse serviceStatus;

            string session = GetSession();
            var response = await DoRequestAsync(new RestRequestSessionStatus(session), cancellationToken, timeoutMs);
            serviceStatus = response.ServiceStatus;


            return new AccountStatus(
                serviceStatus.IsActive,
                IVPN_Helpers.DataConverters.DateTimeConverter.FromUnixTime(serviceStatus.ActiveUtil),
                serviceStatus.IsRenewable,
                serviceStatus.WillAutoRebill,
                serviceStatus.IsOnFreeTrial,
                serviceStatus.Capabilities);
        }

        #region Private emails
        public async Task<List<PrivateEmailInfo>> PrivateEmailListAsync(CancellationToken cancellationToken,
                                                                              int timeoutMs = Consts.DefaultTimeout)
        {
            List<PrivateEmailInfo> emails = new List<PrivateEmailInfo>();
            try
            {
                string session = GetSession();

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
                string session = GetSession();

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
                string session = GetSession();
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
                string session = GetSession();
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
