using api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class FavoriteFieldSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.FavoriteFields.AnyAsync())
            {
                var user = await context.Users.FirstOrDefaultAsync();
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldName == "Sân bóng Cầu Giấy");
                if (user != null && field != null)
                {
                    var favorites = new[]
                    {
                        new FavoriteField
                        {
                            UserId = user.UserId,
                            FieldId = field.FieldId,
                            AddedDate = DateTime.UtcNow
                        }
                    };

                    await context.FavoriteFields.AddRangeAsync(favorites);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}