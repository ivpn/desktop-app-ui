using System;

using Foundation;

using IVPN.Models;

namespace IVPN
{
    [Register("IsConnectedFormVisibleTransformer")]
    public class IsConnectedFormVisibleTransformer: FormVisibilityTransformer
    {
        public IsConnectedFormVisibleTransformer()
        {
        }

        public override NSObject TransformedValue(NSObject value)
        {
            return ToNSBoolean(IsConnected(value));
        }
    }
}

