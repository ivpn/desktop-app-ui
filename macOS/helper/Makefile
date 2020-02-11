CC=cc

BINARY = net.ivpn.client.Helper
PLIST_INFO="IVPN Helper-Info.plist"
PLIST_LAUNCHD="IVPN Helper-Launchd.plist"

SIGN_FILE='../scripts/config/devid_certificate.txt'
SIGN_CERT=$(shell cat $(SIGN_FILE))

release:
	cp "IVPN Helper-Info_template.plist" "IVPN Helper-Info.plist"
	plutil -replace SMAuthorizedClients -xml "<array> <string>identifier net.ivpn.client.IVPN and certificate leaf[subject.OU] = ${SIGN_CERT}</string> </array>" "IVPN Helper-Info.plist"
	./../scripts/update-plist-version.sh "IVPN Helper-Info.plist"

	$(CC) -D TEAM_IDENTIFIER='"${SIGN_CERT}"' -O2 -mmacosx-version-min=10.6 -Xlinker -sectcreate -Xlinker __TEXT -Xlinker __info_plist -Xlinker $(PLIST_INFO) -Xlinker -sectcreate -Xlinker __TEXT -Xlinker __launchd_plist -Xlinker $(PLIST_LAUNCHD) -o $(BINARY) helper.c $(CFLAGS)

	./../scripts/sign-file.sh "$(BINARY)"

#debug:
#	DEBUG_LAUNCH_FILE="$(HOME)/../IVPN/bin/Debug/IVPN.app/Contents/MacOS/IVPN Agent.app/Contents/MacOS/IVPN Agent"
#
#	cp "IVPN Helper-Info_template.plist" "IVPN Helper-Info.plist"
#	plutil -replace SMAuthorizedClients -xml "<array> <string>identifier net.ivpn.client.IVPN and certificate leaf[subject.OU] = ${SIGN_CERT}</string> </array>" "IVPN Helper-Info.plist"
#	./../scripts/update-plist-version.sh "IVPN Helper-Info.plist"
#
#	$(CC) -D TEAM_IDENTIFIER='"${SIGN_CERT}"' -DDEBUG -DAGENT_BINARY='$(DEBUG_LAUNCH_FILE)' -Xlinker -sectcreate -Xlinker __TEXT -Xlinker __info_plist -Xlinker $(PLIST_INFO) -Xlinker -sectcreate -Xlinker __TEXT -Xlinker __launchd_plist -Xlinker $(PLIST_LAUNCHD) -o $(BINARY) helper.c $(CFLAGS)
#
#	./../scripts/sign-file.sh "$(BINARY)"
#
#	echo $(DEBUG_LAUNCH_FILE)
#
#.PHONY: clean

clean:
	rm -f $(BINARY)
