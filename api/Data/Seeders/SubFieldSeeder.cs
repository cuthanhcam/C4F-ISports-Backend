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
            if (!await context.SubFields.AnyAsync())
            {
                logger?.LogInformation("Seeding SubFields...");
                var field = await context.Fields.FirstOrDefaultAsync();
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
                        Field = field, // Gán navigation property
                        SubFieldName = "Sân 5A",
                        FieldType = "5-a-side",
                        Status = "Active",
                        Capacity = 10,
                        Description = "Sân bóng 5 người, cỏ nhân tạo."
                    },
                    new SubField
                    {
                        FieldId = field.FieldId,
                        Field = field, // Gán navigation property
                        SubFieldName = "Sân 7A",
                        FieldType = "7-a-side",
                        Status = "Active",
                        Capacity = 14,
                        Description = "Sân bóng 7 người, cỏ nhân tạo."
                    }
                };

                await context.SubFields.AddRangeAsync(subFields);
                await context.SaveChangesAsync();
                logger?.LogInformation("SubFields seeded successfully. SubFields: {Count}", await context.SubFields.CountAsync());
            }
        }
    }
}