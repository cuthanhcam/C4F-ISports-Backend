using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
                        SportId = 1,
                        SportName = "Football",
                        Description = "Bóng đá sân 5 và sân 7",
                        IconUrl = "https://example.com/icons/football.png",
                        IsActive = true
                    },
                    new Sport
                    {
                        SportId = 2,
                        SportName = "Badminton",
                        Description = "Cầu lông trong nhà",
                        IconUrl = "https://example.com/icons/badminton.png",
                        IsActive = true
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
                    logger?.LogError(ex, "Failed to seed Sports. StackTrace: {StackTrace}", ex.StackTrace);
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