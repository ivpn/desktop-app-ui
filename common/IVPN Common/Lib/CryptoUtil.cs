using System;

namespace IVPN.Lib
{
    public class CryptoUtil
    {
        static byte[] entropy = System.Text.Encoding.UTF8.GetBytes("McNfJg5jJ;jJ.cL:'[!80^!*-dfBJnkd~!KPr3408");

        public static string EncryptString(string password)
        {
            try 
            {
                if (password == null)
                    password = "";
                
                byte [] encryptedData = System.Security.Cryptography.ProtectedData.Protect (
                    System.Text.Encoding.Unicode.GetBytes (password),
                    entropy,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);

                return Convert.ToBase64String (encryptedData);
            }
            catch (Exception ex)
            {
                Logging.Info(string.Format("Encryption error: {0}", ex));
                throw;
            }
        }

        public static string DecryptString(string encryptedData)
        {
            if (string.IsNullOrEmpty(encryptedData))
                return "";
            
            try
            {
                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    entropy,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);

                return System.Text.Encoding.Unicode.GetString(decryptedData);
            }
            catch (Exception ex)
            {
                Logging.Info(string.Format("Decryption error: {0}", ex));
                return "";
            }
        }
    }
}
