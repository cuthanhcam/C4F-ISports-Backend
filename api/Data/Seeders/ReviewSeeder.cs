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
            if (!await context.Reviews.IgnoreQueryFilters().AnyAsync())
            {
                logger?.LogInformation("Seeding Reviews...");
                var user = await context.Users.FirstOrDefaultAsync();
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldName == "Sân ABC");
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
                        User = user,
                        FieldId = field.FieldId,
                        Field = field,
                        Rating = 5,
                        Comment = "Sân rất đẹp, dịch vụ tốt, sẽ quay lại!",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsVisible = true
                    }
                };

                try
                {
                    await context.Reviews.AddRangeAsync(reviews);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Reviews seeded successfully. Reviews: {Count}", await context.Reviews.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed Reviews. StackTrace: {StackTrace}", ex.StackTrace);
                    throw;
                }
            }
            else
            {
                logger?.LogInformation("Reviews already seeded. Skipping...");
            }
        }
    }
}