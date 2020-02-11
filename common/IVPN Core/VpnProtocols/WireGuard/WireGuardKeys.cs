using System.Threading.Tasks;
using IVPN.Exceptions;
using IVPN.Shell;

namespace IVPN.VpnProtocols.WireGuard
{
    public class Keys
    {

        /// <summary>
        /// Generate new pair of Private\Public keys
        /// </summary>
        /// <returns>Public key</returns>
        /// <param name="privateKey">Private key</param>
        private static string GenerateKeys(out string privateKey)
        {
            // private key generation
            ShellCommandResult result = ShellCommand.RunCommand(Platform.WireGuardWgExecutablePath, $"genkey", null, 5000, true);
            if (result.ExitCode != 0)
                throw new IVPNException($"Private key generation error: {result.ExitCode}" + ((result.ErrorOutput == null) ? "" : $" Error: {result.ErrorOutput}"));
            privateKey = result.Output.Trim();

            // public key generation
            result = ShellCommand.RunCommand(Platform.WireGuardWgExecutablePath, $"pubkey", privateKey, 5000, true);
            if (result.ExitCode != 0)
                throw new IVPNException($"Public key generation error: {result.ExitCode}" + ((result.ErrorOutput == null) ? "" : $" Error: {result.ErrorOutput}"));

            return result.Output.Trim();
        }

        public static async Task<string[]> GenerateKeysAsync()
        {
            string[] ret = new string[2];

            string privateKey = "";
            string publicKey = "";

            await Task.Run(() => publicKey = GenerateKeys(out privateKey));

            ret[0] = privateKey;
            ret[1] = publicKey;

            return ret;
        }
    }
}
