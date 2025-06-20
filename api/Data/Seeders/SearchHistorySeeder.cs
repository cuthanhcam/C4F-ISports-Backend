using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace api.Data.Seeders
{
    public static class SearchHistorySeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, IDistributedCache cache, ILogger logger = null)
        {
            if (!await context.SearchHistories.IgnoreQueryFilters().AnyAsync())
            {
                logger?.LogInformation("Seeding SearchHistories...");
                var user = await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync();
                var field = await context.Fields.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.FieldName == "Sân ABC");
                if (user == null)
                {
                    logger?.LogError("No User found for seeding SearchHistories.");
                    return;
                }

                var searchHistories = new[]
                {
                    new SearchHistory
                    {
                        UserId = user.UserId,
                        Keyword = "Sân bóng Quận 1",
                        SearchDateTime = DateTime.UtcNow,
                        FieldId = field?.FieldId,
                        Latitude = 10.7769m,
                        Longitude = 106.7009m
                    }
                };

                try
                {
                    await context.SearchHistories.AddRangeAsync(searchHistories);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("SearchHistories seeded successfully. SearchHistories: {Count}", await context.SearchHistories.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed SearchHistories.");
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