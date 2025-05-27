using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class BookingTimeSlotSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.BookingTimeSlots.IgnoreQueryFilters().AnyAsync())
            {
                logger?.LogInformation("Seeding BookingTimeSlots...");
                var booking = await context.Bookings.FirstOrDefaultAsync();
                if (booking == null)
                {
                    logger?.LogError("No Booking found for seeding BookingTimeSlots.");
                    return;
                }

                var bookingTimeSlots = new[]
                {
                    new BookingTimeSlot
                    {
                        BookingId = booking.BookingId,
                        Booking = booking,
                        StartTime = new TimeSpan(17, 0, 0),
                        EndTime = new TimeSpan(18, 0, 0),
                        Price = 450000
                    }
                };

                try
                {
                    await context.BookingTimeSlots.AddRangeAsync(bookingTimeSlots);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("BookingTimeSlots seeded successfully. BookingTimeSlots: {Count}", await context.BookingTimeSlots.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed BookingTimeSlots. StackTrace: {StackTrace}", ex.StackTrace);
                    throw;
                }
            }
            else
            {
                logger?.LogInformation("BookingTimeSlots already seeded. Skipping...");
            }
        }
    }
}