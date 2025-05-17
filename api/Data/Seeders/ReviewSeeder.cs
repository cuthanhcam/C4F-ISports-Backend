using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class ReviewSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.Reviews.AnyAsync())
            {
                logger?.LogInformation("Seeding Reviews...");
                var user = await context.Users.FirstOrDefaultAsync();
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldName == "Sân bóng Cầu Giấy");
                if (user == null || field == null)
                {
                    logger?.LogError("No User or Field found for seeding Reviews.");
                    return;
                }

                var reviews = new[]
                {
                    new Review
                    {
                        UserId = user.UserId,
                        User = user, // Gán navigation property
                        FieldId = field.FieldId,
                        Field = field, // Gán navigation property
                        Rating = 4,
                        Comment = "Sân sạch sẽ, dịch vụ tốt, nhưng cần thêm quạt mát.",
                        CreatedAt = DateTime.UtcNow,
                        IsVisible = true
                    }
                };

                await context.Reviews.AddRangeAsync(reviews);
                await context.SaveChangesAsync();
                logger?.LogInformation("Reviews seeded successfully. Reviews: {Count}", await context.Reviews.CountAsync());
            }
        }
    }
}