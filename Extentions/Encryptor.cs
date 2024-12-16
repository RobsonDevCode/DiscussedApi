using DiscussedApi.Configuration;
using DiscussedApi.Reopisitory.Auth;
using MySqlConnector;
using System.Security.Cryptography;
using System.Text;

namespace DiscussedApi.Extentions
{
    public class Encryptor : IEncryptor
    {
        private readonly IAuthDataAccess _authDataAccess;
        public Encryptor(IAuthDataAccess authDataAccess) 
        {
          _authDataAccess = authDataAccess;
        }
        private static NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        public static bool TryDecrpytToken(string encryptedToken, out long? nextpageToken)
        {
            try
            {
                if (string.IsNullOrEmpty(encryptedToken))
                {
                    nextpageToken = null;
                    return false;
                }

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Settings.Encryption.Key;
                    aes.IV = Settings.Encryption.IV;

                    using (MemoryStream ms = new(Convert.FromBase64String(encryptedToken)))
                    {
                        using (ICryptoTransform decryptor = aes.CreateDecryptor())
                        {
                            using (CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read))
                            {
                                byte[] valueBytes = new byte[8]; // long is 8 bytes 
                                cs.Read(valueBytes, 0, valueBytes.Length);
                                nextpageToken = BitConverter.ToInt64(valueBytes, 0);
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                nextpageToken = null;
                return false;
            }

        }

        public async Task<string> DecryptPassword(string encyptedPassword, Guid id)
        {
            if (string.IsNullOrWhiteSpace(encyptedPassword))
                throw new ArgumentNullException("Invalid encryption parameters, password can't be null");

            var credentials = await _authDataAccess.GetKeyAndIv(id);
            if (credentials == null)
                throw new Exception("Credentails for decrypting password are null");

            string password = string.Empty;

            byte[] encryptedBytes = Convert.FromBase64String(encyptedPassword);
            byte[] key = Convert.FromBase64String(credentials.Key);
            byte[] iv = Convert.FromBase64String(credentials.Iv);
            using (Aes aes = Aes.Create())
            {
                ICryptoTransform decryptor = aes.CreateDecryptor(key, iv) ;
                using (MemoryStream memoryStream = new(encryptedBytes))
                {
                    using (CryptoStream cs = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new(cs))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static bool TryEncryptValue(long value, out string encryptedString)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Settings.Encryption.Key;
                    aes.IV = Settings.Encryption.IV;

                    using (MemoryStream ms = new())
                    {
                        using (ICryptoTransform encryptor = aes.CreateEncryptor())
                        {
                            using (CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write))
                            {
                                byte[] valueBytes = BitConverter.GetBytes(Convert.ToInt64(value));
                                cs.Write(valueBytes, 0, valueBytes.Length);

                                cs.FlushFinalBlock();
                                encryptedString = Convert.ToBase64String(ms.ToArray());
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                encryptedString = "";
                return false;
            }
        }


    }
}
