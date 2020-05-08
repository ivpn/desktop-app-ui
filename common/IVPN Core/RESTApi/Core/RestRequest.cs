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
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IVPN.RESTApi.Core
{
    /// <summary>
    /// REST request
    /// </summary>
    public class RestRequest
    {
        public enum HttpMethodsEnum
        {
            GET,
            POST,
            PUT,
            DELETE
        }

        public class HttpResponse
        {
            internal HttpResponse(HttpStatusCode httpRetCode, string data)
            {
                HttpRetCode = httpRetCode;
                Data = data;
            }
            public HttpStatusCode HttpRetCode { get; }
            public string Data { get; }
        }

        public Uri EndPoint { get; private set; }
        public HttpMethodsEnum Method { get; private set; }
        public string ContentType { get; private set; }
        public string PostData { get; protected set; }

        public RestRequest (string endpoint, HttpMethodsEnum method = HttpMethodsEnum.GET, string postData = "", string contentType = "application/json")
        {
            EndPoint = new Uri(endpoint);
            Method = method;
            PostData = postData;
            ContentType = contentType;
        }

        public virtual async Task<HttpResponse> RequestAsync (CancellationToken cancellationTocken, int timeoutMs, string userAgent, IPAddress hostIP = null)
        {
            DateTime startTime = DateTime.Now;

            // manually set TLS v1.2 for all HTTP requests in current application domain!
            if (ServicePointManager.SecurityProtocol < SecurityProtocolType.Tls12)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }

            HttpWebRequest request;
            if (hostIP!=null)
            {
                // if accessing API server by IP (not by DNS), we should set correct 'Host' for request
                // Otherwise - Sertificate check will fail
                var ep = EndPoint.ToString().Replace(EndPoint.Host, hostIP.ToString());
                request = (HttpWebRequest)WebRequest.Create(ep);
                request.Host = EndPoint.Host;
            }
            else
                request = (HttpWebRequest)WebRequest.Create (EndPoint);

            // It is important do not keep a connection alive!
            request.KeepAlive = false;

            request.Method = Method.ToString ();
            request.ContentLength = 0;
            request.ContentType = ContentType;

            if (!string.IsNullOrEmpty(userAgent))
                request.UserAgent = userAgent;

            try {
                using (cancellationTocken.Register (() => request.Abort (), useSynchronizationContext: false)) {
                    if (!string.IsNullOrEmpty (PostData) && Method == HttpMethodsEnum.POST) {
                        var bytes = Encoding.UTF8.GetBytes (PostData);
                        request.ContentLength = bytes.Length;

                        using (var writeStream = await TaskWithTimeout (request.GetRequestStreamAsync (), CalcTimeoutMsLeft (startTime, timeoutMs))) {
                            await TaskWithTimeoutNoResult (writeStream.WriteAsync (bytes, 0, bytes.Length), CalcTimeoutMsLeft (startTime, timeoutMs));
                        }
                    }

                    HttpResponse responseObj = null;

                    using (var response = (HttpWebResponse)await TaskWithTimeout (request.GetResponseAsync (), CalcTimeoutMsLeft (startTime, timeoutMs))) 
                    {
                        if (response.StatusCode != HttpStatusCode.OK) 
                        {
                            var message = String.Format ("Request failed. Received HTTP {0}", response.StatusCode);
                            throw new RestException (message);
                        }

                        // grab the response
                        using (var responseStream = response.GetResponseStream ()) 
                        {
                            if (responseStream != null) 
                            {
                                using (var reader = new StreamReader (responseStream)) 
                                {
                                    string responseValue = await TaskWithTimeout (reader.ReadToEndAsync (), CalcTimeoutMsLeft (startTime, timeoutMs));
                                    responseObj = new HttpResponse(response.StatusCode, responseValue);
                                }
                            }
                        }
                    }

                    return responseObj;
                }
            } 
            catch (TimeoutException) 
            {
                throw;
            }
            catch (Exception ex) 
            {
                if (cancellationTocken.IsCancellationRequested)
                    throw new OperationCanceledException ("REST API request cancelled", ex);

                if ((ex as WebException)?.Status == WebExceptionStatus.RequestCanceled)
                    throw new OperationCanceledException("REST API request cancelled", ex);

                var ret = TryGetHttpDataFromException(ex);
                if (ret != null)
                    return ret;

                throw;
            }
            finally
            {
                request.Abort();
            }
        }

        #region Helper functions
        private static HttpResponse TryGetHttpDataFromException(Exception ex)
        {
            WebException wex;

            if (ex is WebException)
                wex = ex as WebException;
            else
                wex = ex.InnerException as WebException;

            if (wex == null)
                return null;

            if (!(wex.Response is HttpWebResponse resp))
                return null;

            HttpStatusCode retHttpCode = resp.StatusCode;
            string respValue = null;
            using (var responseReader = new StreamReader(wex.Response.GetResponseStream()))
            {
                respValue = responseReader.ReadToEnd();
            }

            return new HttpResponse(retHttpCode, respValue);
        }

        private static int CalcTimeoutMsLeft (DateTime startTime, int totalTimeoutMs)
        {
            double elapsedMs = (DateTime.Now - startTime).TotalMilliseconds;
            double ret = totalTimeoutMs - elapsedMs;
            if (ret < 0)
                ret = 0;

            return (int)ret;
        }

        public static async Task<T> TaskWithTimeout<T> (Task<T> task, int duration)
        {
            if (duration <= 0)
                throw new TimeoutException ();

            if (await Task.WhenAny (task, Task.Delay (duration)) == task)
                return task.Result;

            throw new TimeoutException ();
        }

        public static async Task TaskWithTimeoutNoResult (Task task, int duration)
        {
            if (duration <= 0)
                throw new TimeoutException ();

            if (await Task.WhenAny (task, Task.Delay (duration)) != task)
                throw new TimeoutException ();
        }
        #endregion // Helper functions
    } 
}
