using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class FieldPricingSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.FieldPricings.IgnoreQueryFilters().AnyAsync())
            {
                logger?.LogInformation("Seeding FieldPricings...");
                var subField = await context.SubFields.IgnoreQueryFilters().FirstOrDefaultAsync(sf => sf.SubFieldName == "Sân 5A");
                if (subField == null)
                {
                    logger?.LogError("No SubField found for seeding FieldPricings.");
                    return;
                }

                var fieldPricings = new[]
                {
                    new FieldPricing
                    {
                        SubFieldId = subField.SubFieldId,
                        StartTime = new TimeSpan(6, 0, 0),
                        EndTime = new TimeSpan(12, 0, 0),
                        DayOfWeek = 0, // Sunday
                        Price = 300000m,
                        IsActive = true
                    },
                    new FieldPricing
                    {
                        SubFieldId = subField.SubFieldId,
                        StartTime = new TimeSpan(12, 0, 0),
                        EndTime = new TimeSpan(18, 0, 0),
                        DayOfWeek = 0, // Sunday
                        Price = 400000m,
                        IsActive = true
                    }
                };

                try
                {
                    await context.FieldPricings.AddRangeAsync(fieldPricings);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("FieldPricings seeded successfully. FieldPricings: {Count}", await context.FieldPricings.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed FieldPricings.");
                    throw;
                }
            }
            else
            {
                logger?.LogInformation("FieldPricings already seeded. Skipping...");
            }
        }
    }
}