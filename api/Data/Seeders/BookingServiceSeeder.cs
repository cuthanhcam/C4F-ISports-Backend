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
            if (!await context.BookingServices.AnyAsync())
            {
                logger?.LogInformation("Seeding BookingServices...");
                var booking = await context.Bookings.FirstOrDefaultAsync();
                var fieldService = await context.FieldServices.FirstOrDefaultAsync(fs => fs.ServiceName == "Nước uống");
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
                        Booking = booking, // Gán navigation property
                        FieldServiceId = fieldService.FieldServiceId,
                        FieldService = fieldService, // Gán navigation property
                        Quantity = 2,
                        Price = fieldService.Price * 2,
                        Description = "2 chai nước suối cho đội."
                    }
                };

                await context.BookingServices.AddRangeAsync(bookingServices);
                await context.SaveChangesAsync();
                logger?.LogInformation("BookingServices seeded successfully. BookingServices: {Count}", await context.BookingServices.CountAsync());
            }
        }
    }
}