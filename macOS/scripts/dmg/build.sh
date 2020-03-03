#!/bin/bash

# The following DMG image creation is based on this solution:
# http://stackoverflow.com/questions/96882/how-do-i-create-a-nice-looking-dmg-for-mac-os-x-using-command-line-tools
#

echo '###########################'
echo "###    COMPILING DMG    ###";
echo '###########################'

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
    fi
    exit 1
  fi
}

VERSION_FILE="../config/version.txt"
EXPECTED_VERSION=`cat "$VERSION_FILE"`

DAEMON_REPO_ABS_PATH=$("./../daemon_repo_local_path_abs.sh")
CheckLastResult "Failed to determine location of IVPN Daemon sources. Plase check 'config/daemon_repo_local_path.txt'"

CLI_REPO_ABS_PATH=$("./../cli_repo_local_path_abs.sh")
CheckLastResult "Failed to determine location of IVPN CLI sources. Plase check 'config/cli_repo_local_path.txt'"

echo '---------------------------'
echo "Building IVPN Daemon ($DAEMON_REPO_ABS_PATH)...";
echo '---------------------------'
$DAEMON_REPO_ABS_PATH/References/macOS/scripts/build-all.sh -norebuild -v $EXPECTED_VERSION
CheckLastResult "ERROR building IVPN Daemon"

echo '---------------------------'
echo "Building IVPN CLI ($CLI_REPO_ABS_PATH)...";
echo '---------------------------'
$CLI_REPO_ABS_PATH/References/macOS/build.sh -v $EXPECTED_VERSION
CheckLastResult "ERROR building IVPN CLI"

echo '---------------------------'
echo "Building IVPN App (UI client) ...";
echo '---------------------------'
./../../../build_ui_macOS.sh
CheckLastResult "ERROR building IVPN.app"

echo '---------------------------'
echo "Copying files ...";
echo '---------------------------'

# Erasing old files
rm -rf ./_image
mkdir ./_image

# copying required files
APP_FILE="../../IVPN/bin/Release/IVPN.app"
UNINSTALL_FILE="../../Uninstall/bin/Release/Uninstall IVPN.app"

echo "Copying Client ..."
cp -a "${APP_FILE}" ./_image/
CheckLastResult "Error copying ${APP_FILE}"

echo "Copying Uninstaller ..."
cp -a "${UNINSTALL_FILE}" ./_image/
CheckLastResult "Error copying ${UNINSTALL_FILE}"

echo "Copying 'etc'..."
cp -R "$DAEMON_REPO_ABS_PATH/References/macOS/etc" "./_image/IVPN.app/Contents/Resources"
CheckLastResult

echo "Copying 'openvpn'..."
cp "$DAEMON_REPO_ABS_PATH/References/macOS/_deps/openvpn_inst/bin/openvpn" "./_image/IVPN.app/Contents/MacOS/openvpn"
CheckLastResult

echo "Copying 'obfsproxy' binaries..."
cp -R "$DAEMON_REPO_ABS_PATH/References/macOS/obfsproxy" "./_image/IVPN.app/Contents/Resources"
CheckLastResult

echo "Copying 'WireGuard' binaries..."
mkdir -p "./_image/IVPN.app/Contents/MacOS/WireGuard"
cp "$DAEMON_REPO_ABS_PATH/References/macOS/_deps/wg_inst/wg" "./_image/IVPN.app/Contents/MacOS/WireGuard/wg"
CheckLastResult
cp "$DAEMON_REPO_ABS_PATH/References/macOS/_deps/wg_inst/wireguard-go" "./_image/IVPN.app/Contents/MacOS/WireGuard/wireguard-go"
CheckLastResult

echo "Copying daemon..."
cp -R "$DAEMON_REPO_ABS_PATH/IVPN Agent" "./_image/IVPN.app/Contents/MacOS"
echo "Copying CLI..."
mkdir "./_image/IVPN.app/Contents/MacOS/cli"
cp -R "$CLI_REPO_ABS_PATH/ivpn" "./_image/IVPN.app/Contents/MacOS/cli"
CheckLastResult

