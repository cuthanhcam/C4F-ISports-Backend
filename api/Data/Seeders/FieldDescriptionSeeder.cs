using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class FieldDescriptionSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.FieldDescriptions.AnyAsync())
            {
                logger?.LogInformation("Seeding FieldDescriptions...");
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldName == "Sân bóng Cầu Giấy");
                if (field == null)
                {
                    logger?.LogError("No Field found for seeding FieldDescriptions.");
                    return;
                }

                var descriptions = new[]
                {
                    new FieldDescription
                    {
                        FieldId = field.FieldId,
                        Field = field, // Gán navigation property
                        Description = "Sân bóng Cầu Giấy là sân cỏ nhân tạo chất lượng cao, được trang bị hệ thống đèn chiếu sáng hiện đại và mặt cỏ đạt chuẩn FIFA."
                    }
                };

                await context.FieldDescriptions.AddRangeAsync(descriptions);
                await context.SaveChangesAsync();
                logger?.LogInformation("FieldDescriptions seeded successfully. FieldDescriptions: {Count}", await context.FieldDescriptions.CountAsync());
            }
        }
    }
}