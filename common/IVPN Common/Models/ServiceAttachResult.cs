using System;

namespace IVPN.Models
{
    public class ServiceAttachResult
    {
        public ServiceAttachResult(int port, UInt64 secret)
        {
            IsError = false;
            Port = port;
            Secret = secret;
        }

        public ServiceAttachResult(string errorMessage = "")
        {
            IsError = true;
            ErrorMessage = errorMessage;
            Port = 0;
        }

        public string ErrorMessage { get; set; }

        public bool IsError { get; }

        public int Port { get; }
        public UInt64 Secret { get; }
    }
}
