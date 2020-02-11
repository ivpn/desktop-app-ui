#!/bin/bash
LIBIVPN_FILE="../linivpn/libivpn.dylib"

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

echo "*********************************************************************"
echo "-----------------------------"
echo "------ Updating plist -------"
echo "-----------------------------"
CERTIFICATE_INFO_FILE="../scripts/config/devid_certificate.txt"
if [ -f "$CERTIFICATE_INFO_FILE" ]
then
  echo "Reading certificate info from file '$CERTIFICATE_INFO_FILE'"
else
	echo " * plist update FAILED."
  echo " * '$CERTIFICATE_INFO_FILE' not found."
  echo " * Please, create '$CERTIFICATE_INFO_FILE'. Look on example-file: 'ivpn_devid_certificate-example.txt'"
  echo "*********************************************************************"
  exit 1
fi

CERTIFICATE=$(<$CERTIFICATE_INFO_FILE);

#copy plist from template
cp Info_template.plist Info.plist

#update 'SMPrivilegedExecutables' parameter
echo "Updating net.ivpn.client.Helper certificate: $CERTIFICATE ..."
plutil -replace SMPrivilegedExecutables -xml "<dict> <key>net.ivpn.client.Helper</key> <string>identifier net.ivpn.client.Helper and certificate leaf[subject.OU] = $CERTIFICATE</string></dict>" "Info.plist"

echo "Updating version..."
#update version info
./../scripts/update-plist-version.sh Info.plist
if ! [ $? -eq 0 ]; then
    echo FAILED
    echo "*********************************************************************"
    exit 1
fi

if ! [ -f "$LIBIVPN_FILE" ]
then
  # libivpn.dylib binary not exists

  echo "COMPILING: '$LIBIVPN_FILE'..."

  cd "$(dirname "$0")"
  cd "../libivpn"

  make all
  CheckLastResult

  # we are not copying libvpn into bundle manually
  # it will be copied automatically during building the project 

  echo "... 'libivpn.dylib' COMPILING DONE"
fi



echo "*********************************************************************"
