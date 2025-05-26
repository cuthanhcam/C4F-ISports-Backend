using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldName == "Sân ABC");
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
                        Field = field,
                        ImageUrl = "https://example.com/images/field-abc.jpg",
                        PublicId = "field-abc-1",
                        Thumbnail = "https://example.com/images/field-abc-thumb.jpg",
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
                    logger?.LogError(ex, "Failed to seed FieldImages. StackTrace: {StackTrace}", ex.StackTrace);
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