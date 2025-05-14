using api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class FieldImageSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.FieldImages.AnyAsync())
            {
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldName == "Sân bóng Cầu Giấy");
                if (field != null)
                {
                    var images = new[]
                    {
                        new FieldImage
                        {
                            FieldId = field.FieldId,
                            ImageUrl = "https://example.com/field-image1.jpg",
                            Thumbnail = "https://example.com/field-thumbnail1.jpg",
                            IsPrimary = true,
                            UploadedAt = DateTime.UtcNow
                        },
                        new FieldImage
                        {
                            FieldId = field.FieldId,
                            ImageUrl = "https://example.com/field-image2.jpg",
                            Thumbnail = "https://example.com/field-thumbnail2.jpg",
                            IsPrimary = false,
                            UploadedAt = DateTime.UtcNow
                        }
                    };

                    await context.FieldImages.AddRangeAsync(images);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}