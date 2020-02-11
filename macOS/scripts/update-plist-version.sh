#!/bin/bash

# get directory of current script
BASEDIR=$(dirname "$0")
echo "____________________________________________"
VERSION_INFO_FILE="$BASEDIR/config/version.txt"
if [ -f "$VERSION_INFO_FILE" ]
then
  echo "Reading version info from file '$VERSION_INFO_FILE'"
else
	echo " * Version update FAILED."
  echo " * '$VERSION_INFO_FILE' not found."
  echo " * Please, create '$VERSION_INFO_FILE'. Simple text file with version number. (Example: 2.6.4)"
  echo "____________________________________________"
  exit 1
fi

if ! [ -f "$1" ]
then
	echo " * Version update FAILED."
  echo " * File '$1' not found."
  echo "____________________________________________"
  exit 1
fi

VERSION=$(<"$VERSION_INFO_FILE")

echo Updating CFBundleShortVersionString=[${VERSION}] in file [$1] ...
plutil -replace CFBundleShortVersionString -xml "<string>${VERSION}</string>" "$1"
if ! [ $? -eq 0 ]; then
    echo Version update FAILED
    echo "____________________________________________"
    exit 1
fi

echo Updating CFBundleVersion=[${VERSION}] in file [$1] ...
plutil -replace CFBundleVersion -xml "<string>${VERSION}</string>" "$1"
if ! [ $? -eq 0 ]; then
    echo Version update FAILED
    echo "____________________________________________"
    exit 1
fi

echo "____________________________________________"
