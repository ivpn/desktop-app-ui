# Changelog

All notable changes to this project will be documented in this file.

###Version 2.11.2 - 2020-02-20
[IMPROVED] WireGuard upgraded to 0.0.38
[IMPROVED] Wintun upgraded to v0.8
[FIXED] Unable to connect WireGuard if its service not uninstalled
[FIXED] Issue with background update of WireGuard keys
[FIXED] Firewall config changes from Always-On to On-Demand after upgrade
[FIXED] Processing of users additional OpenVPN parameters

###Version 2.11.0 - 2020-01-28
[IMPROVED] Improved performance of IVPN Service

##Version 2.10.9 - 2019-12-12

[IMPROVED] Login screen
[IMPROVED] OpenVPN upgraded to v2.4.8
[IMPROVED] OpenSSL upgraded to v1.1.1d
[IMPROVED] Overall stability
[FIXED] Potential OpenSSL privilege escalation via malicious engine loading
[FIXED] Potential privilege escalation vulnerability by improving OpenVPN extra parameter filtering

##Version 2.10.8 - 2019-12-04

[FIXED] Potential privilege escalation vulnerability by improving OpenVPN extra parameter filtering

##Version 2.10.7 - 2019-12-02

[FIXED] Various UI issues

##Version 2.10.6 - 2019-11-29

[IMPROVED] All API calls to IVPN servers using TLS v1.2. Please note: In-app updates will fail after 01.01.2020 if you do not install this release.
[IMPROVED] Installer is signed with a timestamp

##Version 2.10.4 - 2019-11-12

[NEW] Bypass DNS blocks to IVPN API
[IMPROVED] WireGuard upgraded to v0.0.31
[IMPROVED] Wintun upgraded to v0.7
[IMPROVED] Option to use custom DNS from local network
[IMPROVED] Login session management
[FIXED] Incorrect 'fastest server' detection after system reboot

##Version 2.10.3 - 2019-10-01

[FIXED] Possibility of DNS leak when 'Allow LAN traffic' is enabled
[FIXED] WireGuard sometimes loses connectivity after PC wakes up

##Version 2.10.2 - 2019-09-03

[IMPROVED] WireGuard upgraded to v0.0.23
[IMPROVED] Wintun upgraded to v0.6
[FIXED] Various installer issues
[FIXED] Save/restore selected server for WireGuard

##Version 2.10.1 - 2019-07-15

[NEW] Added new port for connection: 1194 UDP
[IMPROVED] Overall stability
[FIXED] OpenVPN re-connecting sometimes on slow connections
[FIXED] DNS issue with Multi-Hop connection when AntiTracker enabled
[FIXED] Various UI issues

##Version 2.10.0 - 2019-07-08

[NEW] WireGuard protocol - WireGuard is a new VPN protocol that promises better security and faster speeds compared to existing solutions like OpenVPN or IPSec. The WireGuard protocol is currently under heavy development and should be considered as experimental. Review the WireGuard project for more information. We do not recommend WireGuard except for situations where security is not critical.

##Version 2.9.9 - 2019-06-14

[FIXED] DNS switching problem
[FIXED] Failure to submit error reports
[FIXED] Various UI issues

##Version 2.9.8 - 2019-06-11

[NEW] AntiTracker: block ads, malicious websites, and third-party trackers
[NEW] Custom DNS: specify DNS server when connected to VPN

##Version 2.9.7 - 2019-03-21

[NEW] Ability to see current public IP and geolocation information
[NEW] Fastest server configuration
[NEW] Reset settings to defaults on user logout
[NEW] Setting to not to show disconnection dialog when application closing in the connected state
[NEW] Notification when IVPN service not installed
[IMPROVED] OpenVPN upgraded to v2.4.6
[IMPROVED] OpenSSL upgraded to v1.1.1a
[IMPROVED] Uninstaller is closing 'IVPN Client' application
[FIXED] Sometimes IVPN client reconnects without any reason for it
[FIXED] Account status check
[FIXED] Crash when user settings are broken
[FIXED] Windows firewall exception 'Cannot abort a WFP Transaction'
[FIXED] Hi CPU usage after the first LogIn
[FIXED] Various UI issues

##Version 2.9.6 - 2019-01-15

[IMPROVED] Overall stability
[FIXED] Various UI issues
[FIXED] UI issues on Windows 7

##Version 2.9.5 - 2018-12-19

[IMPROVED] Overall stability
[FIXED] Various UI issues

##Version 2.9.4 - 2018-12-17

[IMPROVED] Added possibility to change server without disconnect first
[IMPROVED] Overall stability

##Version 2.8.10 - 2018-10-31

[IMPROVED] Overall stability
[FIXED] Potential privilege escalation vulnerability by improving OpenVPN extra parameter filtering
[FIXED] Various UI issues

