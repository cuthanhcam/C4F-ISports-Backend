using api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class FieldSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.Fields.AnyAsync())
            {
                var football = await context.Sports.FirstOrDefaultAsync(s => s.SportName == "Bóng đá");
                var owner = await context.Owners.FirstOrDefaultAsync();
                if (football != null && owner != null)
                {
                    var fields = new[]
                    {
                        new Field
                        {
                            SportId = football.SportId,
                            FieldName = "Sân bóng Cầu Giấy",
                            Phone = "0909876543",
                            Address = "123 Đường Láng, Cầu Giấy, Hà Nội",
                            OpenHours = "06:00-23:00",
                            OpenTime = TimeSpan.Parse("06:00"),
                            CloseTime = TimeSpan.Parse("23:00"),
                            OwnerId = owner.OwnerId,
                            Status = "Active",
                            Latitude = 21.030123m,
                            Longitude = 105.801456m,
                            City = "Hà Nội",
                            District = "Cầu Giấy",
                            AverageRating = 4.5m,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    await context.Fields.AddRangeAsync(fields);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}