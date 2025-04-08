using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class FieldSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!context.Fields.Any())
            {
                var owner1 = context.Owners.First(o => o.Email == "owner1@gmail.com");
                var football = context.Sports.FirstOrDefault(s => s.SportName == "Football") ?? new Sport { SportName = "Football" };
                var badminton = context.Sports.FirstOrDefault(s => s.SportName == "Badminton") ?? new Sport { SportName = "Badminton" };

                if (!context.Sports.Any(s => s.SportName == "Football"))
                {
                    context.Sports.Add(football);
                }
                if (!context.Sports.Any(s => s.SportName == "Badminton"))
                {
                    context.Sports.Add(badminton);
                }
                await context.SaveChangesAsync();

                var fields = new List<Field>
                {
                    new Field
                    {
                        SportId = football.SportId,
                        FieldName = "Football Field A",
                        Phone = "0901234567",
                        Address = "123 Đường XYZ, Quận 1",
                        OpenHours = "06:00-21:00",
                        OwnerId = owner1.OwnerId,
                        Status = "Active",
                        Latitude = 10.123456m,
                        Longitude = 106.123456m,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Field
                    {
                        SportId = football.SportId,
                        FieldName = "Football Field B",
                        Phone = "0901234568",
                        Address = "456 Đường ABC, Quận 2",
                        OpenHours = "06:00-21:00",
                        OwnerId = owner1.OwnerId,
                        Status = "Active",
                        Latitude = 10.124456m,
                        Longitude = 106.124456m,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Field
                    {
                        SportId = badminton.SportId,
                        FieldName = "Badminton Court X",
                        Phone = "0901234569",
                        Address = "789 Đường DEF, Quận 3",
                        OpenHours = "06:00-21:00",
                        OwnerId = owner1.OwnerId,
                        Status = "Active",
                        Latitude = 10.125456m,
                        Longitude = 106.125456m,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Field
                    {
                        SportId = badminton.SportId,
                        FieldName = "Badminton Court Y",
                        Phone = "0901234570",
                        Address = "101 Đường GHI, Quận 4",
                        OpenHours = "06:00-21:00",
                        OwnerId = owner1.OwnerId,
                        Status = "Active",
                        Latitude = 10.126456m,
                        Longitude = 106.126456m,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                context.Fields.AddRange(fields);
                await context.SaveChangesAsync();

                // Thêm các thực thể liên quan
                foreach (var field in fields)
                {
                    context.FieldImages.AddRange(
                        new FieldImage { FieldId = field.FieldId, ImageUrl = "url1" },
                        new FieldImage { FieldId = field.FieldId, ImageUrl = "url2" }
                    );

                    context.FieldAmenities.AddRange(
                        new FieldAmenity { FieldId = field.FieldId, AmenityName = "Wifi", Description = "Miễn phí tốc độ cao" },
                        new FieldAmenity { FieldId = field.FieldId, AmenityName = "Quầy bar", Description = "Đồ uống đa dạng" }
                    );

                    context.SubFields.AddRange(
                        new SubField { FieldId = field.FieldId, SubFieldName = "Sân 1", Size = field.Sport.SportName, PricePerHour = 50000, Status = "Active" },
                        new SubField { FieldId = field.FieldId, SubFieldName = "Sân 2", Size = field.Sport.SportName, PricePerHour = 50000, Status = "Active" }
                    );
                }

                await context.SaveChangesAsync();
            }
        }
    }
}