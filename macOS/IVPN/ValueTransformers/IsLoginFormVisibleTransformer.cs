using System;

using Foundation;

using IVPN.Models;

namespace IVPN
{
    [Register("IsLoginFormVisibleTransformer")]
    public class IsLoginFormVisibleTransformer: FormVisibilityTransformer
    {
        public IsLoginFormVisibleTransformer()
        {
        }

        public override NSObject TransformedValue(NSObject value)
        {
            return ToNSBoolean(IsDisconnected(value));
        }
    }
}

