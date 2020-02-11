#include <stdio.h>

#include "libivpn.h"

void onConnect(int port) {
    printf("connected, available on port %d\n", port);
}

int main(int argc, char **argv) {
    connect_to_agent("net.ivpn.client.LaunchAgent", onConnect);
    dispatch_main();
}
