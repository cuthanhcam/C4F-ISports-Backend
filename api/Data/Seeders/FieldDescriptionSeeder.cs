using api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class FieldDescriptionSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.FieldDescriptions.AnyAsync())
            {
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldName == "Sân bóng Cầu Giấy");
                if (field != null)
                {
                    var descriptions = new[]
                    {
                        new FieldDescription
                        {
                            FieldId = field.FieldId,
                            Description = "Sân bóng Cầu Giấy là sân cỏ nhân tạo chất lượng cao, được trang bị hệ thống đèn chiếu sáng hiện đại và mặt cỏ đạt chuẩn FIFA."
                        }
                    };

                    await context.FieldDescriptions.AddRangeAsync(descriptions);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}