using DiscussedApi.Configuration;
using DiscussedApi.Reopisitory.Auth;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;

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
        public async Task<string> DecryptStringAsync(string value, Guid id)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value), "Invalid encryption parameters, params can't be null");

            var credentials = await _authDataAccess.GetKeyAndIvAsync(id);

            if (credentials == null)
               throw new Exception("Trouble getting key and iv for decryption");
            
            if(credentials.ExpireTime > DateTime.UtcNow)
            {
                await _authDataAccess.DeleteKeyAndIvByIdAsync(id);
                throw new Exception("Key and Iv have Expired");
            }

            if (!isBase64String(value) || !isBase64String(credentials.Key) || !isBase64String(credentials.Iv))
                throw new ArgumentException("Invalid Base64 input in encryption parameters.");

            byte[] encryptedBytes = Convert.FromBase64String(value);
            byte[] key = Convert.FromBase64String(credentials.Key);
            byte[] iv = Convert.FromBase64String(credentials.Iv);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aes.CreateDecryptor(key, iv);

                using (MemoryStream memoryStream = new MemoryStream(encryptedBytes))
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                using (StreamReader reader = new StreamReader(cryptoStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public async Task<(string UsernameOrEmail, string Password)> DecryptCredentials(string encryptedEmailOrUsername, string encryptedPassword, Guid keyId)
        {
            var decryptEmail = DecryptStringAsync(encryptedEmailOrUsername, keyId);
            var decryptPassword = DecryptStringAsync(encryptedPassword, keyId);

            Task[] decryptCredentials = new[] {
                    decryptEmail,
                    decryptPassword
                };

            Task.WaitAll(decryptCredentials);

            return (await decryptEmail, await decryptPassword);
        }

       
        private static bool isBase64String(string s)
        {
            Span<byte> buffer = new Span<byte>(new byte[s.Length]);
            return Convert.TryFromBase64String(s, buffer, out _);
        }

    }
}
