using System;
using Foundation;

using IVPN.Models;

namespace IVPN
{

    [Register("IsConnectingFormVisibleTransformer")]
    public class IsConnectingFormVisibleTransformer: FormVisibilityTransformer
    {
        public IsConnectingFormVisibleTransformer()
        {
        }

        public override NSObject TransformedValue(NSObject value)
        {
            return ToNSBoolean(IsConnecting(value));
        }
    }
}