echo "Removing unnecessary debug files..."
find "./_image/IVPN.app" -iname "*.pdb" -type f -delete
find "./_image/IVPN.app" -iname "*.dSYM"  -type d -exec rm -r {} +
find "./_image/Uninstall IVPN.app" -iname "*.pdb" -type f -delete
find "./_image/Uninstall IVPN.app" -iname "*.dSYM"  -type d -exec rm -r {} +

echo "Signing..."
../sign-file.sh "./_image/IVPN.app"
CheckLastResult

echo '---------------------------'
echo " Checking binaries versions..."

BUNDLE_PLIST_FILE="./_image/IVPN.app/Contents/Info.plist"
HELPER_FILE="./_image/IVPN.app/Contents/Library/LaunchServices/net.ivpn.client.Helper"

CheckLastResult "Can not read version from '$VERSION_FILE'"
BUNDLE_VERSION=`/usr/libexec/PlistBuddy -c "Print :CFBundleVersion" "$BUNDLE_PLIST_FILE"`
CheckLastResult "Can not read bundle version from '$BUNDLE_PLIST_FILE'"
BUNDLE_SHORT_VERSION=`/usr/libexec/PlistBuddy -c "Print :CFBundleShortVersionString" "$BUNDLE_PLIST_FILE"`
CheckLastResult "Can not read bundle short version from '$BUNDLE_PLIST_FILE'"

echo "  config/version.txt:                       ${EXPECTED_VERSION}"
echo "  IVPN.app bundle version:                  ${BUNDLE_VERSION}"
echo "  IVPN.app bundle short version:            ${BUNDLE_SHORT_VERSION}"
if ! cat $HELPER_FILE | grep -q -F "<string>$EXPECTED_VERSION</string>" ; then
    echo "ERROR: Incorrect version in the helper file. ($HELPER_FILE)"
    return 1
else
  echo "  net.ivpn.client.Helper:                   OK (${EXPECTED_VERSION})"
fi

if [[ $EXPECTED_VERSION != $BUNDLE_VERSION ]]; then
    echo "ERROR: EXPECTED_VERSION != BUNDLE_VERSION"
    return 1
fi

if [[ $EXPECTED_VERSION != $BUNDLE_SHORT_VERSION ]]; then
    echo "ERROR: EXPECTED_VERSION != BUNDLE_SHORT_VERSION"
    return 1
fi
echo '---------------------------'

if [[ "$@" == *"-hardering"* ]]; then
    echo '---------------------------'
    echo "Signing all binaries for Apple notarizarion...";
    echo '---------------------------'
    ./build_sign_all.sh
    CheckLastResult "Signing files FAILED. DMG generation stopped."
fi


DMG_FILE="_compiled/IVPN-"${EXPECTED_VERSION}".dmg"
TMP_DMG_FILE="_compiled/ivpn.temp.dmg"
echo '---------------------------'
echo 'Generating  DMG : ' ${DMG_FILE}
echo '---------------------------'

#prepare background image
mkdir -p "./_image/.background" && cp "resources/back.png" "./_image/.background/back.png"
CheckLastResult

# creating output directory (if not exists)
mkdir -p _compiled
CheckLastResult

echo "Removing old files"
rm ${TMP_DMG_FILE}
rm ${DMG_FILE}

BACKGROUND_FILE="back.png"
APPLICATION_NAME="IVPN.app"
UNINSTALL_APPLICATION_NAME="Uninstall IVPN.app"

source=./_image
title=IVPN
size=256000

echo "Creating a new temporary r/w DMG image"
hdiutil create -srcfolder "${source}" -volname "${title}" -fs HFS+ \
      -fsargs "-c c=64,a=16,e=16" -format UDRW -size ${size}k ${TMP_DMG_FILE}
CheckLastResult

