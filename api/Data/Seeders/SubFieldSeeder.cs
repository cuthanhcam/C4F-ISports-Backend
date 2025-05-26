using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class SubFieldSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.SubFields.IgnoreQueryFilters().AnyAsync())
            {
                logger?.LogInformation("Seeding SubFields...");
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldName == "Sân ABC");
                if (field == null)
                {
                    logger?.LogError("No Field found for seeding SubFields.");
                    return;
                }

                var subFields = new[]
                {
                    new SubField
                    {
                        FieldId = field.FieldId,
                        Field = field,
                        SubFieldName = "Sân 5A",
                        FieldType = "5-a-side",
                        Status = "Active",
                        Capacity = 10,
                        Description = "Sân bóng đá 5 người với cỏ nhân tạo"
                    },
                    new SubField
                    {
                        FieldId = field.FieldId,
                        Field = field,
                        SubFieldName = "Sân 7A",
                        FieldType = "7-a-side",
                        Status = "Active",
                        Capacity = 14,
                        Description = "Sân bóng đá 7 người với cỏ nhân tạo"
                    }
                };

                try
                {
                    await context.SubFields.AddRangeAsync(subFields);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("SubFields seeded successfully. SubFields: {Count}", await context.SubFields.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed SubFields. StackTrace: {StackTrace}", ex.StackTrace);
                    throw;
                }
            }
            else
            {
                logger?.LogInformation("SubFields already seeded. Skipping...");
            }
        }
    }
}