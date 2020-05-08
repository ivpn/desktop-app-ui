//
//  IVPN Client Desktop
//  https://github.com/ivpn/desktop-app-ui
//
//  Created by Stelnykovych Alexandr.
//  Copyright (c) 2020 Privatus Limited.
//
//  This file is part of the IVPN Client Desktop.
//
//  The IVPN Client Desktop is free software: you can redistribute it and/or
//  modify it under the terms of the GNU General Public License as published by the Free
//  Software Foundation, either version 3 of the License, or (at your option) any later version.
//
//  The IVPN Client Desktop is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
//  details.
//
//  You should have received a copy of the GNU General Public License
//  along with the IVPN Client Desktop. If not, see <https://www.gnu.org/licenses/>.
//

﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IVPN.RESTApi.Core;
using Newtonsoft.Json;

namespace IVPN.RESTApi
{
    public enum ApiStatusCode
    {
        NotDefined              = 0,
        Success                 = 200,

        BadRequest              = 400, // Bad Request (No or invalid JSON detected in request body)
        Unauthorized            = 401, // Invalid Credentials	(Username or Password is not valid)
        
        WgPublicKeyNotValid     = 422, // WireGuard Key Not Valid	(Public key is not valid. Key should be exactly 32 bytes base64 encoded.)
        WgPublicKeyAlreadyExists= 423, // WireGuard Key Already Exists	(Specified public key already exists.)
        WgPublicKeyNotFound     = 424, // WireGuard Key Not Found	(Public Key not found)
        WgPublicKeyLimitReached = 425, // WireGuard Key Limit Reached	(WireGuard key limit is reached)

        GeoLookupDbError        = 501, // GeoLookup DB Error (Error while connecting to GEO IP database)
        GeoLookupIpInvalid      = 502, // GeoLookup IP Invalid  (Invalid IP)
        GeoLookupIpNotFound     = 503, // GeoLookup IP Not Found (Error whilst finding city based on IP)

        SessionNotFound         = 601, // Session not found Session not found
        SessionTooManySessions  = 602, // Too many sessions Too many sessions

        // are in use? :
        
        Conflict            = 409,  // Account with this email already registered. Please Sign In instead
        NotFound            = 404,  // Service not found
        Forbidden           = 403,  // Account logged out

        UserNameNotFound    = 421,  // Username was not found while adding the key
        //WgUnknownError      = 429,  // Unknown error related to WireGuard key processing

        Internal            = 500   // Server Error   
    }

    public static class Consts
    {
        // Default request timeout
        public const int DefaultTimeout = -1;
        internal const int DefaultTimeoutValuetMs = 10000;
    }

    /// <summary>
    /// In case of an error, when HTTP Status is set to 400, additional field "message" is present in JSON response. This message can be used to be displayed in client app UI.
    /// In the future versions of API, these messages can be localized.
    ///
    /// For example, response in case of error may look like this:
    /// {
    ///     status: 423,
    ///     message: "Specified public key already exists."
    /// }
    /// </summary>
    public abstract class ResponseWithErrorMessage
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class ResponseWithStatus : ResponseWithErrorMessage
    {
        [JsonProperty("status", Required = Required.Always)]
        public ApiStatusCode Status { get; set; }
    }

    public class HttpResponseEx
    {
        internal HttpResponseEx(RestRequest.HttpResponse response, IPAddress hostIP)
        {
            HttpResponse = response;
            HostIP = hostIP;
        }
        public IPAddress HostIP { get; }
        public RestRequest.HttpResponse HttpResponse { get; }
    }

    /// <summary>
    /// Base class for all IVPN REST requests
    /// </summary>
    /// <typeparam name="TRetType"></typeparam>
    public abstract class IVPNRestRequest<TRetType> : RestRequest
    {
        // constructor
        protected IVPNRestRequest(string endpoint, HttpMethodsEnum method = HttpMethodsEnum.GET, string postData = "", string contentType = "application/json") : base(endpoint, method, postData, contentType)
        { }

