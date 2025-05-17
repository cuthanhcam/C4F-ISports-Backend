using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class BookingSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.Bookings.AnyAsync())
            {
                logger?.LogInformation("Seeding Bookings...");
                var user = await context.Users.FirstOrDefaultAsync();
                var subField = await context.SubFields.FirstOrDefaultAsync(sf => sf.SubFieldName == "Sân 5A");
                var promotion = await context.Promotions.FirstOrDefaultAsync();
                if (user == null || subField == null)
                {
                    logger?.LogError("No User or SubField found for seeding Bookings.");
                    return;
                }

                var bookings = new[]
                {
                    new Booking
                    {
                        UserId = user.UserId,
                        User = user, // Gán navigation property
                        SubFieldId = subField.SubFieldId,
                        SubField = subField, // Gán navigation property
                        BookingDate = DateTime.UtcNow.AddDays(1),
                        StartTime = TimeSpan.Parse("17:00"),
                        EndTime = TimeSpan.Parse("18:00"),
                        TotalPrice = 450000m, // Áp dụng khuyến mãi 10%
                        Status = "Confirmed",
                        PaymentStatus = "Paid",
                        Notes = "Đặt sân cho đội bóng công ty.",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsReminderSent = false,
                        PromotionId = promotion?.PromotionId
                    }
                };

                await context.Bookings.AddRangeAsync(bookings);
                await context.SaveChangesAsync();
                logger?.LogInformation("Bookings seeded successfully. Bookings: {Count}", await context.Bookings.CountAsync());
            }
        }
    }
}