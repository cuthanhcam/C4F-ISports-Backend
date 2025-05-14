using api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class BookingTimeSlotSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.BookingTimeSlots.AnyAsync())
            {
                var booking = await context.Bookings.FirstOrDefaultAsync();
                if (booking != null)
                {
                    var timeSlots = new[]
                    {
                        new BookingTimeSlot
                        {
                            BookingId = booking.BookingId,
                            StartTime = TimeSpan.Parse("17:00"),
                            EndTime = TimeSpan.Parse("18:00"),
                            Price = 450000m
                        }
                    };

                    await context.BookingTimeSlots.AddRangeAsync(timeSlots);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}