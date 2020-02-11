using System;

using Foundation;

using IVPN.Models;

namespace IVPN
{
    [Register("IsConnectedFormHiddenTransformer")]
    public class IsConnectedFormHiddenTransformer: FormVisibilityTransformer
    {
        public IsConnectedFormHiddenTransformer()
        {
            
        }

        public override NSObject TransformedValue(NSObject value)
        {
            return ToNSBoolean(!IsConnected(value));
        }
    }
}

