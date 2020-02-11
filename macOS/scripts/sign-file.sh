#!/bin/bash

# signing binary files by DEVID certificate
# certificate file location: devid_certificate.txt

# get directory of current script
BASEDIR=$(dirname "$0")

echo "--------------------------------------------"
echo "--- Signing file: [$1] ---"
echo "--------------------------------------------"

CERTIFICATE_INFO_FILE="$BASEDIR/config/devid_certificate.txt"
if [ -f "$CERTIFICATE_INFO_FILE" ]
then
  echo "Reading certificate info from file '$CERTIFICATE_INFO_FILE'"
else
	echo " * Signing FAILED."
  echo " * '$CERTIFICATE_INFO_FILE' not found."
  echo " * Please, create '$CERTIFICATE_INFO_FILE'. It shall cantain only Apple DeveloperID/TeamID (XXXXXXXXXX).'"
  echo "--------------------------------------------"
  exit 1
fi

SIGN_CERT=$(<"$CERTIFICATE_INFO_FILE")

echo Signing file [$1] by certificate [${SIGN_CERT}] ...

if [[ $2 == "-hardering" ]]; then
    echo "[Signing with hardering]"
    codesign --verbose=4 --force --deep --sign "${SIGN_CERT}"  --options runtime "$1"
else
    codesign --verbose=4 --force --deep --sign "${SIGN_CERT}" "$1"
fi

if [ $? -eq 0 ]; then
    echo Signing OK
else
    echo Signing FAILED
    echo "--------------------------------------------"
    exit 1
fi
echo "--------------------------------------------"
