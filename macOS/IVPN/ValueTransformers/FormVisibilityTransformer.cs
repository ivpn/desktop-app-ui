using System;

using Foundation;
using IVPN.Models;

namespace IVPN
{
    public class FormVisibilityTransformer: NSValueTransformer
    {
        protected NSObject ToNSBoolean(bool value)
        {
            return NSNumber.FromBoolean(value);
        }

        protected ServiceState GetServiceState(NSObject value)
        {
            var intValue = (int)(NSNumber)value;
            return (ServiceState)intValue;
        }

        protected bool IsConnected(NSObject value)
        {
            var state = GetServiceState(value);

            if (state == ServiceState.Connected)
                return true;

            return false;
        }

        protected bool IsConnecting(NSObject value)
        {
            var state = GetServiceState(value);

            if (state == ServiceState.Connecting ||
                state == ServiceState.Disconnecting ||
                state == ServiceState.CancellingConnection ||
                state == ServiceState.ReconnectingOnClient ||
                state == ServiceState.ReconnectingOnService)
                return true;

            return false;
        }

        protected bool IsDisconnected(NSObject value)
        {
            var state = GetServiceState(value);

            if (state == ServiceState.Disconnected ||
                state == ServiceState.Uninitialized)
                return true;

            return false;
        }
    }
}

