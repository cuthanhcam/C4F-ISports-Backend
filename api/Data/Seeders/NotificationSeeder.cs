using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using api.Interfaces;

namespace api.Data.Seeders
{
    public static class NotificationSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, IEmailSender emailSender, ILogger logger = null)
        {
            if (!await context.Notifications.IgnoreQueryFilters().AnyAsync())
            {
                logger?.LogInformation("Seeding Notifications...");
                var user = await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync();
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
                        Title = "Xác nhận đặt sân",
                        Content = "Đặt sân của bạn tại Sân ABC đã được xác nhận.",
                        NotificationType = "Booking",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                try
                {
                    await context.Notifications.AddRangeAsync(notifications);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Notifications seeded successfully. Notifications: {Count}", await context.Notifications.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed Notifications.");
                    throw;
                }
            }
            else
            {
                logger?.LogInformation("Notifications already seeded. Skipping...");
            }
        }
    }
}