using api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class SearchHistorySeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.SearchHistories.AnyAsync())
            {
                var user = await context.Users.FirstOrDefaultAsync();
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldName == "Sân bóng Cầu Giấy");
                if (user != null && field != null)
                {
                    var searchHistories = new[]
                    {
                        new SearchHistory
                        {
                            UserId = user.UserId,
                            SearchQuery = "Sân bóng Cầu Giấy",
                            SearchDate = DateTime.UtcNow,
                            FieldId = field.FieldId,
                            Latitude = 21.030123m,
                            Longitude = 105.801456m
                        }
                    };

                    await context.SearchHistories.AddRangeAsync(searchHistories);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}