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
using System.Text;

namespace IVPN.Helpers.OpenVPN
{
    public class OpenVPNConfigChecker
    { 
        static readonly HashSet<string> _safeParametersSet = new HashSet<string>(
            new string[] 
                {
                    "tap-sleep",
                    "replay-window",
                    "help",
                    "mssfix",
                    "mute",
                    "ifconfig-ipv6-pool",
                    "show-adapters",
                    "redirect-gateway",
                    "ifconfig",
                    "inactive",
                    "socks-proxy",
                    "config",
                    "ping-exit",
                    "ecdh-curve",
                    "dev-type",
                    "multihome",
                    "group",
                    "mtu-disc",
                    "no-name-remapping",
                    "ncp-disable",
                    "local",
                    "iroute-ipv6",
                    "auth-user-pass",
                    "dh",
                    "pull",
                    "pkcs11-id-management",
                    "allow-recursive-routing",
                    "disable-occ",
                    "redirect-private",
                    "inetd",
                    "persist-tun",
                    "ifconfig-nowarn",
                    "verb",
                    "block-outside-dns",
                    "client-cert-not-required",
                    "http-proxy",
                    "pkcs11-cert-private",
                    "client-disconnect",
                    "show-net-up",
                    "verify-client-cert",
                    "server",
                    "shaper",
                    "no-replay",
                    "machine-readable-output",
                    "explicit-exit-notify",
                    "tls-version-max",
                    "route-nopull",
                    "show-valid-subnets",
                    "tls-server",
                    "mktun",
                    "tls-timeout",
                    "status-version",
                    "server-bridge",
                    "pkcs12",
                    "lladdr",
                    "syslog",
                    "hand-window",
                    "nobind",
                    "reneg-sec",
                    "ccd-exclusive",
                    "socket-flags",
                    "show-ciphers",
                    "port",
                    "route-metric",
                    "persist-key",
                    "ping-timer-rem",
                    "dhcp-renew",
                    "learn-address",
                    "route-gateway",
                    "mode",
                    "compat-names",
                    "client-connect",
                    "rmtun",
                    "tmp-dir",
                    "persist-local-ip",
                    "ifconfig-push",
                    "tcp-nodelay",
                    "sndbuf",
                    "ping-restart",
                    "key",
                    "route-method",
                    "tun-mtu-extra",
                    "topology",
                    "persist-remote-ip",
                    "auth-user-pass-verify",
                    "reneg-bytes",
                    "http-proxy-option",
                    "connect-retry",
                    "cipher",
                    "push-reset",
                    "keysize",
                    "prng",
                    "auth",
                    "test-crypto",
                    "no-iv",
                    "user",
                    "dhcp-release",
                    "show-pkcs11-ids",
                    "errors-to-stderr",
                    "ncp-ciphers",
                    "dev-node",
                    "float",
                    "replay-persist",
                    "reneg-pkts",
                    "echo",
                    "opt-verify",
                    "show-engines",
                    "rcvbuf",
                    "keepalive",
                    "allow-pull-fqdn",
                    "service",
                    "pkcs11-pin-cache",
                    "cert",
                    "mark",
                    "secret",
                    "max-routes-per-client",
                    "register-dns",
                    "show-curves",
                    "ifconfig-noexec",
                    "proto-force",
                    "bcast-buffers",
                    "client-nat",
                    "resolv-retry",
                    "duplicate-cn",
                    "setenv-safe",
                    "tls-cipher",
                    "push-peer-info",
                    "server-ipv6",
                    "mlock",
                    "port-share",
                    "keying-material-exporter",
                    "tun-mtu",
                    "show-tls",
                    "key-method",
                    "remap-usr1",
                    "tls-client",
                    "ip-win32",
                    "static-challenge",
                    "comp-noadapt",
                    "mute-replay-warnings",
                    "show-gateway",
                    "passtos",
                    "dev",
                    "link-mtu",
                    "pkcs11-protected-authentication",
                    "ifconfig-ipv6",
                    "ifconfig-pool-persist",
                    "tls-version-min",
                    "client-to-client",
                    "use-prediction-resistance",
                    "route",
                    "fragment",
                    "tls-crypt",
                    "pkcs11-id",
                    "txqueuelen",
                    "route-noexec",
                    "auth-gen-token",
                    "comp-lzo",
                    "fast-io",
                    "ifconfig-ipv6-push",
                    "key-direction",
                    "genkey",
                    "max-clients",
                    "pkcs11-private-mode",
                    "connect-freq",
                    "rport",
                    "client-config-dir",
                    "push-remove",
                    "remote-random",
                    "ignore-unknown-option",
                    "verify-hash",
                    "ping",
                    "auth-retry",
                    "remote-random-hostname",
                    "setenv",
                    "tcp-queue-limit",
                    "extra-certs",
                    "allow-nonadmin",
                    "tls-cert-profile",
                    "nice",
                    "status",
                    "show-digests",
                    "hash-size",
                    "iroute",
                    "win-sys",
                    "connect-retry-max",
                    "compress",
                    "stale-routes-check",
                    "single-session",
                    "pkcs11-providers",
                    "disable",
                    "show-net",
                    "cryptoapicert",
                    "route-ipv6",
                    "suppress-timestamps",
                    "tls-exit",
                    "route-delay",
                    "tran-window",
                    "remote",
                    "ifconfig-pool-linear",
                    "bind",
                    "dhcp-option",
                    "proto",
                    "auth-user-pass-optional",
                    "ifconfig-pool",
                    "lport",
                    "client",
                    "username-as-common-name",
                    "pause-exit",
                    "push",
                    "setcon",
                    "mtu-test",

                    // UNSAFE COMMANDS
                    // "writepid", 		// unsafe 
                    // "http-proxy-user-pass", 		// not found in public documentation 
                    // "version", 		// not found in public documentation 
                    // "route-up", 		// unsafe 
                    // "ca", 		// unsafe 
                    // "mtu-dynamic", 		// not found in public documentation 
                    // "cd", 		// unsafe 
                    // "down", 		// unsafe 
                    // "iproute", 		// unsafe 
                    // "remote-cert-tls", 		// not found in public documentation 
                    // "ipchange", 		// unsafe 
                    // "udp-mtu", 		// not found in public documentation 
                    // "management*", 		// unsafe 
                    // "ip-remote-hint", 		// not found in public documentation 
                    // "socks-proxy-retry", 		// not found in public documentation 
                    // "rdns-internal", 		// not found in public documentation 
                    // "chroot", 		// unsafe 
                    // "tls-ciphersuites", 		// not found in public documentation 
                    // "log", 		// unsafe 
                    // "http-proxy-timeout", 		// not found in public documentation 
                    // "askpass", 		// not found in public documentation 
                    // "foreign-option", 		// not found in public documentation 
                    // "tls-verify", 		// unsafe 
                    // "script-security", 		// unsafe 
                    // "x509-track", 		// not found in public documentation 
                    // "memstats", 		// not found in public documentation 
                    // "log-append", 		// unsafe 
                    // "pull-filter", 		// not found in public documentation 
                    // "parameter", 		// not found in public documentation 
                    // "engine", 		// unsafe 
                    // "gremlin", 		// not found in public documentation 
                    // "max-routes", 		// not found in public documentation 
                    // "server-poll-timeout", 		// not found in public documentation 
                    // "tls-auth", 		// unsafe 
                    // "auth-nocache", 		// not found in public documentation 
                    // "tun-ipv6", 		// not found in public documentation 
                    // "preresolve", 		// not found in public documentation 
                    // "dhcp-pre-release", 		// not found in public documentation 
                    // "tls-export-cert", 		// not found in public documentation 
                    // "verify-x509-name", 		// not found in public documentation 
                    // "x509-username-field", 		// not found in public documentation 
                    // "ns-cert-type", 		// not found in public documentation 
                    // "show-proxy-settings", 		// not found openVPN sources 
                    // "push-continuation", 		// not found in public documentation 
                    // "http-proxy-retry", 		// not found in public documentation 
                    // "crl-verify", 		// not found in public documentation 
                    // "daemon", 		// unsafe 
                    // "peer-id", 		// not found in public documentation 
                    // "msg-channel", 		// not found in public documentation 
                    // "plugin", 		// unsafe 
                    // "ifconfig-push-constraint", 		// not found in public documentation 
                    // "remote-cert-eku", 		// not found in public documentation 
                    // "remote-cert-ku", 		// not found in public documentation 
                    // "up", 		// unsafe 
                    // "auth-token", 		// not found in public documentation 
                    // "connection", 		// not found in public documentation 
                    // "up-restart", 		// unsafe 
                    // "route-pre-down", 		// unsafe 
                    // "http-proxy-override", 		// not found in public documentation 
                    // "connect-timeout", 		// not found in public documentation 
                    // "dhcp-internal", 		// not found in public documentation 
            });


