using DiscussedApi.Extentions;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace DiscussedApi.Processing.Comments
{
    public static class CachingSettings
    {
        private static NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        internal static MemoryCacheEntryOptions SetCommentCacheSettings()
        {
            return new MemoryCacheEntryOptions()
                                      .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                                      .SetAbsoluteExpiration(TimeSpan.FromMinutes(1))
                                      .SetPriority(CacheItemPriority.High);

        }

        internal static MemoryCacheEntryOptions SetReplyCommentSettings()
        {
            return new MemoryCacheEntryOptions()
                                    .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(2))
                                    .SetPriority(CacheItemPriority.Normal);
        }

        internal static (string Key, long? Token) GenerateCacheKeyFromToken(string encryptedToken, string baseKey)
        {
            try
            {

                if (!Encryptor.TryDecrpytToken(encryptedToken, out long? nextPageToken)) //Decrypt encryptedToken to get paging Reference 
                    throw new CryptographicException("error while decrypting value, check the Value, Key or Iv");

                if (!nextPageToken.HasValue)
                    throw new CryptographicException("next page token returned null after attempting decrypting");

                return ($"{baseKey}-{nextPageToken}", nextPageToken);// adjust the key so we dont get previous cache

            }
            catch(CryptographicException ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
            catch (ArgumentNullException ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }

        }
    }
}
