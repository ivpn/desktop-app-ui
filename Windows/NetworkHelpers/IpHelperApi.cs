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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;

namespace NetworkHelpers
{
    public class IpHelperApi
    {
        public static NativeNetworkMethods.IPForwardTable ReadIPForwardTable(IntPtr tablePtr)
        {
            var result = (NativeNetworkMethods.IPForwardTable)Marshal.PtrToStructure(tablePtr, typeof(NativeNetworkMethods.IPForwardTable));

            NativeNetworkMethods.IPFORWARDROW[] table = new NativeNetworkMethods.IPFORWARDROW[result.Size];
            IntPtr p = new IntPtr(tablePtr.ToInt64() + Marshal.SizeOf(result.Size));
            for (int i = 0; i < result.Size; ++i)
            {
                table[i] = (NativeNetworkMethods.IPFORWARDROW)Marshal.PtrToStructure(p, typeof(NativeNetworkMethods.IPFORWARDROW));
                p = new IntPtr(p.ToInt64() + Marshal.SizeOf(typeof(NativeNetworkMethods.IPFORWARDROW)));
            }
            result.Table = table;

            return result;
        }

        public static RoutingTableEntry ParseIpForwardTableEntry(NativeNetworkMethods.IPFORWARDROW row)
        {
            return new RoutingTableEntry
            {
                destination = new IPAddress((long)row.dwForwardDest),
                subnetMask = new IPAddress((long)row.dwForwardMask),
                nextHop = new IPAddress((long)row.dwForwardNextHop),
                interfaceIndex = row.dwForwardIfIndex,
                type = row.dwForwardType,
                proto = row.dwForwardProto,
                age = row.dwForwardAge,
                metric = row.dwForwardMetric1
            };
        }

        public static RoutingTableEntry[] GetRoutingTable()
        {
            var fwdTable = IntPtr.Zero;
            int size = 0;
            var result = NativeNetworkMethods.GetIpForwardTable(fwdTable, ref size, true);
            fwdTable = Marshal.AllocHGlobal(size);

            result = NativeNetworkMethods.GetIpForwardTable(fwdTable, ref size, true);

            var forwardTable = ReadIPForwardTable(fwdTable);

            Marshal.FreeHGlobal(fwdTable);

            RoutingTableEntry[] entries = new RoutingTableEntry[forwardTable.Size];

            Debug.Print("{0,17}  {1,15}  {2,15}  {3,2}  {4,6}  {5}", "DESTINATION", "SUBNET MASK", "NEXT HOP", "IF", "AGE", "METRIC");
            for (int i = 0; i < forwardTable.Table.Length; ++i)
            {
                RoutingTableEntry row = entries[i] = ParseIpForwardTableEntry(forwardTable.Table[i]);
                Debug.Print("  {0,15}  {1,15}  {2,15}  {3,2}  {4,6}  {5}", row.destination, row.subnetMask, row.nextHop, row.interfaceIndex, row.age, row.metric);
            }

            return entries;
        }

        public static RoutingTableEntry GetBestRoute(IPAddress destination)
        {
            uint dwSourceAddr = BitConverter.ToUInt32(destination.GetAddressBytes(), 0);
            NativeNetworkMethods.IPFORWARDROW pBestRoute;

            uint ret = NativeNetworkMethods.GetBestRoute(dwSourceAddr, 0, out pBestRoute);
            if (ret == 0)
            {
                return ParseIpForwardTableEntry(pBestRoute);
            }
            else
            {
                return null;
            }
        }

        public static Process GetLocalConnectionProcess(int localPort)
        {
            //int nbLocalPort = IPAddress.HostToNetworkOrder(localPort);

            foreach (var tcpRow in GetAllTcpConnections())
            {
                byte[] b = BitConverter.GetBytes((short)tcpRow.localPort);
                if (BitConverter.IsLittleEndian)
                    b = b.Reverse().ToArray();

                int lport = BitConverter.ToUInt16(b, 0);

                //Debug.Print("TCP Connection: pid: {0}, local: {1}:{2} remote: {3}:{4}", tcpRow.owningPid, new IPAddress(tcpRow.localAddr), lport, new IPAddress(tcpRow.remoteAddr), tcpRow.remotePort);

                if (lport == localPort)
                    return Process.GetProcessById(tcpRow.owningPid);
            }

            return null;
        }

        // from http://www.pinvoke.net/default.aspx/iphlpapi.getextendedtcptable
        // WARNING: MIB_TCPROW_OWNER_PID fields are in network byte-order
        public static NativeNetworkMethods.MIB_TCPROW_OWNER_PID[] GetAllTcpConnections()
        {
            //  TcpRow is my own class to display returned rows in a nice manner.
            //    TcpRow[] tTable;
            NativeNetworkMethods.MIB_TCPROW_OWNER_PID[] tTable;
            int AF_INET = 2;    // IP_v4
            int buffSize = 0;

            // how much memory do we need?
            uint ret = NativeNetworkMethods.GetExtendedTcpTable(IntPtr.Zero, ref buffSize, true, AF_INET, NativeNetworkMethods.TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_CONNECTIONS, 0);
            IntPtr buffTable = Marshal.AllocHGlobal(buffSize);

            try
            {
                ret = NativeNetworkMethods.GetExtendedTcpTable(buffTable, ref buffSize, true, AF_INET, NativeNetworkMethods.TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_CONNECTIONS, 0);
                if (ret != 0)
                {
                    return null;
                }

                // get the number of entries in the table
                //MibTcpTable tab = (MibTcpTable)Marshal.PtrToStructure(buffTable, typeof(MibTcpTable));
                NativeNetworkMethods.MIB_TCPTABLE_OWNER_PID tab = (NativeNetworkMethods.MIB_TCPTABLE_OWNER_PID)Marshal.PtrToStructure(buffTable, typeof(NativeNetworkMethods.MIB_TCPTABLE_OWNER_PID));
                //IntPtr rowPtr = (IntPtr)((long)buffTable + Marshal.SizeOf(tab.numberOfEntries) );
                IntPtr rowPtr = (IntPtr)((long)buffTable + Marshal.SizeOf(tab.dwNumEntries));
                // buffer we will be returning
                //tTable = new TcpRow[tab.numberOfEntries];
                tTable = new NativeNetworkMethods.MIB_TCPROW_OWNER_PID[tab.dwNumEntries];

                //for (int i = 0; i < tab.numberOfEntries; i++)        
                for (int i = 0; i < tab.dwNumEntries; i++)
                {
                    //MibTcpRow_Owner_Pid tcpRow = (MibTcpRow_Owner_Pid)Marshal.PtrToStructure(rowPtr, typeof(MibTcpRow_Owner_Pid));
                    NativeNetworkMethods.MIB_TCPROW_OWNER_PID tcpRow = (NativeNetworkMethods.MIB_TCPROW_OWNER_PID)Marshal.PtrToStructure(rowPtr, typeof(NativeNetworkMethods.MIB_TCPROW_OWNER_PID));
                    //tTable[i] = new TcpRow(tcpRow);
                    tTable[i] = tcpRow;
                    rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf(tcpRow));   // next entry
                }

            }
            finally
            {
                // Free the Memory
                Marshal.FreeHGlobal(buffTable);
            }

            return tTable;
        }
    }
}
