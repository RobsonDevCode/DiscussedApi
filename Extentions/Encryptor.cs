using DiscussedApi.Configuration;
using System.Security.Cryptography;

namespace DiscussedApi.Extentions
{
    public static class Encryptor
    {
        private static NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        public static bool TryDecrpytToken(string encryptedToken, out long? nextpageToken)
        {
            try
            {
                if(string.IsNullOrEmpty(encryptedToken))
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
            catch(Exception ex)
            {
                _logger.Error(ex.Message);
                nextpageToken = null;
                return false;
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
            catch(Exception ex)
            {
                _logger.Error(ex.Message);
                encryptedString = "";
                return false;
            }
        }

        public static (string key, string iv) GenerateKeyAndIVStrings()
        {
            using (Aes aes = Aes.Create())
            {
                aes.GenerateKey();
                aes.GenerateIV();

                string keyString = Convert.ToBase64String(aes.Key);
                string ivString = Convert.ToBase64String(aes.IV);

                return (keyString, ivString);
            }
        }
    }
}
