using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Caching;
using Volo.Abp.Timing;

namespace Pusula.Student.Automation.Tests
{
    public class RedisCacheTestAppService : ApplicationService, IRedisCacheTestAppService
    {
        private readonly IDistributedCache<string> _cache;

        public RedisCacheTestAppService(IDistributedCache<string> cache)
        {
            _cache = cache;
        }

        public async Task<string> GetOrSetAsync(string key)
        {
            var cacheKey = $"demo:{key}";
            var cached = await _cache.GetAsync(cacheKey);
            if (!string.IsNullOrEmpty(cached))
                return $"Cache’den geldi: {cached}";

            var value = $"Redis cache test {Clock.Now:HH:mm:ss}";
            await _cache.SetAsync(cacheKey, value, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            return $"Yeni oluşturuldu: {value}";
        }
    }
}
