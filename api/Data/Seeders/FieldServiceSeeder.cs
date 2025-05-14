using api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class FieldServiceSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.FieldServices.AnyAsync())
            {
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldName == "Sân bóng Cầu Giấy");
                if (field != null)
                {
                    var services = new[]
                    {
                        new FieldService
                        {
                            FieldId = field.FieldId,
                            ServiceName = "Nước uống",
                            Price = 10000m,
                            Description = "Nước suối đóng chai 500ml.",
                            IsActive = true
                        },
                        new FieldService
                        {
                            FieldId = field.FieldId,
                            ServiceName = "Thuê giày",
                            Price = 30000m,
                            Description = "Giày bóng đá đa dạng kích cỡ.",
                            IsActive = true
                        }
                    };

                    await context.FieldServices.AddRangeAsync(services);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}