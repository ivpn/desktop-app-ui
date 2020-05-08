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
