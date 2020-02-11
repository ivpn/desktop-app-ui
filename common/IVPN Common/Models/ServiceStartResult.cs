namespace IVPN.Models
{
    public class ServiceStartResult
    {
        public ServiceStartResult(bool isError, string errorMessage = "")
        {
            IsError = isError;
            ErrorMessage = errorMessage;
        }

        public string ErrorMessage { get; set; }

        public bool IsError { get; }
    }
}
