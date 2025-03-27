using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class ReviewSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (!context.Reviews.Any())
            {
                var user1 = context.Users.First(u => u.Email == "user1@gmail.com");
                var user2 = context.Users.First(u => u.Email == "user2@gmail.com");
                var field1 = context.Fields.First(f => f.FieldName == "Football Field A");
                var field3 = context.Fields.First(f => f.FieldName == "Badminton Court X");

                context.Reviews.AddRange(
                    new Review { UserId = user1.UserId, FieldId = field1.FieldId, Rating = 4, Comment = "Sân đẹp, dịch vụ tốt.", CreatedAt = DateTime.UtcNow },
                    new Review { UserId = user2.UserId, FieldId = field3.FieldId, Rating = 5, Comment = "Rất hài lòng!", CreatedAt = DateTime.UtcNow }
                );
                context.SaveChanges();
            }
        }
    }
}