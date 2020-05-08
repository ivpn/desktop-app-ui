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
using System.Runtime.InteropServices;
using System.Threading;

namespace NetworkHelpers
{
    // portions from http://social.msdn.microsoft.com/Forums/en-US/csharpgeneral/thread/19a3ce21-e395-4151-86f6-723a64272f0d/
    // and http://social.msdn.microsoft.com/Forums/en-US/netfxnetcom/thread/b0e10230-d174-4dfe-92fc-899667c038ef/
    public class NativeNetworkMethods
    {
        [ComVisible(false), StructLayout(LayoutKind.Sequential)]
        public struct IPForwardTable
        {
            public uint Size;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public IPFORWARDROW[] Table;
        };

        [ComVisible(false), StructLayout(LayoutKind.Sequential)]
        public struct IPFORWARDROW
        {
            internal uint /*DWORD*/ dwForwardDest;
            internal uint /*DWORD*/ dwForwardMask;
            internal uint /*DWORD*/ dwForwardPolicy;
            internal uint /*DWORD*/ dwForwardNextHop;
            internal uint /*DWORD*/ dwForwardIfIndex;
            internal uint /*DWORD*/ dwForwardType;
            internal uint /*DWORD*/ dwForwardProto;
            internal uint /*DWORD*/ dwForwardAge;
            internal uint /*DWORD*/ dwForwardNextHopAS;
            internal uint /*DWORD*/ dwForwardMetric1;
            internal uint /*DWORD*/ dwForwardMetric2;
            internal uint /*DWORD*/ dwForwardMetric3;
            internal uint /*DWORD*/ dwForwardMetric4;
            internal uint /*DWORD*/ dwForwardMetric5;
        };

        // from http://www.pinvoke.net/default.aspx/iphlpapi.getextendedtcptable
        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPROW_OWNER_PID
        {
            public uint state;
            public uint localAddr;
            public uint localPort;
            public uint remoteAddr;
            public uint remotePort;
            public int owningPid;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPTABLE_OWNER_PID
        {
            public uint dwNumEntries;
            MIB_TCPROW_OWNER_PID table;
        }

        public enum TCP_TABLE_CLASS
        {
            TCP_TABLE_BASIC_LISTENER,
            TCP_TABLE_BASIC_CONNECTIONS,
            TCP_TABLE_BASIC_ALL,
            TCP_TABLE_OWNER_PID_LISTENER,
            TCP_TABLE_OWNER_PID_CONNECTIONS,
            TCP_TABLE_OWNER_PID_ALL,
            TCP_TABLE_OWNER_MODULE_LISTENER,
            TCP_TABLE_OWNER_MODULE_CONNECTIONS,
            TCP_TABLE_OWNER_MODULE_ALL
        }

        [DllImport("Iphlpapi.dll", CharSet = CharSet.Auto)]
        public static extern UInt32 NotifyRouteChange(
            ref IntPtr Handle,
            ref NativeOverlapped overlapped);

        [DllImport("Iphlpapi.dll", CharSet = CharSet.Auto)]
        public static extern int GetIpForwardTable(
            IntPtr pIpForwardTable,
            ref int pdwSize,
            bool bOrder);

        [DllImport("Iphlpapi.dll", CharSet = CharSet.Auto)]
        public static extern uint GetBestRoute(
            uint dwDestAddr,
            uint dwSourceAddr,
            out IPFORWARDROW pBestRoute);

        /*[DllImport("Ws2_32.dll")]
        public extern static IntPtr WSACreateEvent();

        [DllImport("Ws2_32.dll")]
        public extern static System.Int32 WSACloseEvent(IntPtr hEvent);*/

        [DllImport("Iphlpapi.dll", CharSet = CharSet.Auto)]
        public static extern uint GetExtendedTcpTable(
            IntPtr pTcpTable,
            ref int dwOutBufLen,
            bool sort,
            int ipVersion,
            TCP_TABLE_CLASS tblClass,
            int reserved);
    }
}
