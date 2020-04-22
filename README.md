# IVPN Client Desktop (Windows/macOS)

**IVPN Client Desktop** is an official IVPN client for Windows and macOS.
It divided on two parts:
  - IVPN Client Desktop UI (user interface application - this repository)
  - IVPN service/daemon (repository *ivpn-desktop-daemon*)

IVPN Client Desktop UI built using Xamarin.Mac (C#).

IVPN Client Desktop app is distributed on the official site [www.ivpn.net](www.ivpn.net).  

* [Installation](#installation)
* [Versioning](#versioning)
* [Contributing](#contributing)
* [Security Policy](#security)
* [License](#license)
* [Authors](#Authors)
* [Acknowledgements](#acknowledgements)

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

#### Windows

To compile IVPN Desktop it is necessary to have **IVPN Daemon for IVPN Client Desktop** sources:

  - please, checkout *ivpn-desktop-daemon* repository.
  - update path to *ivpn-desktop-daemon* location in file `Windows/References/config/service_repo_local_path.txt` (if necessary)

To compile IVPN Desktop all binaries and create an installer for the whole application - run the batch file from the terminal. Use Developer Command Prompt for Visual Studio (required for building native sub-projects).
```
ivpn-desktop-ui/Windows/References/Installer/build.bat
```
New compiled installer of IVPN Desktop can be found in folder `Windows/References/bin`

#### macOS

To compile IVPN Desktop it is necessary to have **IVPN Daemon for IVPN Client Desktop** sources:

  - please, checkout *ivpn-desktop-daemon* repository.
  - update path to *ivpn-desktop-daemon* location in file `macOS/scripts/config/daemon_repo_local_path.txt` (if necessary)
  - **Important:** Create text file which contains Apple Developer ID `macOS/scripts/config/devid_certificate.txt`. This is required for signing binaries.

To compile IVPN Desktop all binaries and create DMG-installer for it - run the batch file from the terminal:
```
macOS/scripts/dmg/build.sh
```
New compiled DMG-installer of IVPN Desktop can be found in folder `macOS/scripts/dmg/_compiled`.

**Note!** In order to run application as macOS daemon, the binary must be signed by Apple Developer ID (must be located in `macOS/scripts/config/devid_certificate.txt`).

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

If you are interested in contributing to IVPN Client Desktop project, please read our [Contributing Guidelines](/.github/CONTRIBUTING.md).

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
