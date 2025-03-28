using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class FieldImageSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!context.FieldImages.Any())
            {
                // Đảm bảo Field với FieldId = 1 tồn tại
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldId == 1);
                if (field == null)
                {
                    // Nếu không có, bạn có thể log lỗi hoặc bỏ qua
                    return;
                }

                var fieldImages = new List<FieldImage>
                {
                    new FieldImage { FieldId = field.FieldId, ImageUrl = "https://example.com/image1.jpg" },
                    new FieldImage { FieldId = field.FieldId, ImageUrl = "https://example.com/image2.jpg" }
                };

                await context.FieldImages.AddRangeAsync(fieldImages);
                await context.SaveChangesAsync();
            }
        }
    }
}