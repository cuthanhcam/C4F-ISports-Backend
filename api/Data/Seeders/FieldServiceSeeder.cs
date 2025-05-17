using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class FieldServiceSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.FieldServices.AnyAsync())
            {
                logger?.LogInformation("Seeding FieldServices...");
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldName == "Sân bóng Cầu Giấy");
                if (field == null)
                {
                    logger?.LogError("No Field found for seeding FieldServices.");
                    return;
                }

                var services = new[]
                {
                    new FieldService
                    {
                        FieldId = field.FieldId,
                        Field = field, // Gán navigation property
                        ServiceName = "Nước uống",
                        Price = 10000m,
                        Description = "Nước suối đóng chai 500ml.",
                        IsActive = true
                    },
                    new FieldService
                    {
                        FieldId = field.FieldId,
                        Field = field, // Gán navigation property
                        ServiceName = "Thuê giày",
                        Price = 30000m,
                        Description = "Giày bóng đá đa dạng kích cỡ.",
                        IsActive = true
                    }
                };

                await context.FieldServices.AddRangeAsync(services);
                await context.SaveChangesAsync();
                logger?.LogInformation("FieldServices seeded successfully. FieldServices: {Count}", await context.FieldServices.CountAsync());
            }
        }
    }
}