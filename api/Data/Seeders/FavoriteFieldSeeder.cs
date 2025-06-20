using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class FavoriteFieldSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.FavoriteFields.IgnoreQueryFilters().AnyAsync())
            {
                logger?.LogInformation("Seeding FavoriteFields...");
                var user = await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync();
                var field = await context.Fields.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.FieldName == "Sân ABC");

                if (user == null || field == null)
                {
                    logger?.LogError("No User or Field found for seeding FavoriteFields.");
                    return;
                }

                var favoriteFields = new[]
                {
            new FavoriteField
            {
                UserId = user.UserId,
                FieldId = field.FieldId,
                AddedDate = DateTime.UtcNow
            }
        };

                try
                {
                    await context.FavoriteFields.AddRangeAsync(favoriteFields);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("FavoriteFields seeded successfully. FavoriteFields: {Count}", await context.FavoriteFields.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed FavoriteFields.");
                    throw;
                }
            }
            else
            {
                logger?.LogInformation("FavoriteFields already seeded. Skipping...");
            }
        }
    }
}