##Version 2.8.6 - 2018-08-28

[IMPROVED] 'Pause VPN' feature does not affect 'Always-on Firewall' configuration

##Version 2.8.4 - 2018-08-22

[IMPROVED] Overall stability

##Version 2.8.3 - 2018-08-20

[NEW] Ability to temporary pause VPN connection
[IMPROVED] Overall stability
[FIXED] Problems with connection on ports: UDP 2050 and TCP 1443
[FIXED] Various UI issues

##Version 2.8.2 - 2018-07-27

[IMPROVED] General performance and stability
[FIXED] Various UI issues

##Version 2.8.1 - 2018-07-24

[IMPROVED] General performance and stability

##Version 2.8.0 - 2018-07-23

[NEW] Ability to assign trust status and create connection rules for WiFi networks
[IMPROVED] Minor interface improvements
[FIXED] A bug that prevented automatic connection on startup for some users

##Version 2.7.9 - 2018-06-18

[NEW] Automatic selection of fastest server
[NEW] Added new TCP/UDP ports for connection
[IMPROVED] Minor interface updates

##Version 2.7.8 - 2018-06-04

[IMPROVED] Minor interface updates

##Version 2.7.7 - 2018-05-22

[NEW] Ability to connect to VPN when API server is not reachable
[IMPROVED] General performance and stability

##Version 2.7.6.1 - 2018-04-24

[FIXED] Installer for Windows 7

##Version 2.7.6 - 2018-04-20

[IMPROVED] OpenVPN upgraded to v2.4.5
[IMPROVED] OpenSSL upgraded to v1.1.0h
[IMPROVED] Updated installer for Windows 7
[IMPROVED] UI improvements
[IMPROVED] General performance and stability

##Version 2.7.5 - 2018-04-02

[IMPROVED] OpenSSL upgraded to 1.0.2o

##Version 2.7.4 - 2018-03-22

[NEW] Setting to automatically stop IVPN Windows service when not in use
[FIXED] Various UI issues

##Version 2.7.3 - 2018-03-13

[IMPROVED] UI improvements

##Version 2.7.2 - 2018-03-06

[IMPROVED] General performance and stability

##Version 2.7.1 - 2018-02-26

[IMPROVED] General performance and stability
[FIXED] Various UI issues

##Version 2.7 - 2018-02-13

[NEW] New UI that makes the set up and first connection process more intuitive
[NEW] New multihop UI
[IMPROVED] Consistency across desktop clients and mobile apps
[IMPROVED] The client will now automatically select a port if the default port is blocked
[IMPROVED] General performance and stability
[FIXED] A problem causing connection over port UDP 53 to be unstable

##Version 2.6.7 - 2017-10-18

[NEW] Added new TAP driver interface to avoid compatibility problems with other VPN clients
[NEW] Added ability to override basic OpenVPN parameters with user-defined parameters
[IMPROVED] Updated OpenVPN parameters according to ‘Deprecated options’ list
[IMPROVED] Reduced CPU usage relating to GUI
[FIXED] Fixed authentication error due to specific symbols present in password
[FIXED] Fix for empty window shown on Alt+Tab
[FIXED] Various stability fixes

##Version 2.6.6 - 2017-10-05

[NEW] Added option to specify additional OpenVPN parameters
[NEW] Added new new update framework to ease future upgrades
[NEW] Implemented IVPN Firewall notification windows
[FIXED] Fixed error: "Could not read Auth username/password/ok string from management interface" on connect
[FIXED] Various stability fixes
[IMPROVED] OpenVPN upgraded to v2.4.4

##Version 2.6.4 - 2017-02-24

[IMPROVED] Improved stability of communication protocol between OpenVPN process and desktop client
[IMPROVED] Updated software license agreement

##Version 2.6.3 - 2017-02-07

[IMPROVED] Improved communication protocol between local service and desktop client for improved stability
[FIXED] Fix for rare freezes during connection process to unresponsive servers
[FIXED] Fix for the client auto-starting even when auto-start is disabled in Settings
[NEW] Implemented additional protection for user credentials
[FIXED] Fix for client is unable to start minimized in some configurations

##Version 2.6.2 - 2017-01-16

[FIXED] Fix privilege escalation issue in user mode
[IMPROVED] Improved stability

##Version 2.6.1 - 2016-10-03

[NEW] New setting ‘Allow Multicast when LAN traffic is allowed’ added. This option allows access to more LAN resources while IVPN Firewall is active
[IMPROVED] OpenVPN upgraded to v2.3.12
[FIXED] Minor bug fixes related to IVPN service

##Version 2.6 - 2016-02-11

[IMPROVED] Improved error handling.
[IMPROVED] Disable client diagnostic logging by default.

