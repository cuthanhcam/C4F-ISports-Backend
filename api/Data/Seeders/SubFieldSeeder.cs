using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
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
                var field = await context.Fields.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.FieldName == "Sân ABC");
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
                        SubFieldName = "Sân 5A",
                        FieldType = "5-a-side",
                        Status = "Active",
                        Capacity = 10,
                        Description = "Sân bóng đá 5 người với cỏ nhân tạo",
                        OpenTime = TimeSpan.Parse("07:00"),
                        CloseTime = TimeSpan.Parse("23:00"),
                        DefaultPricePerSlot = 200000m,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new SubField
                    {
                        FieldId = field.FieldId,
                        SubFieldName = "Sân 7A",
                        FieldType = "7-a-side",
                        Status = "Active",
                        Capacity = 14,
                        Description = "Sân bóng đá 7 người với cỏ nhân tạo",
                        OpenTime = TimeSpan.Parse("07:00"),
                        CloseTime = TimeSpan.Parse("23:00"),
                        DefaultPricePerSlot = 300000m,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
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
                    logger?.LogError(ex, "Failed to seed SubFields.");
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