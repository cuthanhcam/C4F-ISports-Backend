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
            if (!await context.FieldDescriptions.IgnoreQueryFilters().AnyAsync())
            {
                logger?.LogInformation("Seeding FieldDescriptions...");
                var field = await context.Fields.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.FieldName == "Sân ABC");
                if (field == null)
                {
                    logger?.LogError("No Field found for seeding FieldDescriptions.");
                    return;
                }

                var fieldDescriptions = new[]
                {
            new FieldDescription
            {
                FieldId = field.FieldId,
                Description = "Sân ABC là một trong những sân bóng đá chất lượng cao tại TP.HCM, với cỏ nhân tạo hiện đại và hệ thống chiếu sáng tốt."
            }
        };

                try
                {
                    await context.FieldDescriptions.AddRangeAsync(fieldDescriptions);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("FieldDescriptions seeded successfully. FieldDescriptions: {Count}", await context.FieldDescriptions.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed FieldDescriptions.");
                    throw;
                }
            }
            else
            {
                logger?.LogInformation("FieldDescriptions already seeded. Skipping...");
            }
        }
    }
}