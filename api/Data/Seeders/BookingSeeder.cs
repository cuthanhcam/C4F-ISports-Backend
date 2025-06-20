using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class BookingSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.Bookings.IgnoreQueryFilters().AnyAsync())
            {
                logger?.LogInformation("Seeding Bookings...");
                var user = await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync();
                var subField = await context.SubFields.IgnoreQueryFilters().FirstOrDefaultAsync(sf => sf.SubFieldName == "Sân 5A");
                var promotion = await context.Promotions.IgnoreQueryFilters().FirstOrDefaultAsync();

                if (user == null || subField == null)
                {
                    logger?.LogError("No User or SubField found for seeding Bookings.");
                    return;
                }

                try
                {
                    var booking = new Booking
                    {
                        UserId = user.UserId,
                        SubFieldId = subField.SubFieldId,
                        BookingDate = DateTime.UtcNow.AddDays(1),
                        TotalPrice = 450000m,
                        Status = "Confirmed", // Đảm bảo khớp với RegularExpression
                        PaymentStatus = "Paid", // Đảm bảo khớp với RegularExpression
                        Notes = "Đặt sân cho đội bóng công ty.",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsReminderSent = false,
                        PromotionId = promotion?.PromotionId // Chỉ gán nếu promotion tồn tại
                    };

                    var timeSlots = new List<BookingTimeSlot>
            {
                new BookingTimeSlot
                {
                    StartTime = TimeSpan.Parse("17:00"),
                    EndTime = TimeSpan.Parse("17:30"),
                    Price = 225000m
                },
                new BookingTimeSlot
                {
                    StartTime = TimeSpan.Parse("17:30"),
                    EndTime = TimeSpan.Parse("18:00"),
                    Price = 225000m
                }
            };

                    await context.Bookings.AddAsync(booking);
                    await context.SaveChangesAsync();

                    foreach (var slot in timeSlots)
                    {
                        slot.BookingId = booking.BookingId;
                    }

                    booking.TimeSlots = timeSlots;
                    await context.BookingTimeSlots.AddRangeAsync(timeSlots);
                    await context.SaveChangesAsync();

                    logger?.LogInformation("Bookings seeded successfully. Bookings: {Count}", await context.Bookings.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Error seeding Bookings.");
                    throw;
                }
            }
        }
    }
}