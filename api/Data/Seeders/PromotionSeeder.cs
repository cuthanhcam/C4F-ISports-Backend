using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class PromotionSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.Promotions.AnyAsync())
            {
                logger?.LogInformation("Seeding Promotions...");
                var promotions = new[]
                {
                    new Promotion
                    {
                        Code = "WELCOME10",
                        Description = "Giảm 10% cho lần đặt sân đầu tiên.",
                        DiscountType = "Percentage",
                        DiscountValue = 10m,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddMonths(1),
                        MinBookingValue = 300000m,
                        MaxDiscountAmount = 100000m,
                        IsActive = true,
                        UsageLimit = 100,
                        UsageCount = 0
                    }
                };

                await context.Promotions.AddRangeAsync(promotions);
                await context.SaveChangesAsync();
                logger?.LogInformation("Promotions seeded successfully. Promotions: {Count}", await context.Promotions.CountAsync());
            }
        }
    }
}