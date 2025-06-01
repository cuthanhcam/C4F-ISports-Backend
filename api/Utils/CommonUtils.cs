using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace api.Utils
{
    public static class CommonUtils
    {
        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Bán kính Trái Đất (km)
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRadians(double deg) => deg * Math.PI / 180;

        private static readonly string[] PrefixesToRemove =
        {
            "thành phố", "thanh pho", "tp", "quận", "quan", "huyện", "huyen", "thị xã", "thi xa"
        };

        public static string NormalizeVietnameseString(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var normalized = input.Normalize(NormalizationForm.FormD);
            var result = new StringBuilder();
            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    result.Append(c);
                }
            }
            return result.ToString().ToLowerInvariant().Trim();
        }

        public static string StandardizeLocationName(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Chuyển về chữ thường và loại bỏ khoảng trắng thừa
            var cleanedInput = input.ToLowerInvariant().Trim();

            // Loại bỏ các tiền tố
            foreach (var prefix in PrefixesToRemove)
            {
                if (cleanedInput.StartsWith(prefix + " "))
                {
                    cleanedInput = cleanedInput.Substring(prefix.Length).Trim();
                    break;
                }
            }

            // Áp dụng NormalizeVietnameseString để bỏ dấu
            return NormalizeVietnameseString(cleanedInput);
        }
    }

    public static class DistributedCacheExtensions
    {
        public static async Task SetRecordAsync<T>(this IDistributedCache cache, string recordId, T data, TimeSpan? absoluteExpireTime = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromMinutes(60)
            };
            var jsonData = System.Text.Json.JsonSerializer.Serialize(data);
            await cache.SetStringAsync(recordId, jsonData, options);
        }

        public static async Task<T> GetRecordAsync<T>(this IDistributedCache cache, string recordId)
        {
            var jsonData = await cache.GetStringAsync(recordId);
            if (jsonData == null)
                return default;
            return System.Text.Json.JsonSerializer.Deserialize<T>(jsonData);
        }
    }
}