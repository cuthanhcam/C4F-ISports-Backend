using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class NotificationSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.Notifications.AnyAsync())
            {
                logger?.LogInformation("Seeding Notifications...");
                var user = await context.Users.FirstOrDefaultAsync();
                if (user == null)
                {
                    logger?.LogError("No User found for seeding Notifications.");
                    return;
                }

                var notifications = new[]
                {
                    new Notification
                    {
                        UserId = user.UserId,
                        User = user, // Gán navigation property
                        Title = "Đặt sân thành công",
                        Content = "Bạn đã đặt sân 5A tại Sân bóng Cầu Giấy vào 17:00 ngày mai.",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow,
                        NotificationType = "Booking"
                    }
                };

                await context.Notifications.AddRangeAsync(notifications);
                await context.SaveChangesAsync();
                logger?.LogInformation("Notifications seeded successfully. Notifications: {Count}", await context.Notifications.CountAsync());
            }
        }
    }
}