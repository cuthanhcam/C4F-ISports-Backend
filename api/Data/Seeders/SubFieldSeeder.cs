using api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class SubFieldSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.SubFields.AnyAsync())
            {
                var field = await context.Fields.FirstOrDefaultAsync();
                if (field != null)
                {
                    var subFields = new[]
                    {
                        new SubField
                        {
                            FieldId = field.FieldId,
                            SubFieldName = "Sân 5A",
                            FieldType = "5-a-side",
                            Status = "Active",
                            Capacity = 10,
                            Description = "Sân bóng 5 người, cỏ nhân tạo."
                        },
                        new SubField
                        {
                            FieldId = field.FieldId,
                            SubFieldName = "Sân 7A",
                            FieldType = "7-a-side",
                            Status = "Active",
                            Capacity = 14,
                            Description = "Sân bóng 7 người, cỏ nhân tạo."
                        }
                    };

                    await context.SubFields.AddRangeAsync(subFields);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}