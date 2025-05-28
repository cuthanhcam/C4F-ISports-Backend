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
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
        {
            try
            {
                if (await context.PricingRules.AnyAsync())
                {
                    logger.LogInformation("PricingRules đã tồn tại, bỏ qua seeding.");
                    return;
                }

                logger.LogInformation("Bắt đầu seeding PricingRules...");

                // Get all subfields
                var subFields = await context.SubFields.ToListAsync();
                if (!subFields.Any())
                {
                    logger.LogWarning("Không tìm thấy SubFields để tạo PricingRules.");
                    return;
                }

                // Get all timeslots
                var timeSlots = await context.TimeSlots.ToListAsync();
                if (!timeSlots.Any())
                {
                    logger.LogWarning("Không tìm thấy TimeSlots để tạo PricingRules.");
                    return;
                }

                var pricingRules = new List<PricingRule>();

                // Create pricing rules for each subfield
                foreach (var subField in subFields)
                {
                    // Weekday rule (Monday to Friday)
                    var weekdayRule = new PricingRule
                    {
                        SubFieldId = subField.SubFieldId,
                        AppliesToDays = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" },
                        TimeSlots = timeSlots.Where(ts => ts.StartTime.Hours >= 17 && ts.StartTime.Hours < 23).ToList()
                    };
                    pricingRules.Add(weekdayRule);

                    // Weekend rule (Saturday and Sunday)
                    var weekendRule = new PricingRule
                    {
                        SubFieldId = subField.SubFieldId,
                        AppliesToDays = new List<string> { "Saturday", "Sunday" },
                        TimeSlots = timeSlots.ToList() // All time slots
                    };
                    pricingRules.Add(weekendRule);

                    // Special morning rule
                    var morningRule = new PricingRule
                    {
                        SubFieldId = subField.SubFieldId,
                        AppliesToDays = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" },
                        TimeSlots = timeSlots.Where(ts => ts.StartTime.Hours >= 5 && ts.StartTime.Hours < 12).ToList()
                    };
                    pricingRules.Add(morningRule);
                }

                await context.PricingRules.AddRangeAsync(pricingRules);
                await context.SaveChangesAsync();

                logger.LogInformation("Đã seed {Count} PricingRules thành công.", pricingRules.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi seed PricingRules: {Message}", ex.Message);
                throw;
            }
        }
    }
}