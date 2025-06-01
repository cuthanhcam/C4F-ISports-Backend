using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using api.Interfaces;

namespace api.Data.Seeders
{
    public static class FieldImageSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ICloudinaryService cloudinaryService, ILogger logger = null)
        {
            if (!await context.FieldImages.IgnoreQueryFilters().AnyAsync())
            {
                logger?.LogInformation("Seeding FieldImages...");
                var field = await context.Fields.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.FieldName == "Sân ABC");
                if (field == null)
                {
                    logger?.LogError("No Field found for seeding FieldImages.");
                    return;
                }

                var fieldImages = new[]
                {
            new FieldImage
            {
                FieldId = field.FieldId,
                ImageUrl = "https://res.cloudinary.com/demo/image/upload/v1234567890/field-abc.jpg",
                PublicId = "field-abc-1",
                Thumbnail = "https://res.cloudinary.com/demo/image/upload/v1234567890/field-abc-thumb.jpg",
                IsPrimary = true,
                UploadedAt = DateTime.UtcNow
            }
        };

                try
                {
                    await context.FieldImages.AddRangeAsync(fieldImages);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("FieldImages seeded successfully. FieldImages: {Count}", await context.FieldImages.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed FieldImages.");
                    throw;
                }
            }
            else
            {
                logger?.LogInformation("FieldImages already seeded. Skipping...");
            }
        }
    }
}