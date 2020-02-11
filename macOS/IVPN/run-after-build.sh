#!/bin/bash
echo "*********************************************************************"
if [ $# -eq 0 ]
then
    echo "Usage:"
    echo "      $0 <TARGET_DIR>"
    echo "ERROR: Prease define <TARGET_DIR> argument"
    echo "*********************************************************************"
    exit 1
fi

# folder where IVPN.app located (release or debug)
# eg: /Users/stenya/Projects/ivpn-client/mac/IVPN/bin/Release
TARGET_DIR=$1
HELPER_FILE="../helper/net.ivpn.client.Helper"

# enter to current folder (where script located)
cd "$(dirname "$0")"

# check result of last executed command
function CheckLastResult
{
  if ! [ $? -eq 0 ]
  then #check result of last command
    if [ -n "$1" ]
    then
      echo $1
    else
      echo "FAILED"
      echo "*********************************************************************"
    fi
    exit 1
  fi
}

function BuildHelper
{
  echo "COMPILING: '$HELPER_FILE'..."

  cd "$(dirname "$0")"
  cd "../helper"

  make release
  CheckLastResult
  echo "... helper COMPILING DONE"
}

echo "Checking helper ..."
HELPER_FILE="../helper/net.ivpn.client.Helper"
if ! [ -f "$HELPER_FILE" ]
then
  # helper binary not exists
  BuildHelper
else
  # checking version of helper binary
  BUNDLE_PLIST_FILE="${TARGET_DIR}/IVPN.app/Contents/Info.plist"
  BUNDLE_VERSION=`/usr/libexec/PlistBuddy -c "Print :CFBundleVersion" "$BUNDLE_PLIST_FILE"`
  if ! cat $HELPER_FILE | grep -q -F "<string>$BUNDLE_VERSION</string>" ; then
      echo "Incorrect version in the helper file. ($HELPER_FILE)"
      BuildHelper
  fi
fi

# enter to current folder (where script located)
cd "$(dirname "$0")"
# Copying helper
echo "Copying '../helper/net.ivpn.client.Helper' to '${TARGET_DIR}/IVPN.app/Contents/Library/LaunchServices'"
mkdir -p "${TARGET_DIR}/IVPN.app/Contents/Library/LaunchServices"
cp "../helper/net.ivpn.client.Helper" "${TARGET_DIR}/IVPN.app/Contents/Library/LaunchServices"
CheckLastResult

#signing binary
echo "Signing..."
cd "$(dirname "$0")"
./../scripts/sign-file.sh "${TARGET_DIR}/IVPN.app"
CheckLastResult

echo "*********************************************************************"
