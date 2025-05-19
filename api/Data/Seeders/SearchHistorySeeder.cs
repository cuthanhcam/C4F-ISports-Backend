using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class SearchHistorySeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, IDistributedCache cache = null, ILogger logger = null)
        {
            if (!await context.SearchHistories.AnyAsync())
            {
                logger?.LogInformation("Seeding SearchHistories...");
                var user = await context.Users.FirstOrDefaultAsync();
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldName == "Sân bóng Cầu Giấy");
                if (user == null || field == null)
                {
                    logger?.LogError("No User or Field found for seeding SearchHistories.");
                    return;
                }

                var searchHistories = new[]
                {
                    new SearchHistory
                    {
                        UserId = user.UserId,
                        User = user,
                        SearchQuery = "Sân bóng Cầu Giấy",
                        SearchDate = DateTime.UtcNow,
                        FieldId = field.FieldId,
                        Latitude = field.Latitude,
                        Longitude = field.Longitude
                    }
                };

                try
                {
                    await context.SearchHistories.AddRangeAsync(searchHistories);
                    await context.SaveChangesAsync();

                    if (cache != null)
                    {
                        try
                        {
                            var cacheKey = $"SearchHistory_User_{user.UserId}";
                            var cacheValue = JsonSerializer.Serialize(searchHistories);
                            await cache.SetStringAsync(cacheKey, cacheValue, new DistributedCacheEntryOptions
                            {
                                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
                            });
                            logger?.LogInformation("Search history cached for UserId: {UserId}", user.UserId);
                        }
                        catch (Exception ex)
                        {
                            logger?.LogWarning("Failed to cache search history for UserId: {UserId}: {Error}", user.UserId, ex.Message);
                        }
                    }

                    logger?.LogInformation("SearchHistories seeded successfully. SearchHistories: {Count}", await context.SearchHistories.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed SearchHistories. StackTrace: {StackTrace}", ex.StackTrace);
                    throw;
                }
            }
            else
            {
                logger?.LogInformation("SearchHistories already seeded. Skipping...");
            }
        }
    }
}