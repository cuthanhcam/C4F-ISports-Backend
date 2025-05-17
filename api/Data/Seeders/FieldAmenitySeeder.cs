using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class FieldAmenitySeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.FieldAmenities.AnyAsync())
            {
                logger?.LogInformation("Seeding FieldAmenities...");
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldName == "Sân bóng Cầu Giấy");
                if (field == null)
                {
                    logger?.LogError("No Field found for seeding FieldAmenities.");
                    return;
                }

                var amenities = new[]
                {
                    new FieldAmenity
                    {
                        FieldId = field.FieldId,
                        Field = field, // Gán navigation property
                        AmenityName = "Nhà vệ sinh",
                        Description = "Nhà vệ sinh sạch sẽ, tiện nghi.",
                        IconUrl = "https://example.com/toilet-icon.png"
                    },
                    new FieldAmenity
                    {
                        FieldId = field.FieldId,
                        Field = field, // Gán navigation property
                        AmenityName = "Bãi đỗ xe",
                        Description = "Bãi đỗ xe rộng rãi, miễn phí.",
                        IconUrl = "https://example.com/parking-icon.png"
                    }
                };

                await context.FieldAmenities.AddRangeAsync(amenities);
                await context.SaveChangesAsync();
                logger?.LogInformation("FieldAmenities seeded successfully. FieldAmenities: {Count}", await context.FieldAmenities.CountAsync());
            }
        }
    }
}