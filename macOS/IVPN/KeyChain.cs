using System.Text;
using Foundation;
using Security;

namespace IVPN
{
    public class KeyChain
    {
        private const string ServiceName = "net.ivpn.client.IVPN";

        #region Base methods
        public static string GetCredentialFromFromKeychain(string username)
        {
            byte[] passwordBytes;

            if (SecKeyChain.FindGenericPassword(ServiceName, username, out passwordBytes) != SecStatusCode.Success)
                return null;

            if (passwordBytes == null || passwordBytes.Length == 0)
                return null;

            return Encoding.UTF8.GetString(passwordBytes);
        }

        public static void SaveCredentialToKeychain(string username, string password)
        {
            SecStatusCode code = SecStatusCode.NotAvailable;

            byte[] passwordBytes;

            code = SecKeyChain.FindGenericPassword(ServiceName, username, out passwordBytes);
            if (code == SecStatusCode.Success)
            {
                code = SecKeyChain.Update(new SecRecord(SecKind.GenericPassword)
                {
                    Service = ServiceName,
                    Account = username
                }, new SecRecord(SecKind.GenericPassword)
                {
                    Account = username,
                    ValueData = NSData.FromString(password)
                });
            }
            else
            {
                code = SecKeyChain.AddGenericPassword(ServiceName, username, System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public static void RemoveCredentialFromKeychain(string username)
        {
            SecStatusCode code = SecStatusCode.NotAvailable;

            code = SecKeyChain.Remove(new SecRecord(SecKind.GenericPassword)
            {
                Service = ServiceName,
                Account = username
            });

            if (code != SecStatusCode.Success)
                Logging.Info("error removing password from keychain: " + code);
        }
        #endregion //Base methods

        #region Universal methods
        private static string MakeKey(string username, string key) 
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(key))
                return null;

            return $"{username}::{key}"; 
        }

        public static string GetSecuredValueFromKeychain(string username, string key)
        {
            string theKey = MakeKey(username, key);
            if (string.IsNullOrEmpty(theKey))
                return null;

            return GetCredentialFromFromKeychain(theKey);
        }

        public static void SaveSecuredValueToKeychain(string username, string key, string value)
        {
            string theKey = MakeKey(username, key);
            if (string.IsNullOrEmpty(theKey))
                return;
            SaveCredentialToKeychain(theKey, value);
        }

        public static void RemoveSecuredValueFromKeychain(string username, string key)
        {
            string theKey = MakeKey(username, key);
            if (string.IsNullOrEmpty(theKey))
                return;

            RemoveCredentialFromKeychain(theKey);
        }
        #endregion //Universal methods
    }
}

