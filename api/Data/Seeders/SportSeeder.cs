using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace api.Data.Seeders
{
    public static class SportSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
        {
            try
            {
                if (!await context.Sports.IgnoreQueryFilters().AnyAsync())
                {
                    logger.LogInformation("Seeding Sports...");

                    var sports = new List<Sport>
                    {
                        new Sport
                        {
                            SportName = "Football",
                            Description = "Môn thể thao đồng đội phổ biến nhất thế giới.",
                            ImageUrl = "https://example.com/icons/football.png",
                            IsActive = true
                        },
                        new Sport
                        {
                            SportName = "Badminton",
                            Description = "Môn thể thao đối kháng sử dụng vợt và cầu lông.",
                            ImageUrl = "https://example.com/icons/badminton.png",
                            IsActive = true
                        }
                    };

                    context.Sports.AddRange(sports);
                    await context.SaveChangesAsync();

                    logger.LogInformation("Seeded {Count} sports.", sports.Count);
                }
                else
                {
                    logger.LogInformation("Sports already seeded. Skipping...");
                }

                logger.LogInformation("SportSeeder completed. Sports: {Count}", await context.Sports.CountAsync());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to seed Sports. StackTrace: {StackTrace}", ex.StackTrace);
                throw;
            }
        }
    }
}