using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class BookingTimeSlotSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.BookingTimeSlots.AnyAsync())
            {
                logger?.LogInformation("Seeding BookingTimeSlots...");
                var booking = await context.Bookings.FirstOrDefaultAsync();
                if (booking == null)
                {
                    logger?.LogError("No Booking found for seeding BookingTimeSlots.");
                    return;
                }

                var timeSlots = new[]
                {
                    new BookingTimeSlot
                    {
                        BookingId = booking.BookingId,
                        Booking = booking, // Gán navigation property
                        StartTime = TimeSpan.Parse("17:00"),
                        EndTime = TimeSpan.Parse("18:00"),
                        Price = 450000m
                    }
                };

                await context.BookingTimeSlots.AddRangeAsync(timeSlots);
                await context.SaveChangesAsync();
                logger?.LogInformation("BookingTimeSlots seeded successfully. BookingTimeSlots: {Count}", await context.BookingTimeSlots.CountAsync());
            }
        }
    }
}