        /// <summary>
        /// Check if OpenVPN parameters defined by user are safe.
        /// All parameters which can execute external command - not allowed.
        /// </summary>
        /// <param name="userParameters">User-defined parameters</param>
        /// <param name="problemDescription">(out argument) If parameters not allowed - contains detailed description about deprecated parameter</param>
        /// <returns>true - if parameters are OK</returns>
        public static bool IsIsUserParametersAllowed(string userParameters, out string problemDescription)
        {
            problemDescription = "";
                                 
            if (string.IsNullOrEmpty(userParameters))
                return true;
            
            StringBuilder badParameters = new StringBuilder();
            int badParametersCnt = 0;

            var lines = userParameters.Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);

            var cmdSeparator = new char[] { ' ', '\t' };

            foreach (var line in lines)
            {
                var l = line.ToLower().Trim();
                if (string.IsNullOrEmpty(l))
                    continue;

                if (l.StartsWith("#", StringComparison.Ordinal) || l.StartsWith(";", StringComparison.Ordinal))
                    continue;

                var cmdArgs = l.Split(cmdSeparator);
                if (_safeParametersSet.Contains(cmdArgs[0]))
                    continue;

                badParametersCnt += 1;
                if (badParametersCnt < 6)
                    badParameters.Append("\n\t"+cmdArgs[0]);
                else if (badParametersCnt == 6)
                    badParameters.Append("\n\t...");
            }
            
            if (badParametersCnt > 0)
            {
                if (badParametersCnt > 1)
                    problemDescription = "Not supported OpenVPN parameters:\n" + badParameters + "\n\nPlease, check configuration. ";
                else
                    problemDescription = "Not supported OpenVPN parameter:\n" + badParameters + "\n\nPlease, check configuration. ";

                return false;
            }

            return true;
        }
    }
}
