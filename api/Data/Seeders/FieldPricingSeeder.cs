using api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class FieldPricingSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.FieldPricings.AnyAsync())
            {
                var subField = await context.SubFields.FirstOrDefaultAsync(sf => sf.SubFieldName == "Sân 5A");
                if (subField != null)
                {
                    var pricings = new[]
                    {
                        new FieldPricing
                        {
                            SubFieldId = subField.SubFieldId,
                            StartTime = TimeSpan.Parse("17:00"),
                            EndTime = TimeSpan.Parse("19:00"),
                            DayOfWeek = 1, // Thứ Hai
                            Price = 500000m,
                            IsActive = true
                        },
                        new FieldPricing
                        {
                            SubFieldId = subField.SubFieldId,
                            StartTime = TimeSpan.Parse("19:00"),
                            EndTime = TimeSpan.Parse("21:00"),
                            DayOfWeek = 1, // Thứ Hai
                            Price = 600000m,
                            IsActive = true
                        }
                    };

                    await context.FieldPricings.AddRangeAsync(pricings);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}