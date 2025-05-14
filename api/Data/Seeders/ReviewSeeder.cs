using api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class ReviewSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.Reviews.AnyAsync())
            {
                var user = await context.Users.FirstOrDefaultAsync();
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldName == "Sân bóng Cầu Giấy");
                if (user != null && field != null)
                {
                    var reviews = new[]
                    {
                        new Review
                        {
                            UserId = user.UserId,
                            FieldId = field.FieldId,
                            Rating = 4,
                            Comment = "Sân sạch sẽ, dịch vụ tốt, nhưng cần thêm quạt mát.",
                            CreatedAt = DateTime.UtcNow,
                            IsVisible = true
                        }
                    };

                    await context.Reviews.AddRangeAsync(reviews);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}