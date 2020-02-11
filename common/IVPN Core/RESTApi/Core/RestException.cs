using System;
namespace IVPN.RESTApi.Core
{
    /// <summary>
    /// Base class for all REST communication exceptions
    /// </summary>
    public class RestException : Exception
    {
        public RestException (string description)
            : base (description) {}

        public RestException (string description, Exception ex)
            : base (description, ex) {}
    }
}
