using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class PaymentSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.Payments.AnyAsync())
            {
                logger?.LogInformation("Seeding Payments...");
                var booking = await context.Bookings.FirstOrDefaultAsync();
                if (booking == null)
                {
                    logger?.LogError("No Booking found for seeding Payments.");
                    return;
                }

                var payments = new[]
                {
                    new Payment
                    {
                        BookingId = booking.BookingId,
                        Booking = booking, // Gán navigation property
                        Amount = booking.TotalPrice,
                        PaymentMethod = "CreditCard",
                        TransactionId = "TXN_" + Guid.NewGuid().ToString(),
                        Status = "Success",
                        Currency = "VND",
                        CreatedAt = DateTime.UtcNow,
                        PaymentDate = DateTime.UtcNow
                    }
                };

                await context.Payments.AddRangeAsync(payments);
                await context.SaveChangesAsync();
                logger?.LogInformation("Payments seeded successfully. Payments: {Count}", await context.Payments.CountAsync());
            }
        }
    }
}