device=$(hdiutil attach -readwrite -noverify -noautoopen ${TMP_DMG_FILE} | \
         egrep '^/dev/' | sed 1q | awk '{print $1}')
CheckLastResult

echo "Mounted as device: ${device}"

sleep 2 # give time to finish mounting

echo '
   tell application "Finder"
     tell disk "'${title}'"
           open
           set current view of container window to icon view
           set toolbar visible of container window to false
           set statusbar visible of container window to false
           set the bounds of container window to {200, 200, 758, 680}
           set theViewOptions to the icon view options of container window
           set arrangement of theViewOptions to not arranged
           set icon size of theViewOptions to 108
           set background picture of theViewOptions to file ".background:'${BACKGROUND_FILE}'"
           make new alias file at container window to POSIX file "/Applications" with properties {name:"Applications"}
           set position of item "'${APPLICATION_NAME}'" of container window to {120, 110}
           set position of item "'${UNINSTALL_APPLICATION_NAME}'" of container window to {420, 300}
           set position of item "Applications" of container window to {420, 110}
           set position of item ".background" of container window to {120, 500}
           set position of item ".fseventsd" of container window to {420, 500}
           update without registering applications
           delay 3
           close
     end tell
   end tell
' | osascript
CheckLastResult

sleep 5

chmod -Rf go-w /Volumes/"${title}"
sync
sync

echo "Detaching temporary dmg from ${device}"
hdiutil detach $device
CheckLastResult

hdiutil convert ${TMP_DMG_FILE} -format UDZO -imagekey zlib-level=9 -o "${DMG_FILE}"
CheckLastResult
rm -f ${TMP_DMG_FILE}

if [[ "$@" != *"-hardering"* ]]; then
  ./../sign-file.sh "${DMG_FILE}"
  CheckLastResult
else
  ./../sign-file.sh "${DMG_FILE}" "-hardering"
  CheckLastResult

  NOTARIZATION_SENT=0
  echo " *** [APPLE NOTARIZATION] Do you wish to upload '${DMG_FILE}' to Apple for notarization? *** "
  read -p "(y\n)" yn
      case $yn in
          [Yy]* )
            echo "UPLOADING TO APPLE NOTARIZATION SERVICE...";
            read -p  'Apple credentials - Username (email): ' uservar
            read -sp 'Apple credentials - Password        : ' passvar
            echo ""
            echo "Uploading (will take few minutes of time, no progress indication) ..."
            xcrun altool --notarize-app --primary-bundle-id "${EXPECTED_VERSION}" --username "${uservar}" --password "${passvar}" --file "${DMG_FILE}"
            CheckLastResult;
            NOTARIZATION_SENT=1
            ;;
          [Nn]* )
            echo "Apple notarization skipped."
            ;;
          * ) ;;
      esac

      if [[ $NOTARIZATION_SENT == 1 ]]; then
      echo "--------------------------------------------"
      echo " *** Do you wish to stample Apple notarization result to a file? *** "
      echo "    [NOTE!] Before doing that, you must wait until Apple service"
      echo "            will finish notarization process with 'Package Approved' result."
      echo "            Usually, it takes less than a hour."
      echo "            Untill that, you can leave this script opened (do not answer 'y')."
      echo ""
      echo "    Usefull commands (you can execute them in another terminal):  "
      echo "        To check notarization history:  "
      echo "            xcrun altool --notarization-history 0 -u <APPLE_NOTARIZATION_USER> "
      echo "        To check notarization status of concrete package:  "
      echo "            xcrun altool --notarization-info <RequestUUID> -u <APPLE_NOTARIZATION_USER> "
      read -p "(y\n)" yn
          case $yn in
              [Yy]* )
                echo "STAPLING NOTARIZATION INFO...";
                xcrun stapler staple "${DMG_FILE}"
                CheckLastResult;
                ;;
              [Nn]* );;
              * ) ;;
          esac
      fi

fi

open ./_compiled/
