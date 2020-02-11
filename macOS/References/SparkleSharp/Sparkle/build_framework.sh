#!/bin/bash

DIR=$(dirname ${0})
cd ${DIR}
git submodule init && git submodule update
cd 3rdparty/Sparkle
xcodebuild -configuration Release -target Sparkle
