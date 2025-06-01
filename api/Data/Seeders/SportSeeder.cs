using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class SportSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.Sports.IgnoreQueryFilters().AnyAsync())
            {
                logger?.LogInformation("Seeding Sports...");

                var sports = new List<Sport>
                {
                    new Sport
                    {
                        SportName = "Football",
                        Description = "Môn thể thao đồng đội phổ biến nhất thế giới.",
                        ImageUrl = "https://res.cloudinary.com/demo/image/upload/v1234567890/football.png",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Sport
                    {
                        SportName = "Badminton",
                        Description = "Môn thể thao đối kháng sử dụng vợt và cầu lông.",
                        ImageUrl = "https://res.cloudinary.com/demo/image/upload/v1234567890/badminton.png",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                try
                {
                    await context.Sports.AddRangeAsync(sports);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Sports seeded successfully. Sports: {Count}", await context.Sports.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed Sports.");
                    throw;
                }
            }
            else
            {
                logger?.LogInformation("Sports already seeded. Skipping...");
            }
        }
    }
}