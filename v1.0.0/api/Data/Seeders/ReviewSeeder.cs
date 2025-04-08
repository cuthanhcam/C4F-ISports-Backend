using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class ReviewSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!context.Reviews.Any())
            {
                var user1 = context.Users.FirstOrDefault(u => u.Email == "user1@gmail.com");
                var user2 = context.Users.FirstOrDefault(u => u.Email == "user2@gmail.com");
                var field1 = context.Fields.FirstOrDefault(f => f.FieldName == "Football Field A");
                var field3 = context.Fields.FirstOrDefault(f => f.FieldName == "Badminton Court X");

                var reviews = new List<Review>();
                if (user1 != null && field1 != null)
                    reviews.Add(new Review { UserId = user1.UserId, FieldId = field1.FieldId, Rating = 4, Comment = "Sân đẹp, dịch vụ tốt.", CreatedAt = DateTime.UtcNow });
                if (user2 != null && field3 != null)
                    reviews.Add(new Review { UserId = user2.UserId, FieldId = field3.FieldId, Rating = 5, Comment = "Rất hài lòng!", CreatedAt = DateTime.UtcNow });

                if (reviews.Any())
                {
                    context.Reviews.AddRange(reviews);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}