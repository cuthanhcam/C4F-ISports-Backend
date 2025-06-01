using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class PricingRuleSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.PricingRules.IgnoreQueryFilters().AnyAsync())
            {
                logger?.LogInformation("Seeding PricingRules...");

                var subFields = await context.SubFields.IgnoreQueryFilters().ToListAsync();
                if (!subFields.Any())
                {
                    logger?.LogError("No SubFields found for seeding PricingRules.");
                    return;
                }

                var pricingRules = new List<PricingRule>();

                foreach (var subField in subFields)
                {
                    pricingRules.Add(new PricingRule
                    {
                        SubFieldId = subField.SubFieldId,
                        AppliesToDays = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" },
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    pricingRules.Add(new PricingRule
                    {
                        SubFieldId = subField.SubFieldId,
                        AppliesToDays = new List<string> { "Saturday", "Sunday" },
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    pricingRules.Add(new PricingRule
                    {
                        SubFieldId = subField.SubFieldId,
                        AppliesToDays = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" },
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                try
                {
                    await context.PricingRules.AddRangeAsync(pricingRules);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("PricingRules seeded successfully. PricingRules: {Count}", await context.PricingRules.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed PricingRules.");
                    throw;
                }
            }
            else
            {
                logger?.LogInformation("PricingRules already seeded. Skipping...");
            }
        }
    }
}