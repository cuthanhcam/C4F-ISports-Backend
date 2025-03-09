using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class BookingSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (!context.Bookings.Any())
            {
                var user = context.Users.FirstOrDefault(u => u.Email == "user@example.com");
                var field = context.Fields.FirstOrDefault(f => f.FieldName == "Football Field A");
                if (user != null && field != null)
                {
                    context.Bookings.Add(
                        new Booking
                        {
                            UserId = user.UserId,
                            FieldId = field.FieldId,
                            BookingDate = DateTime.UtcNow.Date,
                            StartTime = TimeSpan.FromHours(14),
                            EndTime = TimeSpan.FromHours(15),
                            TotalPrice = 100000m,
                            Status = "Pending",
                            PaymentStatus = "Unpaid",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    );
                }
            }
        }
    }
}