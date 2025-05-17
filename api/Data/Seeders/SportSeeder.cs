using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class SportSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.Sports.AnyAsync())
            {
                logger?.LogInformation("Seeding Sports...");
                var sports = new[]
                {
                    new Sport
                    {
                        SportName = "Bóng đá",
                        Description = "Môn thể thao vua, phổ biến toàn cầu.",
                        IconUrl = "https://example.com/football-icon.png",
                        IsActive = true
                    },
                    new Sport
                    {
                        SportName = "Cầu lông",
                        Description = "Môn thể thao sử dụng vợt, phù hợp mọi lứa tuổi.",
                        IconUrl = "https://example.com/badminton-icon.png",
                        IsActive = true
                    }
                };

                await context.Sports.AddRangeAsync(sports);
                await context.SaveChangesAsync();
                logger?.LogInformation("Sports seeded successfully. Sports: {Count}", await context.Sports.CountAsync());
            }
        }
    }
}