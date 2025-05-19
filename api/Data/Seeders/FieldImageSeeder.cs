using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using api.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace api.Data.Seeders
{
    public static class FieldImageSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ICloudinaryService cloudinaryService = null, ILogger logger = null)
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
                        Field = field,
                        ImageUrl = await UploadSampleImage(cloudinaryService, "field-image1.jpg", logger),
                        Thumbnail = await UploadSampleImage(cloudinaryService, "field-thumbnail1.jpg", logger),
                        IsPrimary = true,
                        UploadedAt = DateTime.UtcNow
                    },
                    new FieldImage
                    {
                        FieldId = field.FieldId,
                        Field = field,
                        ImageUrl = await UploadSampleImage(cloudinaryService, "field-image2.jpg", logger),
                        Thumbnail = await UploadSampleImage(cloudinaryService, "field-thumbnail2.jpg", logger),
                        IsPrimary = false,
                        UploadedAt = DateTime.UtcNow
                    }
                };

                try
                {
                    await context.FieldImages.AddRangeAsync(images);
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

        private static async Task<string> UploadSampleImage(ICloudinaryService cloudinaryService, string fileName, ILogger logger)
        {
            if (cloudinaryService == null)
            {
                logger?.LogWarning("CloudinaryService is null. Using fallback URL for {FileName}.", fileName);
                return $"https://example.com/{fileName}";
            }

            try
            {
                // Dữ liệu JPEG tối thiểu (hình ảnh xám 1x1 pixel)
                byte[] minimalJpeg = new byte[]
                {
                    0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x00, 0x00, 0x01,
                    0x00, 0x01, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00,
                    0x01, 0x00, 0x01, 0x01, 0x01, 0x11, 0x00, 0xFF, 0xC4, 0x00, 0x14, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0xFF, 0xDA, 0x00,
                    0x08, 0x01, 0x01, 0x00, 0x00, 0x3F, 0x00, 0x63, 0xFF, 0xD9
                };
                if (minimalJpeg.Length == 0)
                {
                    logger?.LogWarning("Image data is empty for {FileName}. Using fallback URL.", fileName);
                    return $"https://example.com/{fileName}";
                }

                using var stream = new MemoryStream(minimalJpeg);
                var file = new FormFile(stream, 0, stream.Length, "file", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/jpeg"
                };
                var url = await cloudinaryService.UploadImageAsync(file);
                logger?.LogInformation("Uploaded image {FileName} to Cloudinary: {Url}", fileName, url);
                return url;
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Failed to upload image {FileName} to Cloudinary. Using fallback URL. StackTrace: {StackTrace}", fileName, ex.StackTrace);
                return $"https://example.com/{fileName}";
            }
        }
    }
}