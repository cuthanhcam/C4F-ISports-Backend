using api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class PaymentSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.Payments.AnyAsync())
            {
                var booking = await context.Bookings.FirstOrDefaultAsync();
                if (booking != null)
                {
                    var payments = new[]
                    {
                        new Payment
                        {
                            BookingId = booking.BookingId,
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
                }
            }
        }
    }
}