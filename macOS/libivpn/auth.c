#include <ServiceManagement/ServiceManagement.h>

int main(int argc, char **argv) {
    OSStatus err;
    AuthorizationExternalForm extForm;
    AuthorizationRef authRef;
    CFErrorRef error;

    err = AuthorizationCreate(NULL, NULL, 0, &authRef);
    if(err == errAuthorizationSuccess) {
        puts("auth created");
        
        Boolean success = SMJobBless(kSMDomainSystemLaunchd, CFSTR("net.ivpn.client.Helper"), authRef, &error);
        if(success) {
            puts("SUCCESS!");
        } else {
            puts("ERROR");
            puts( CFStringGetCStringPtr(CFErrorCopyDescription(error), kCFStringEncodingMacRoman) );
            //puts( [(__bridge NSError *)error description] );
            CFRelease(error);
        }
    }
    
    puts("authorization error");
}
