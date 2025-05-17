using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class SearchHistorySeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
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
                        User = user, // Gán navigation property
                        SearchQuery = "Sân bóng Cầu Giấy",
                        SearchDate = DateTime.UtcNow,
                        FieldId = field.FieldId,
                        Latitude = 21.030123m,
                        Longitude = 105.801456m
                    }
                };

                await context.SearchHistories.AddRangeAsync(searchHistories);
                await context.SaveChangesAsync();
                logger?.LogInformation("SearchHistories seeded successfully. SearchHistories: {Count}", await context.SearchHistories.CountAsync());
            }
        }
    }
}