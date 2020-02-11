using System;
using Foundation;

using IVPN.Models;

namespace IVPN
{
    
    [Register("IsCancelConnectionEnabledTransformer")]
    public class IsCancelConnectionEnabledTransformer: FormVisibilityTransformer
    {
        public IsCancelConnectionEnabledTransformer()
        {
        }

        public override NSObject TransformedValue(NSObject value)
        {
            var state = GetServiceState(value);

            if (state == ServiceState.Connected ||
                state == ServiceState.Connecting ||
                state == ServiceState.ReconnectingOnClient ||
                state == ServiceState.ReconnectingOnService)
                return ToNSBoolean(true);

            return ToNSBoolean(false);
        }
    }
}

