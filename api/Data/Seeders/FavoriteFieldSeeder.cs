using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldName == "Sân ABC");
                if (user == null || field == null)
                {
                    logger?.LogError("No User or Field found for seeding FavoriteFields.");
                    return;
                }

                var favoriteFields = new[]
                {
                    new FavoriteField
                    {
                        FavoriteId = 1,
                        UserId = user.UserId,
                        User = user,
                        FieldId = field.FieldId,
                        Field = field,
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
                    logger?.LogError(ex, "Failed to seed FavoriteFields. StackTrace: {StackTrace}", ex.StackTrace);
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