#!/bin/bash

cd "$(dirname "$0")"
BASE_DIR="$( pwd )"

# Exit immediately if a command exits with a non-zero status.
set -e

# 'CustomCommands' (from IVPN project) does not supported by msbuild
# here we should manually execute them
cd "macOS/IVPN"
./run-before-build.sh

cd $BASE_DIR
nuget restore "IVPN App macOS.sln"

cd $BASE_DIR
msbuild "IVPN App macOS.sln" /verbosity:minimal /t:Clean,Build -p:Configuration=Release

# 'CustomCommands' (from IVPN project) does not supported by msbuild
# here we should manually execute them
cd "macOS/IVPN"
./run-after-build.sh "$BASE_DIR/macOS/IVPN/bin/Release"
