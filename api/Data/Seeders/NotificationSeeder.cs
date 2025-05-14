using api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class NotificationSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.Notifications.AnyAsync())
            {
                var user = await context.Users.FirstOrDefaultAsync();
                if (user != null)
                {
                    var notifications = new[]
                    {
                        new Notification
                        {
                            UserId = user.UserId,
                            Title = "Đặt sân thành công",
                            Content = "Bạn đã đặt sân 5A tại Sân bóng Cầu Giấy vào 17:00 ngày mai.",
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow,
                            NotificationType = "Booking"
                        }
                    };

                    await context.Notifications.AddRangeAsync(notifications);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}