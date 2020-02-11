using System;

using Foundation;

using IVPN.Models;

namespace IVPN
{
    [Register("IsLoginFormHiddenTransformer")]
    public class IsLoginFormHiddenTransformer: FormVisibilityTransformer
    {
        public IsLoginFormHiddenTransformer()
        {
        }

        public override NSObject TransformedValue(NSObject value)
        {
            return ToNSBoolean(!IsDisconnected(value));
        }
    }
}

