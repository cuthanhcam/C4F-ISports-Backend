using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class BookingServiceSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.BookingServices.IgnoreQueryFilters().AnyAsync())
            {
                logger?.LogInformation("Seeding BookingServices...");
                var booking = await context.Bookings.IgnoreQueryFilters().FirstOrDefaultAsync();
                var fieldService = await context.FieldServices.IgnoreQueryFilters().FirstOrDefaultAsync();
                if (booking == null || fieldService == null)
                {
                    logger?.LogError("No Booking or FieldService found for seeding BookingServices.");
                    return;
                }

                var bookingServices = new[]
                {
            new BookingService
            {
                BookingId = booking.BookingId,
                FieldServiceId = fieldService.FieldServiceId,
                Quantity = 5,
                Price = fieldService.Price * 5,
                Description = "Mua 5 chai nước suối"
            }
        };

                try
                {
                    await context.BookingServices.AddRangeAsync(bookingServices);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("BookingServices seeded successfully. BookingServices: {Count}", await context.BookingServices.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed BookingServices.");
                    throw;
                }
            }
            else
            {
                logger?.LogInformation("BookingServices already seeded. Skipping...");
            }
        }
    }
}