        /// <summary>
        /// Request and get raw string data and status code
        /// 
        /// DNS-BE-100 Access to our API server have to be available using multiple IP addresses
        /// DNS-APP-100 Every call to IVPN API have to be done using DNS name first
        /// DNS-APP-200 If that call fails with connection error / timeout / TLS certificate error, connection with IP addresses have to be initiated
        /// DNS-APP-300 If the call fails with error result from the API, no retry have to be done
        /// DNS-APP-400 Calls to API server have to be retries using every IP address provided in the configuration section of /servers.json file.
        /// DNS-APP-500 IVPN client app should store the IP address of the last successful API request and use it in any subsequent calls for the first try
        /// </summary>
        public async Task<HttpResponseEx> RequestAsync(List<IPAddress> alternateHostIPs, IPAddress defaultAlternateHostIP, CancellationToken cancellationToken, int timeoutMs = Consts.DefaultTimeout)
        {
            try
            {
                bool isTimeoutDefined = true;

                if (timeoutMs == Consts.DefaultTimeout)
                {
                    isTimeoutDefined = false;
                    timeoutMs = Consts.DefaultTimeoutValuetMs;
                }

                int timeoutMsArg = timeoutMs;
                DateTime startTime = DateTime.Now;
                
                // CURRENT ALTERNATE HOST
                // DNS-APP-500 IVPN client app should store the IP address of the last successful API request and use it in any subsequent calls for the first try
                if (defaultAlternateHostIP != null)
                {
                    try
                    {
                        var ret = await DoRequestAsyncRaw(cancellationToken, timeoutMs, defaultAlternateHostIP);
                        return new HttpResponseEx(ret, defaultAlternateHostIP);
                    }
                    catch (Exception ex)
                    {
                        if (!IsNetworkError(ex))
                            throw;

                        if (isTimeoutDefined)
                        {
                            var elapsedMs = (DateTime.Now - startTime).TotalMilliseconds;
                            if (elapsedMs > timeoutMsArg)
                                throw;
                            timeoutMs = timeoutMsArg - (int)elapsedMs;
                        }
                    }
                }       

                Exception retException;
                // DEFAULT DNS REQUEST
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var ret = await DoRequestAsyncRaw(cancellationToken, timeoutMs, null);
                    return new HttpResponseEx(ret, null);
                }
                catch (Exception ex)
                {
                    if (!IsNetworkError(ex))
                        throw;

                    if (isTimeoutDefined)
                    {
                        var elapsedMs = (DateTime.Now - startTime).TotalMilliseconds;
                        if (elapsedMs > timeoutMsArg)
                            throw;
                        timeoutMs = timeoutMsArg - (int)elapsedMs;
                    }

                    retException = ex;
                }

                // LIST OF ALTERNATE HOST IPs

                // DNS-APP-200 If that call fails with connection error / timeout / TLS certificate error, connection with IP addresses have to be initiated
                // DNS-APP-400 Calls to API server have to be retries using every IP address provided in the configuration section of / servers.json file.
                foreach (IPAddress hostIP in alternateHostIPs)
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var ret = await DoRequestAsyncRaw(cancellationToken, timeoutMs, hostIP);
                        return new HttpResponseEx(ret, hostIP);
                    }
                    catch (Exception ex)
                    {
                        if (!IsNetworkError(ex))
                            throw;

                        if (isTimeoutDefined)
                        {
                            var elapsedMs = (DateTime.Now - startTime).TotalMilliseconds;
                            if (elapsedMs > timeoutMsArg)
                                throw;
                            timeoutMs = timeoutMsArg - (int)elapsedMs;
                        }
                    }
                }

                throw retException;
            }
            catch (Exception ex)
            {
                Logging.Info($"[REST ERROR] {ex.Message}");
                throw;
            }
        }

        #region Private methods
        /// <summary>Rrequest and get raw string data and status code</summary>        
        private async Task<HttpResponse> DoRequestAsyncRaw(CancellationToken cancellationToken, int timeoutMs = Consts.DefaultTimeout, IPAddress hostIP = null)
        {
            if (timeoutMs == Consts.DefaultTimeout)
                timeoutMs = Consts.DefaultTimeoutValuetMs;

            try
            {
                HttpResponse ret = await base.RequestAsync(cancellationToken, timeoutMs, Platform.HttpUserAgent, hostIP);
                if (ret == null)
                    throw new RestException("Empty response from server");

                return ret;
            }
            catch (RestException)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (TimeoutException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (ex is AggregateException aggr && aggr.InnerExceptions != null && aggr.InnerExceptions.Count == 1)
                    throw aggr.InnerExceptions[0];

                throw new RestException("Error getting response from server", ex);
            }
        }

        private bool IsNetworkError(Exception ex)
        {
            if (ex is TimeoutException)
                return true;
            if (ex is WebException)
                return true;
            if (ex is RestException)
                return true;
            return false;
        }
        #endregion //Private methods
    }    
}
