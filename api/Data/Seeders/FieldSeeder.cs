using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class FieldSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (!context.Fields.Any())
            {
                var owner1 = context.Owners.First(o => o.Email == "owner1@gmail.com");
                var owner2 = context.Owners.First(o => o.Email == "owner2@gmail.com");
                var football = context.Sports.First(s => s.SportName == "Football");
                var badminton = context.Sports.First(s => s.SportName == "Badminton");

                context.Fields.AddRange(
                    new Field
                    {
                        SportId = football.SportId,
                        FieldName = "Football Field A",
                        Phone = "0123456789",
                        Address = "123 Main St, Thủ Đức, TP.HCM",
                        OpenHours = "08:00-22:00",
                        OwnerId = owner1.OwnerId,
                        Status = "Active",
                        Latitude = 10.776900m,
                        Longitude = 106.700900m,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Field
                    {
                        SportId = football.SportId,
                        FieldName = "Football Field B",
                        Phone = "0123456790",
                        Address = "456 Tran Hung Dao, Quận 1, TP.HCM",
                        OpenHours = "07:00-23:00",
                        OwnerId = owner2.OwnerId,
                        Status = "Active",
                        Latitude = 10.771000m,
                        Longitude = 106.698000m,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Field
                    {
                        SportId = badminton.SportId,
                        FieldName = "Badminton Court X",
                        Phone = "0365716824",
                        Address = "32/1 Tran Hung Dao, Đông Hoà, Dĩ An, Bình Dương",
                        OpenHours = "05:00-22:00",
                        OwnerId = owner1.OwnerId,
                        Status = "Active",
                        Latitude = 10.894621m,
                        Longitude = 106.779709m,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Field
                    {
                        SportId = badminton.SportId,
                        FieldName = "Badminton Court Y",
                        Phone = "0365716835",
                        Address = "789 Nguyen Trai, Thủ Đức, TP.HCM",
                        OpenHours = "06:00-21:00",
                        OwnerId = owner2.OwnerId,
                        Status = "Maintenance",
                        Latitude = 10.780000m,
                        Longitude = 106.710000m,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                );
                context.SaveChanges();
            }
        }
    }
}