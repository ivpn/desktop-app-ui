# Changelog

All notable changes to this project will be documented in this file.

## Version 2.12.10 - 2020-11-12

[FIXED] Removed dependencies from WIFI libraries 

[Download IVPN Client v. 2.12.10](https://cdn.ivpn.net/releases/osx/IVPN-2.12.10.dmg)  
SHA256: b955b230977d64b45cb81b6f4a31b8f42dbc864b98bc91202f46a495c89b3d21

## Version 2.12.9 - 2020-11-11

[IMPROVED] Сompatibility with macOS Big Sur  
[FIXED] Set custom DNS to 127.0.0.1  
[FIXED] Various UI issues  

[Download IVPN Client v. 2.12.9](https://cdn.ivpn.net/releases/osx/IVPN-2.12.9.dmg)  
SHA256: d3c2ca2c5c6aecaff51807797e58c6403752eed4e64dcde0f4170b14024c8a03

## Version 2.12.6 - 2020-09-02

[IMPROVED] Uninstaller improvements  
[FIXED] Spontaneous reconnections  

[Download IVPN Client v. 2.12.6](https://cdn.ivpn.net/releases/osx/IVPN-2.12.6.dmg)  
SHA256: dac7caeff9a1584e6e471a566df0b662c03ef15cab4abf5b8dba349ac9901d2e

## Version 2.12.4 - 2020-06-30

[IMPROVED] Account ID validation logic  

[Download IVPN Client v. 2.12.4](https://cdn.ivpn.net/releases/osx/IVPN-2.12.4.dmg)  
SHA256: 3d12c0d576cf3d61f16ecd23852b486c99b8208acfa9552c24e14d9f95dd1c66

## Version 2.12.3 - 2020-06-05

[IMPROVED] User-defined extra configuration parameters for OpenVPN moved to separate file with access rights only for privileged account  
[FIXED] Random disconnections on waking-up from sleep  
[FIXED] Always-on Firewall is blocking traffic on system boot  
[FIXED] WireGuard connection error when a network interface not initialized  

[Download IVPN Client v. 2.12.3](https://cdn.ivpn.net/releases/osx/IVPN-2.12.3.dmg)  
SHA256: 858b8fcf618a6213e1021f108d5f0986e79d6faca378a7b6d33d39c48490585e

## Version 2.12.2 - 2020-05-23

[IMPROVED] Overall stability  
[FIXED] Potential disconnection when network changes  

[Download IVPN Client v. 2.12.2](https://cdn.ivpn.net/releases/osx/IVPN-2.12.2.dmg)  
SHA256: f3657528b6f96f444e83e889a0a6cc6f337ae7328f15718e40ea424fc102aa2f

## Version 2.12.1 - 2020-05-21

[FIXED] Potential disconnection when network changes  

[Download IVPN Client v. 2.12.1](https://cdn.ivpn.net/releases/osx/IVPN-2.12.1.dmg)  
SHA256: 39723ff2f3e8f4b96dbd90b70ead407cf2d050fea86083816ec02a1b1d6e90ee

## Version 2.12.0 - 2020-05-14

[NEW] Command line interface for IVPN service  
[IMPROVED] Overall stability  

[Download IVPN Client v. 2.12.0](https://cdn.ivpn.net/releases/osx/IVPN-2.12.0.dmg)  
SHA256: a8296ed265f5d56695c75beeb6c3263b829a239a5619708160f6df92de16a1ed

## Version 2.11.9 - 2020-04-27

[IMPROVED] Removed WireGuard warning  

[Download IVPN Client v. 2.11.9](https://cdn.ivpn.net/releases/osx/IVPN-2.11.9.dmg)  
SHA256: 56ecadde02dca890d0eca9de677961abc405b60384ed382cf86b105050e077de

## Version 2.11.7 - 2020-03-30

[FIXED] Sending logs  

[Download IVPN Client v. 2.11.7](https://cdn.ivpn.net/releases/osx/IVPN-2.11.7.dmg)  
SHA256: b5589eea1709d4de8c770719fb7067506980cf3e8043b0f03f3d9548e97cffde

## Version 2.11.6 - 2020-03-27

[FIXED] Sometimes ping indicators are not updating because of an issue on the daemon side

## Version 2.11.5 - 2020-03-26

[IMPROVED] Reconnection functionality  
[FIXED] "Automatically change port" feature

## Version 2.11.3 - 2020-02-24

[FIXED] Pause feature for WireGuard  
[FIXED] Reset AntiTracker to default value on LogOut

## Version 2.11.2 - 2020-02-20

[IMPROVED] WireGuard upgraded to 0.0.20200121  
[FIXED] Issue with background update of WireGuard keys  
[FIXED] Firewall config changes from Always-On to On-Demand after upgrade  
[FIXED] Processing of users additional OpenVPN parameters

## Version 2.11.0 - 2020-01-24

[IMPROVED] Reduced binary size and improved performance of IVPN Agent

## Version 2.10.9 - 2019-12-12

[IMPROVED] Login screen  
[IMPROVED] Decreased application size, removed unnecessary files from the bundle  
[IMPROVED] Overall stability  
[FIXED] Potential privilege escalation vulnerability by improving OpenVPN extra parameter filtering

## Version 2.10.8 - 2019-12-04

[FIXED] Potential privilege escalation vulnerability by improving OpenVPN extra parameter filtering

## Version 2.10.7 - 2019-12-02

[FIXED] Various UI issues

## Version 2.10.6 - 2019-11-29

[IMPROVED] Overall stability  
[FIXED] Various UI issues

## Version 2.10.5 - 2019-11-14

[IMPROVED] Overall stability

## Version 2.10.4 - 2019-11-12

[NEW] Bypass DNS blocks to IVPN API  
[IMPROVED] WireGuard upgraded to v0.0.20191012  
[IMPROVED] Reliability of Always-on Firewall during boot time  
[IMPROVED] Login session management  
[FIXED] Incorrect 'fastest server' detection after system reboot  
[FIXED] Various UI issues

## Version 2.10.3 - 2019-10-07

[NEW] Dark Mode  
[IMPROVED] macOS Catalina compatibility  
[FIXED] WireGuard sometimes loses connectivity after PC wakes up

## Version 2.10.1 - 2019-08-02

[NEW] Added new port for connection: 1194 UDP  
[NEW] Binaries notarized by Apple notary service  
[IMPROVED] Overall stability  
[FIXED] OpenVPN re-connecting sometimes on slow connections  
[FIXED] DNS issue with Multi-Hop connection when AntiTracker enabled  
[FIXED] Various UI issues

## Version 2.9.9 - 2019-06-14

[FIXED] DNS switching problem  
[FIXED] Failure to submit error reports  
[FIXED] Various UI issues

## Version 2.9.8 - 2019-06-11

[NEW] AntiTracker: block ads, malicious websites, and third-party trackers  
[NEW] Custom DNS: specify DNS server when connected to VPN  
[NEW] Automatic WireGuard key regeneration

## Version 2.9.7 - 2019-03-21

[NEW] Display public IP and geolocation information  
[NEW] Fastest server configuration  
[NEW] Setting to skip confirmation when application closing in the connected state  
[NEW] Setting to stop IVPN agent when not in use  
[IMPROVED] OpenVPN upgraded to v2.4.6  
[IMPROVED] OpenSSL upgraded to v1.1.1a  
[IMPROVED] Settings are reset to defaults on logout  
[FIXED] Ocassional IVPN client reconnections  
[FIXED] Account status check  
[FIXED] Save selected servers when switching between OpenVPN and WireGuard  
[FIXED] Crash when failed to connect to IVPN agent  
[FIXED] Various UI issues

## Version 2.9.6 - 2019-01-15

[IMPROVED] WireGuard upgraded to v0.0.20181222  
[IMPROVED] Overall stability  
[FIXED] Various UI issues

## Version 2.9.5 - 2018-12-19

[IMPROVED] Overall stability  
[FIXED] Various UI issues

## Version 2.9.4 - 2018-12-17

[IMPROVED] Added possibility to change server without disconnect first  
[IMPROVED] Overall stability

## Version 2.9.3 - 2018-11-29

[NEW] WireGuard protocol - WireGuard is a new VPN protocol that promises better security and faster speeds compared to existing solutions like OpenVPN or IPSec. The WireGuard protocol is currently under heavy development and should be considered as experimental. Review the WireGuard project for more information. We do not recommend WireGuard except for situations where security is not critical.  
[IMPROVED] Detection of the fastest server  
[IMPROVED] Server ping time detection

## Version 2.8.10 - 2018-10-31

[IMPROVED] Notification about lack of privileges for 'Launch at login' feature on macOS Mojave 10.14  
[IMPROVED] Overall stability  
[FIXED] Various UI issues

## Version 2.8.8 - 2018-10-01

[FIXED] Connection issue when using 'obfsproxy' transport proxy on macOS Mojave

## Version 2.8.7 - 2018-09-21

[FIXED] Potential privilege escalation vulnerability by improving OpenVPN extra parameter filtering  
[FIXED] UI issues in macOS Mojave 10.14

## Version 2.8.6 - 2018-08-28

[IMPROVED] 'Pause VPN' feature does not affect 'Always-on Firewall' configuration

## Version 2.8.5 - 2018-08-27

[FIXED] Various UI issues

## Version 2.8.4 - 2018-08-22

[IMPROVED] Overall stability

## Version 2.8.3 - 2018-08-20

[NEW] Ability to temporary pause VPN connection  
[IMPROVED] Overall stability  
[FIXED] Problems with connection on ports: UDP 2050 and TCP 1443  
[FIXED] Various UI issues

## Version 2.8.1 - 2018-08-01

[IMPROVED] General performance and stability

## Version 2.8.0 - 2018-07-23

[NEW] Ability to assign trust status and create connection rules for WiFi networks  
[IMPROVED] Minor interface improvements  
[FIXED] A bug that prevented automatic connection on startup for some users  
[FIXED] Loss of internet connection when switching WiFi and disconnecting from the VPN

## Version 2.7.9 - 2018-06-18

[NEW] Automatic selection of fastest server  
[NEW] Added new TCP/UDP ports for connection  
[IMPROVED] Minor interface updates

## Version 2.7.8 - 2018-06-04

[NEW] The IVPN firewall does not block Apple services when enabled 'Allow LAN'  
[IMPROVED] Minor interface updates

## Version 2.7.7 - 2018-05-22

[NEW] Ability to connect to VPN when API server is not reachable  
[IMPROVED] General performance and stability

## Version 2.7.6 - 2018-04-23

[IMPROVED] OpenVPN upgraded to v2.4.5  
[IMPROVED] OpenSSL upgraded to v1.1.0h  
[IMPROVED] General performance and stability

## Version 2.7.5 - 2018-04-03

[FIXED] Compatibility issues with OS X 10.10 Yosemite

## Version 2.7.4 - 2018-03-28

[IMPROVED] General performance and stability  
[FIXED] Various UI issues

## Version 2.7.2 - 2018-03-07

[IMPROVED] General performance and stability

## Version 2.7.1 - 2018-02-26

[IMPROVED] General performance and stability  
[FIXED] Compatibility issues with OS X 10.10 Yosemite  
[FIXED] Various UI issues

## Version 2.7 - 2018-02-13

[NEW] New UI that makes the set up and first connection process more intuitive  
[NEW] New multihop UI  
[NEW] Added the ability to "show icon in dock" in preferences  
[IMPROVED] Consistency across desktop clients and mobile apps  
[IMPROVED] The client will now automatically select a port if the default port is blocked  
[IMPROVED] General performance and stability  
[FIXED] An issue causing "launch at startup" to function improperly

## Version 2.6.8 - 2017-10-18

[NEW] Added ability to override basic OpenVPN parameters with user-defined parameters  
[IMPROVED] Updated OpenVPN parameters according to ‘Deprecated options’ list  
[FIXED] Fixed authentication error due to specific symbols present in password  
[FIXED] Various stability fixes

## Version 2.6.7 - 2017-10-09

[FIXED] Fix for losing internet connection after wakeup  
[FIXED] Various stability fixes

## Version 2.6.6 - 2017-09-13

[NEW] Implemented IVPN Firewall notification windows  
[NEW] Added option to specify additional OpenVPN parameters  
[NEW] Added new new update framework to ease future upgrades  
[IMPROVED] Updated OpenVPN to the latest version (2.4.3)  
[FIXED] Fixed rare bug when application stuck in the “Connected…” or “Reconnecting…” state  
[FIXED] Fixed "Start at login" setting not working correctly  
[FIXED] Fixed bug where spaces and other special characters in password resulted in incorrect username/password  
[FIXED] Fixed error: "Could not read Auth username/password/ok string from management interface" on connect  
[FIXED] Fixed "Cancel" button is disabled when the server is not responding  
[FIXED] Various stability fixes

## Version 2.6.2 - 2017-02-14

[IMPROVED] Improved communication protocol between local service and desktop client for improved stability  
[FIXED] Fix for rare freezes during connection process to unresponsive servers  
[IMPROVED] Implemented additional protection for user credentials

## Version 2.6.1 - 2016-08-13

[NEW] New setting ‘Allow Multicast when LAN traffic is allowed’ added. This option allows access to more LAN resources while IVPN Firewall is active  
[IMPROVED] OpenVPN updated to v2.3.11  
[NEW] The installer and uninstaller are now packaged in a .dmg image  
[NEW] Login Item can now be enabled/disabled from the System Preferences  
[IMPROVED] Reduced verbosity of information messages in the log file when logging is enabled

## Version 2.6.0 - 2016-02-11

[IMPROVED] Improved error handling.  
[IMPROVED] Reduced CPU usage.  
[IMPROVED] Disable client diagnostic logging by default.

## Version 2.5 - 2015-12-07

[FIXED] Minor bug fixes relating to server selection windows.  
[FIXED] Resolved firewall issue where multiple TUN adapters are installed.

## Version 2.4.2 - 2015-11-28

[FIXED] Resolve firewall issue with multiple tun devices installed.

## Version 2.4.1 - 2015-11-28

[FIXED] Fix obfsproxy startup issue.

## Version 2.4 - 2015-11-26

[NEW] IVPN Firewall added!  
[NEW] Full multi-hop support on all servers.  
[IMPROVED] OpenVPN upgraded to 2.3.8.  
[NEW] Brand new UI.  
[IMPROVED] Full compatibility with El Capitan.  
[IMPROVED] DNS leak protection reworked to be more stable.  
[IMPROVED] Obfsproxy updated to support obfs3.  
[FIXED] Fixed multiple bugs relating to power event changes and switching Wifi networks.  
[IMPROVED] Diagnostics reports upload speed improved.  
[NEW] App is now signed to work with gatekeeper.  
[NEW] Created application uninstaller.  
[IMPROVED] Version bump to bring into line with Windows version.

## Version 0.8.2 - 2014-07-01

[IMPROVED] Upgrade obfsproxy to 0.2.9.  
[IMPROVED] Improve stability of OBFSproxy.

## Version 0.8.1 - 2014-06-09

[IMPROVED] Improve stablity of DNS leak protection code.

## Version 0.8 - 2014-05-30

[IMPROVED] Ensure routes are correctly installed when using a proxy.  
[IMPROVED] Update app logging mechanism to show in Console.app.

## Version 0.7 - 2014-04-28

[FIXED] Fix routing table bug after unblocking traffic.

## Version 0.6 - 2014-04-16

[NEW] Add privacy leak detection.  
[NEW] Add insecure Wi-Fi network detection.  
[NEW] Add OBFSProxy support.  
[NEW] Reopen application window from dock if closed.  
[NEW] Remove dedicated 'Exit' button.

## Version 0.5 - 2014-04-10

[NEW] Add system menu bar icon and working preferences.
