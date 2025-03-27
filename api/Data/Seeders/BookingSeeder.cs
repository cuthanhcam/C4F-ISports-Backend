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
                var user1 = context.Users.First(u => u.Email == "user1@gmail.com");
                var field1 = context.Fields.First(f => f.FieldName == "Football Field A");
                var field3 = context.Fields.First(f => f.FieldName == "Badminton Court X");

                context.Bookings.AddRange(
                    new Booking
                    {
                        UserId = user1.UserId,
                        FieldId = field1.FieldId,
                        BookingDate = DateTime.UtcNow.AddDays(1),
                        StartTime = TimeSpan.FromHours(18),
                        EndTime = TimeSpan.FromHours(20),
                        TotalPrice = 700000,
                        Status = "Confirmed",
                        PaymentStatus = "Paid",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Booking
                    {
                        UserId = user1.UserId,
                        FieldId = field3.FieldId,
                        BookingDate = DateTime.UtcNow.AddDays(2),
                        StartTime = TimeSpan.FromHours(6),
                        EndTime = TimeSpan.FromHours(8),
                        TotalPrice = 200000,
                        Status = "Pending",
                        PaymentStatus = "Unpaid",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                );
                context.SaveChanges();
            }
        }
    }
}