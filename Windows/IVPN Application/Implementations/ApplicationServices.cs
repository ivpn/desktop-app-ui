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

﻿using IVPN.Interfaces;
using IVPN.Models;
using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace IVPN
{
    public class ApplicationServices: IApplicationServices
    {
        private const string IVPN_WINDOWS_SERVICE_NAME = "IVPN Client";

        private readonly ILocalizedStrings __LocalizedStrings;

        public ApplicationServices(ILocalizedStrings localizedStrings)
        {
            __LocalizedStrings = localizedStrings;
        }

        public string LocalizedString(string key, string defaultText = null)
        {
            return __LocalizedStrings.LocalizedString(key, defaultText);
        }

        public bool IsExiting => App.IsExiting;

        public bool IsServiceExists
        {
            get
            {
#if DEBUG
                return true;
#else
                ServiceController[] services = ServiceController.GetServices();
                var service = services.FirstOrDefault(s => s.ServiceName == IVPN_WINDOWS_SERVICE_NAME);
                return service != null;
#endif
            }
        }

        public bool IsServiceStarted
        {
            get {
#if DEBUG
                    return true;
#else
                ServiceController sc = new ServiceController(IVPN_WINDOWS_SERVICE_NAME);

                return sc.Status == ServiceControllerStatus.Running;
#endif
            }
        }
        
        public async Task<ServiceStartResult> StartService()
        {
            Exception exceptionOnServiceStart = null;
            await Task.Run(() =>
            {
                ServiceController sc = new ServiceController(IVPN_WINDOWS_SERVICE_NAME);

                try
                {
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                }
                catch (Exception ex)
                {
                    exceptionOnServiceStart = ex;
                }
            });

            if (exceptionOnServiceStart != null)
            {
                return new ServiceStartResult(true, exceptionOnServiceStart.Message);
            }

            return new ServiceStartResult(false);
        }

        public async Task<ServiceAttachResult> AttachToService()
        {
            // On Windows, IVPN Client Proxy knows about the port used to connect to service
            // So we just can pass -1 here, and let the IVPNClientProxy figure it out by itself
            return await Task.Run(() => new ServiceAttachResult(-1, 0));
        }
    }
}
