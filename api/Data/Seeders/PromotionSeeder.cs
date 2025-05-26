using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class PromotionSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.Promotions.IgnoreQueryFilters().AnyAsync())
            {
                logger?.LogInformation("Seeding Promotions...");
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldName == "Sân ABC");
                if (field == null)
                {
                    logger?.LogError("No Field found for seeding Promotions.");
                    return;
                }

                var promotions = new[]
                {
                    new Promotion
                    {
                        Code = "SUMMER2025",
                        Description = "Giảm giá 10% cho đặt sân tháng 6",
                        DiscountType = "Percentage",
                        DiscountValue = 10,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddMonths(1),
                        MinBookingValue = 200000,
                        MaxDiscountAmount = 100000,
                        IsActive = true,
                        UsageLimit = 100,
                        UsageCount = 0,
                        FieldId = field.FieldId,
                        Field = field
                    }
                };

                try
                {
                    await context.Promotions.AddRangeAsync(promotions);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Promotions seeded successfully. Promotions: {Count}", await context.Promotions.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed Promotions. StackTrace: {StackTrace}", ex.StackTrace);
                    throw;
                }
            }
            else
            {
                logger?.LogInformation("Promotions already seeded. Skipping...");
            }
        }
    }
}