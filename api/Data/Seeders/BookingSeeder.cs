using api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class BookingSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.Bookings.AnyAsync())
            {
                var user = await context.Users.FirstOrDefaultAsync();
                var subField = await context.SubFields.FirstOrDefaultAsync(sf => sf.SubFieldName == "Sân 5A");
                var promotion = await context.Promotions.FirstOrDefaultAsync();
                if (user != null && subField != null)
                {
                    var bookings = new[]
                    {
                        new Booking
                        {
                            UserId = user.UserId,
                            SubFieldId = subField.SubFieldId,
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
                }
            }
        }
    }
}