using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class FieldImageSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.FieldImages.AnyAsync())
            {
                logger?.LogInformation("Seeding FieldImages...");
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldName == "Sân bóng Cầu Giấy");
                if (field == null)
                {
                    logger?.LogError("No Field found for seeding FieldImages.");
                    return;
                }

                var images = new[]
                {
                    new FieldImage
                    {
                        FieldId = field.FieldId,
                        Field = field, // Gán navigation property
                        ImageUrl = "https://example.com/field-image1.jpg",
                        Thumbnail = "https://example.com/field-thumbnail1.jpg",
                        IsPrimary = true,
                        UploadedAt = DateTime.UtcNow
                    },
                    new FieldImage
                    {
                        FieldId = field.FieldId,
                        Field = field, // Gán navigation property
                        ImageUrl = "https://example.com/field-image2.jpg",
                        Thumbnail = "https://example.com/field-thumbnail2.jpg",
                        IsPrimary = false,
                        UploadedAt = DateTime.UtcNow
                    }
                };

                await context.FieldImages.AddRangeAsync(images);
                await context.SaveChangesAsync();
                logger?.LogInformation("FieldImages seeded successfully. FieldImages: {Count}", await context.FieldImages.CountAsync());
            }
        }
    }
}