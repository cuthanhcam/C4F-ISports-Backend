using api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class FieldAmenitySeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.FieldAmenities.AnyAsync())
            {
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldName == "Sân bóng Cầu Giấy");
                if (field != null)
                {
                    var amenities = new[]
                    {
                        new FieldAmenity
                        {
                            FieldId = field.FieldId,
                            AmenityName = "Nhà vệ sinh",
                            Description = "Nhà vệ sinh sạch sẽ, tiện nghi.",
                            IconUrl = "https://example.com/toilet-icon.png"
                        },
                        new FieldAmenity
                        {
                            FieldId = field.FieldId,
                            AmenityName = "Bãi đỗ xe",
                            Description = "Bãi đỗ xe rộng rãi, miễn phí.",
                            IconUrl = "https://example.com/parking-icon.png"
                        }
                    };

                    await context.FieldAmenities.AddRangeAsync(amenities);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}