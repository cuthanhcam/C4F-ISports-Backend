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
            if (!await context.FieldServices.IgnoreQueryFilters().AnyAsync())
            {
                logger?.LogInformation("Seeding FieldServices...");
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldName == "Sân ABC");
                if (field == null)
                {
                    logger?.LogError("No Field found for seeding FieldServices.");
                    return;
                }

                var fieldServices = new[]
                {
                    new FieldService
                    {
                        FieldId = field.FieldId,
                        Field = field,
                        ServiceName = "Nước uống",
                        Price = 10000,
                        Description = "Nước suối đóng chai 500ml",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                try
                {
                    await context.FieldServices.AddRangeAsync(fieldServices);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("FieldServices seeded successfully. FieldServices: {Count}", await context.FieldServices.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed FieldServices. StackTrace: {StackTrace}", ex.StackTrace);
                    throw;
                }
            }
            else
            {
                logger?.LogInformation("FieldServices already seeded. Skipping...");
            }
        }
    }
}