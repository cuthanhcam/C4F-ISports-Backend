using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class BookingSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!context.Bookings.Any())
            {
                var user1 = context.Users.First(u => u.Email == "user1@gmail.com");
                var subField1 = context.SubFields.First(sf => sf.SubFieldName == "Sân 1");

                context.Bookings.AddRange(
                    new Booking
                    {
                        UserId = user1.UserId,
                        SubFieldId = subField1.SubFieldId,
                        BookingDate = DateTime.Parse("2024-03-01"), // Sửa chuỗi ngày hợp lệ
                        StartTime = TimeSpan.Parse("07:00"),
                        EndTime = TimeSpan.Parse("08:00"),
                        TotalPrice = 50000,
                        Status = "Confirmed",
                        PaymentStatus = "Paid",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                );
                await context.SaveChangesAsync(); // Đổi thành async
            }
        }
    }
}