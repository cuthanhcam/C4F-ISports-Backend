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
            if (!await context.FieldAmenities.IgnoreQueryFilters().AnyAsync())
            {
                logger?.LogInformation("Seeding FieldAmenities...");
                var field = await context.Fields.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.FieldName == "Sân ABC");
                if (field == null)
                {
                    logger?.LogError("No Field found for seeding FieldAmenities.");
                    return;
                }

                var fieldAmenities = new[]
                {
            new FieldAmenity
            {
                FieldId = field.FieldId,
                AmenityName = "Phòng thay đồ",
                Description = "Phòng thay đồ sạch sẽ với tủ khóa",
                IconUrl = "https://res.cloudinary.com/demo/image/upload/v1234567890/dressing-room.png",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new FieldAmenity
            {
                FieldId = field.FieldId,
                AmenityName = "Bãi đỗ xe",
                Description = "Bãi đỗ xe rộng rãi",
                IconUrl = "https://res.cloudinary.com/demo/image/upload/v1234567890/parking.png",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

                try
                {
                    await context.FieldAmenities.AddRangeAsync(fieldAmenities);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("FieldAmenities seeded successfully. FieldAmenities: {Count}", await context.FieldAmenities.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed FieldAmenities.");
                    throw;
                }
            }
            else
            {
                logger?.LogInformation("FieldAmenities already seeded. Skipping...");
            }
        }
    }
}