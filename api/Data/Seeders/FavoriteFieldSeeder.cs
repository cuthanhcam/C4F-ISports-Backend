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
            if (!await context.FavoriteFields.AnyAsync())
            {
                logger?.LogInformation("Seeding FavoriteFields...");
                var user = await context.Users.FirstOrDefaultAsync();
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldName == "Sân bóng Cầu Giấy");
                if (user == null || field == null)
                {
                    logger?.LogError("No User or Field found for seeding FavoriteFields.");
                    return;
                }

                var favorites = new[]
                {
                    new FavoriteField
                    {
                        UserId = user.UserId,
                        User = user, // Gán navigation property
                        FieldId = field.FieldId,
                        Field = field, // Gán navigation property
                        AddedDate = DateTime.UtcNow
                    }
                };

                await context.FavoriteFields.AddRangeAsync(favorites);
                await context.SaveChangesAsync();
                logger?.LogInformation("FavoriteFields seeded successfully. FavoriteFields: {Count}", await context.FavoriteFields.CountAsync());
            }
        }
    }
}