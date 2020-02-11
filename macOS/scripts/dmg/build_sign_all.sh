#!/bin/bash
echo "Signing all binaries (required for Apple notarization)"

# get directory of current script
BASEDIR=$(dirname "$0")

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

CERTIFICATE_INFO_FILE="$BASEDIR/../config/devid_certificate.txt"
if [ -f "$CERTIFICATE_INFO_FILE" ]
then
  echo "Reading certificate info from file '$CERTIFICATE_INFO_FILE'"
else
	echo " * Signing FAILED."
  echo " * '$CERTIFICATE_INFO_FILE' not found."
  echo " * Please, create '$CERTIFICATE_INFO_FILE'. Look on example-file: 'devid_certificate-example.txt'"
  exit 1
fi

# file 'ivpn_devid.txt' should be located in same directory
SIGN_CERT=$(<"$CERTIFICATE_INFO_FILE")

# temporarily setting the IFS (internal field seperator) to the newline character.
# (required to process result pf 'find' command)
IFS=$'\n'; set -f

echo ""
echo ">>> COPYING LIBRARIES WILL BE REQUIRED AFTER SIGNING..."
echo ""

#echo cp "/usr/lib/libc.dylib" "_image/IVPN.app/Contents/MacOS/IVPN Agent.app/Contents/MonoBundle/"
#cp "/usr/lib/libc.dylib" "_image/IVPN.app/Contents/MacOS/IVPN Agent.app/Contents/MonoBundle/"

echo cp "/usr/lib/libc.dylib" "_image/IVPN.app/Contents/MonoBundle/"
cp "/usr/lib/libc.dylib" "_image/IVPN.app/Contents/MonoBundle/"
CheckLastResult "Copying failed"

#exit 0

#signing all libraries
echo ""
echo ">>> Signing libraries..."
echo ""
for f in $(find _image -name '*.dylib' -or -name '*.so');
do
  echo "Signing: [" $f "]";
  codesign --verbose=4 --force --sign "${SIGN_CERT}" "$f"
  CheckLastResult "Signing failed"
done

#restore temporarily setting the IFS (internal field seperator)
unset IFS; set +f

# additional binaries to sign
ListThirdPartyBinaries=(
"_image/IVPN.app/Contents/Frameworks/Sparkle.framework/Versions/A/Resources/Autoupdate.app/Contents/MacOS/fileop"
"_image/IVPN.app/Contents/Frameworks/Sparkle.framework/Versions/A/Resources/Autoupdate.app/Contents/MacOS/Autoupdate"
"_image/IVPN.app/Contents/Library/LaunchServices/net.ivpn.client.Helper"
"_image/IVPN.app/Contents/MacOS/openvpn"
"_image/IVPN.app/Contents/MacOS/WireGuard/wg"
"_image/IVPN.app/Contents/MacOS/WireGuard/wireguard-go"
)

ListCompiledBinaries=(
"_image/Uninstall IVPN.app/Contents/MacOS/Uninstall IVPN"
"_image/Uninstall IVPN.app"

"_image/IVPN.app/Contents/MacOS/IVPN Agent"
"_image/IVPN.app/Contents/MacOS/IVPN"
"_image/IVPN.app"
)

echo ""
echo ">>> Signing third-party binaries..."
echo ""
for f in "${ListThirdPartyBinaries[@]}";
do
  echo "Signing: [" $f "]";
  codesign --verbose=4 --force --sign "${SIGN_CERT}" --options runtime "$f"
  CheckLastResult "Signing failed"
done

echo ""
echo ">>> Signing compiled binaries..."
echo ""
for f in "${ListCompiledBinaries[@]}";
do
  echo "Signing: [" $f "]";
  codesign --verbose=4 --force --deep --sign "${SIGN_CERT}" --entitlements build_HarderingEntitlements.plist --options runtime "$f"
  CheckLastResult "Signing failed"
done
