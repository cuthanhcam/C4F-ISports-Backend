using api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class BookingServiceSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.BookingServices.AnyAsync())
            {
                var booking = await context.Bookings.FirstOrDefaultAsync();
                var fieldService = await context.FieldServices.FirstOrDefaultAsync(fs => fs.ServiceName == "Nước uống");
                if (booking != null && fieldService != null)
                {
                    var bookingServices = new[]
                    {
                        new BookingService
                        {
                            BookingId = booking.BookingId,
                            FieldServiceId = fieldService.FieldServiceId,
                            Quantity = 2,
                            Price = fieldService.Price * 2,
                            Description = "2 chai nước suối cho đội."
                        }
                    };

                    await context.BookingServices.AddRangeAsync(bookingServices);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}