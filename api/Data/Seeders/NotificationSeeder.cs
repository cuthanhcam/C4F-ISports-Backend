using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using api.Interfaces;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class NotificationSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, IEmailSender emailSender = null, ILogger logger = null)
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
                        User = user,
                        Title = "Đặt sân thành công",
                        Content = "Bạn đã đặt sân 5A tại Sân bóng Cầu Giấy vào 17:00 ngày mai.",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow,
                        NotificationType = "Booking"
                    }
                };

                try
                {
                    await context.Notifications.AddRangeAsync(notifications);
                    await context.SaveChangesAsync();

                    if (emailSender != null)
                    {
                        try
                        {
                            await emailSender.SendEmailAsync(
                                user.Account.Email,
                                "Đặt sân thành công",
                                "<h1>Đặt sân thành công!</h1><p>Bạn đã đặt sân 5A tại Sân bóng Cầu Giấy vào 17:00 ngày mai.</p>"
                            );
                            logger?.LogInformation("Email sent to {Email}", user.Account.Email);
                        }
                        catch (Exception ex)
                        {
                            logger?.LogWarning("Failed to send email to {Email}: {Error}", user.Account.Email, ex.Message);
                        }
                    }

                    logger?.LogInformation("Notifications seeded successfully. Notifications: {Count}", await context.Notifications.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed Notifications. StackTrace: {StackTrace}", ex.StackTrace);
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