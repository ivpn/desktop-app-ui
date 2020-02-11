using System;

namespace IVPN
{
    public class IVPNClientProxyNotConnectedException : Exception
    {
        public IVPNClientProxyNotConnectedException(string message)
            : base(message)
        {

        }
    }
}
