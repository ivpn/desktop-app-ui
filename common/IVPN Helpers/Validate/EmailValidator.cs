namespace IVPN.Helpers.Validate
{
    public class EmailValidator
    {
        public static bool IsEmailValid(string email)
        {
            try 
            {
                var addr = new System.Net.Mail.MailAddress (email);
                return addr.Address == email;
            } 
            catch 
            {
                return false;
            }
        }
    }
}
