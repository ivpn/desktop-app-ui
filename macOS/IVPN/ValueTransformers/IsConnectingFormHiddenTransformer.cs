using System;
using Foundation;

using IVPN.Models;

namespace IVPN
{

    [Register("IsConnectingFormHiddenTransformer")]
    public class IsConnectingFormHiddenTransformer: FormVisibilityTransformer
    {
        public IsConnectingFormHiddenTransformer()
        {
        }

        public override NSObject TransformedValue(NSObject value)
        {
            return ToNSBoolean(!IsConnecting(value));
        }
    }
}