##Version 2.5 - 2016-12-07

[FIXED] Minor bug fixes relating to server selection windows.

##Version 2.4 - 2015-11-26

[NEW] Full multi-hop support on all servers.
[IMPROVED] OpenVPN upgraded to 2.3.8.
[IMPROVED] Improved UI.
[IMPROVED] Full compatibility with Windows 10.
[IMPROVED] Obfsproxy updated to support obfs3.
[FIXED] Fixed multiple bugs relating to power event changes and switching Wifi networks.
[IMPROVED] Diagnostics reports upload speed improved.

##Version 2.3 - 2015-05-13

[FIXED] Fix port forwarding issue when IVPN firewall is enabled.

##Version 2.2 - 2015-03-11

[NEW] Ping time is now displayed for every server in the servers selection page.
[FIXED] Fix for the issue which caused application to crash on start on slow or highly loaded computers.
[FIXED] Fix for the issue which caused application to crash on Windows Vista with 'Allow LAN traffic' option enabled.
[FIXED] Fix for the client crashing when using multi-hop servers on some computers with non-English locales.
[IMPROVED] Improved system tray notification.

##Version 2.1 - 2015-02-12

[FIXED] Fix DNS leak when 'Allow LAN traffic' option is enabled.

##Version 2.0 - 2015-01-27

[NEW] New IVPN Firewall.
[NEW] Reworked UI interface.
[IMPROVED] Improved overall stability of the application and service.
[FIXED] Fix for when IVPN Client crashes after waking from Sleep/Hibernate on some devices.
[FIXED] Fix for when the VPN tunnel becomes unreachable after connecting to different WiFi or wired network.
[NEW] IPv6 traffic is now blocked when IVPN Firewall is enabled.
[FIXED] Fix for when client crashes where configuration file is broken.
[NEW] IVPN Client will now ask you to send a crash report if software has crashed.
[IMPROVED] Settings window is optimized.
[IMPROVED] Application Installer is improved and signed.
[NEW] .NET Framework 4.5 is now required to run the application. It will be automatically downloaded and installed if required.

##Version 1.0.8 - 2014-08-19

[FIXED] Minor bug fixes to improve stability of UI.

##Version 1.0.7 - 2014-07-03

[IMPROVED] Improve stability of OBFSproxy.

##Version 1.0.6 - 2014-05-30

[IMPROVED] Improve stablity of DNS leak protection code.

##Version 1.0.5 - 2014-05-20

[IMPROVED] Minor UI text updates.

##Version 1.0.4 - 2014-05-15

[IMPROVED] Ensure routes are correctly installed when using a proxy.

##Version 1.0.3 - 2014-05-08

[IMPROVED] Resolve authentication error message (status -1).

##Version 1.0.2 - 2014-04-15

[FIXED] Fix bug with Windows 8.1 to ensure that DNS search list is correctly modified on shutdown.

##Version 1.0.1 - 2014-04-09

[IMPROVED] Update OpenVPN to 2.3.3 due to recent OpenSSL Heartbleed vulnerability.

##Version 0.9 - 2014-03-28

[FIXED] Fix a bug where the uninstaller wouldn't completely remove itself from the registry.

##Version 0.8 - 2014-02-20

[IMPROVED] Improvements to routing table monitoring code.
[NEW] Hide multihop servers when OBFS proxy is enabled.
[NEW] Update IVPN Service to launch on-demand.
[NEW] Update authenticate checking code.
[IMPROVED] Improve cold startup time on windows 8.
[FIXED] Fix crash when clicking on a non-URL notification.
[NEW] Add software update notifications.
[NEW] Respect VPN-provided DNS servers by setting up a new DNS resolver.
[IMPROVED] Improvements to ensure when DHCP is renewed, the VPN-provided DNS servers are maintained.

##Version 0.7 - 2014-02-10

[NEW] Add diagnostics report.
[NEW] Add DNS leak prevention.
[IMPROVED] Several performance enhancements.

##Version 0.6 - 2014-02-05

[NEW] Minor UI improvements.
[NEW] Add connection duration.
[NEW] Add authentication error message.
[NEW] Add version number to tray icon.

##Version 0.5 - 2014-01-06

[IMPROVED] Resolve client hanging on startup issues.
[IMPROVED] Improve startup times.

##Version 0.4 - 2013-12-05

[IMPROVED] Reduces debug logging.

##Version 0.3 - 2013-11-20

[NEW] Omit bundled server list.
[FIXED] Fix minor UI rendering bug.

##Version 0.2 - 2013-11-16

[NEW] Update server list signature verification.

##Version 0.1 - 2013-11-13

[IMPROVED] Significantly refactored XAML UI code.
[NEW] Remove dedicated 'Exit' button.
