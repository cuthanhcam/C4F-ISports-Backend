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
            if (!await context.FieldPricings.AnyAsync())
            {
                logger?.LogInformation("Seeding FieldPricings...");
                var subField = await context.SubFields.FirstOrDefaultAsync(sf => sf.SubFieldName == "Sân 5A");
                if (subField == null)
                {
                    logger?.LogError("No SubField found for seeding FieldPricings.");
                    return;
                }

                var pricings = new[]
                {
                    new FieldPricing
                    {
                        SubFieldId = subField.SubFieldId,
                        SubField = subField, // Gán navigation property
                        StartTime = TimeSpan.Parse("17:00"),
                        EndTime = TimeSpan.Parse("19:00"),
                        DayOfWeek = 1, // Thứ Hai
                        Price = 500000m,
                        IsActive = true
                    },
                    new FieldPricing
                    {
                        SubFieldId = subField.SubFieldId,
                        SubField = subField, // Gán navigation property
                        StartTime = TimeSpan.Parse("19:00"),
                        EndTime = TimeSpan.Parse("21:00"),
                        DayOfWeek = 1, // Thứ Hai
                        Price = 600000m,
                        IsActive = true
                    }
                };

                await context.FieldPricings.AddRangeAsync(pricings);
                await context.SaveChangesAsync();
                logger?.LogInformation("FieldPricings seeded successfully. FieldPricings: {Count}", await context.FieldPricings.CountAsync());
            }
        }
    }
}