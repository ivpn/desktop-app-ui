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
using Newtonsoft.Json;

namespace IVPN.VpnProtocols
{
    [Serializable]
    public abstract class VpnServerInfoBase
    {
        [JsonProperty("gateway")]
        public string GatewayId { get; set; }
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }

        public abstract List<string> GetHostsIpAddresses();
        public abstract bool IsContainHostIpAddress(string host);

        public override string ToString() { return FullName; }

        public string FullName => $"{City}, {CountryCode}";

        public string GetMultihopId()
        {
            string gwID = GatewayId;
            if (string.IsNullOrEmpty(gwID))
                return "";

            if (!gwID.Contains("."))
                return gwID;

            return gwID.Split(new char[] { '.' })[0];
        }
    }
}
