namespace IVPN.Models
{
    public class ConnectionResult
    {
        public bool Success { get; }

        public string ErrorMessage { get; }

        public ConnectionInfo ConnectionInfo { get; set; }

        public ConnectionResult(bool isSuccess, string errorMessage, ConnectionInfo info = null)
        {
            Success = isSuccess;
            ErrorMessage = errorMessage;
            ConnectionInfo = info;
        }

        public ConnectionResult(bool isSuccess)
            : this(isSuccess, "")
        {

        }        
    }
}
