using Microsoft.Extensions.Caching.Memory;

namespace DiscussedApi.Processing.Comments
{
    public static class Caching
    { 
        public static MemoryCacheEntryOptions SetCommentCacheSettings()
        {
           return new MemoryCacheEntryOptions()
                                     .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                                     .SetAbsoluteExpiration(TimeSpan.FromMinutes(1))
                                     .SetPriority(CacheItemPriority.High);
          
        }

    }
}
