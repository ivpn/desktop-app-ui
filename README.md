# IVPN Client (Windows/macOS)

_This is a legacy project._  
_Development has been moved to a new repository:_ https://github.com/ivpn/desktop-app-ui2  

**IVPN Client** is an official IVPN client for Windows and macOS.
It is a client for  IVPN daemon ([ivpn-desktop-daemon](https://github.com/ivpn/desktop-app-daemon))   

IVPN Client built using Xamarin.Mac (C#).

IVPN Client app is distributed on the official site [www.ivpn.net](https://www.ivpn.net).  

* [About this Repo](#about-repo)
* [Installation](#installation)
* [Versioning](#versioning)
* [Contributing](#contributing)
* [Security Policy](#security)
* [License](#license)
* [Authors](#Authors)
* [Acknowledgements](#acknowledgements)

<a name="about-repo"></a>
## About this Repo

This is the official Git repo of the [IVPN Client](https://github.com/ivpn/desktop-app-ui).

<a name="installation"></a>
## Installation

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

### Requirements

#### Windows

  - Windows 10+
  - NSIS installer
  - Microsoft Visual Studio Community 2019 or Build Tools for Visual Studio 2019
    (ensure Windows SDK 10.0 is installed)
  - Go 1.13+ (downloads automatically by the build script)
  - WiX Toolset (downloads automatically by the build script)

#### macOS

  - macOS Mojave 10.14.6
  - Xcode Command Line Tools
  - Visual Studio for Mac (Xamarin.Mac)
  - Go 1.13+

### Compilation
To build fully redistributable 'installer' binary, addition projects are in use of build process:
  - IVPN service/daemon (repository [Daemon for IVPN Client](https://github.com/ivpn/desktop-app-daemon))
  - IVPN command line interface (repository [IVPN CLI](https://github.com/ivpn/desktop-app-cli))

#### Windows
**Important:**  Use *Developer Command Prompt for Visual Studio* (required for building native sub-projects) to compile.
```
git clone https://github.com/ivpn/desktop-app-ui.git
git clone https://github.com/ivpn/desktop-app-daemon.git
git clone https://github.com/ivpn/desktop-app-cli.git
desktop-app-ui\Windows\References\Installer\build.bat
```
These commands will compile the project and its dependencies. The compiled installer of VPN Client placed in the folder `desktop-app-ui\Windows\References\bin`

#### macOS
```
git clone https://github.com/ivpn/desktop-app-ui.git
git clone https://github.com/ivpn/desktop-app-daemon.git
git clone https://github.com/ivpn/desktop-app-cli.git
```
**Important:** Create text file which contains Apple Developer ID `desktop-app-ui/macOS/scripts/config/devid_certificate.txt`.
This is required for signing binaries.  

Running this script will compile the project with all dependencies.
```
macOS/scripts/dmg/build.sh
```
Compiled *.DMG installer of IVPN Client can be found in folder `macOS/scripts/dmg/_compiled`.

**Note!** The binary must be signed by Apple Developer ID to install and run the application (must be located in `desktop-app-ui/macOS/scripts/config/devid_certificate.txt`).

<a name="versioning"></a>
## Versioning

Project is using [Semantic Versioning (SemVer)](https://semver.org) for creating release versions.

SemVer is a 3-component system in the format of `x.y.z` where:

`x` stands for a **major** version  
`y` stands for a **minor** version  
`z` stands for a **patch**

So we have: `Major.Minor.Patch`

<a name="contributing"></a>
## Contributing

If you are interested in contributing to IVPN Client project, please read our [Contributing Guidelines](/.github/CONTRIBUTING.md).

<a name="security"></a>
## Security Policy

If you want to report a security problem, please read our [Security Policy](/.github/SECURITY.md).

<a name="license"></a>
## License

This project is licensed under the GPLv3 - see the [License](/LICENSE.md) file for details.

<a name="Authors"></a>
## Authors

See the [Authors](/AUTHORS) file for the list of contributors who participated in this project.

<a name="acknowledgements"></a>
## Acknowledgements

See the [Acknowledgements](/ACKNOWLEDGEMENTS.md) file for the list of third party libraries used in this project.
