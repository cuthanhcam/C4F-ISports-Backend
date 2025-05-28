using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace api.Utils
{
    public static class DistributedCacheExtensions
    {
        public static async Task<T?> GetRecordAsync<T>(this IDistributedCache cache, string key)
        {
            var data = await cache.GetStringAsync(key);
            
            if (data == null)
                return default;

            return JsonSerializer.Deserialize<T>(data);
        }

        public static async Task SetRecordAsync<T>(this IDistributedCache cache, 
            string key, T data, TimeSpan? absoluteExpireTime = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromMinutes(60)
            };

            var jsonData = JsonSerializer.Serialize(data);
            await cache.SetStringAsync(key, jsonData, options);
        }
    }
}