namespace IVPN.Models
{
    public class ServiceAttachResult
    {
        public ServiceAttachResult(int port)
        {
            IsError = false;
            Port = port;
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
    }
}
