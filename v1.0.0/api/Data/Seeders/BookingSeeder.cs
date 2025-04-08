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
                var user1 = context.Users.FirstOrDefault(u => u.Email == "user1@gmail.com");
                var user2 = context.Users.FirstOrDefault(u => u.Email == "user2@gmail.com");
                var subField1 = context.SubFields.FirstOrDefault(sf => sf.SubFieldName == "Sân 1" && sf.Field.FieldName == "Football Field A");
                var subField2 = context.SubFields.FirstOrDefault(sf => sf.SubFieldName == "Sân 1" && sf.Field.FieldName == "Badminton Court X");
                var fieldService1 = context.FieldServices.FirstOrDefault(fs => fs.ServiceName == "Thuê vợt");
                var fieldService2 = context.FieldServices.FirstOrDefault(fs => fs.ServiceName == "Thuê giày");

                if (user1 == null || user2 == null || subField1 == null || subField2 == null || fieldService1 == null || fieldService2 == null)
                {
                    throw new Exception("Required entities (Users, SubFields, or FieldServices) are missing for BookingSeeder.");
                }

                // Tạo danh sách Booking mà không có BookingServices trước
                var bookings = new List<Booking>
                {
                    new Booking
                    {
                        UserId = user1.UserId,
                        SubFieldId = subField1.SubFieldId,
                        BookingDate = DateTime.Parse("2025-03-01"),
                        StartTime = TimeSpan.Parse("07:00"),
                        EndTime = TimeSpan.Parse("08:00"),
                        TotalPrice = 50000 + 20000, // PricePerHour + Thuê vợt
                        Status = "Confirmed",
                        PaymentStatus = "Paid",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Booking
                    {
                        UserId = user2.UserId,
                        SubFieldId = subField2.SubFieldId,
                        BookingDate = DateTime.Parse("2025-03-02"),
                        StartTime = TimeSpan.Parse("09:00"),
                        EndTime = TimeSpan.Parse("10:00"),
                        TotalPrice = 50000 + 15000, // PricePerHour + Thuê giày
                        Status = "Pending",
                        PaymentStatus = "Unpaid",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                // Thêm Booking và lưu để tạo BookingId
                context.Bookings.AddRange(bookings);
                await context.SaveChangesAsync();

                // Thêm BookingServices sau khi Booking đã có BookingId
                var bookingServices = new List<BookingService>
                {
                    new BookingService
                    {
                        BookingId = bookings[0].BookingId,
                        FieldServiceId = fieldService1.FieldServiceId,
                        Quantity = 1,
                        Price = fieldService1.Price
                    },
                    new BookingService
                    {
                        BookingId = bookings[1].BookingId,
                        FieldServiceId = fieldService2.FieldServiceId,
                        Quantity = 1,
                        Price = fieldService2.Price
                    }
                };

                context.BookingServices.AddRange(bookingServices);
                await context.SaveChangesAsync();
            }
        }
    }
}