#!/bin/bash

cd "$(dirname "$0")"
BASE_DIR="$( pwd )"

# Exit immediately if a command exits with a non-zero status.
set -e

echo "*** Building (running custom vommands before build)..."
# 'CustomCommands' (from IVPN project) does not supported by msbuild
# here we should manually execute them
cd "macOS/IVPN"
./run-before-build.sh

echo "*** Building (restoring packages)..."
cd $BASE_DIR
nuget restore "IVPN App macOS.sln"

echo "*** Building (Starkle framework)..."
cd $BASE_DIR
./macOS/References/SparkleSharp/Sparkle/build_framework.sh

echo "*** Building IVPN App..."
cd $BASE_DIR
msbuild "IVPN App macOS.sln" /verbosity:minimal /t:Clean,Build -p:Configuration=Release

# 'CustomCommands' (from IVPN project) does not supported by msbuild
# here we should manually execute them
cd "macOS/IVPN"
./run-after-build.sh "$BASE_DIR/macOS/IVPN/bin/Release"
