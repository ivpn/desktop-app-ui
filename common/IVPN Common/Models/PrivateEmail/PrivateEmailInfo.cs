namespace IVPN.Models.PrivateEmail
{
    /// <summary>
    /// Information about single private email
    /// </summary>
    public class PrivateEmailInfo
    {
        /// <summary> Private email address </summary>
        public string Email { get; }

        /// <summary> Email addres to which will be forwarded all mails</summary>
        public string ForwardToEmail { get; }

        /// <summary> Notes </summary>
        public string Notes { get; }

        public PrivateEmailInfo (string email, string forwardToEmail, string notes)
        {
            Email = email;
            ForwardToEmail = forwardToEmail;
            Notes = notes;
        }
    }
}
