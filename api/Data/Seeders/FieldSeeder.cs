using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using api.Interfaces;

namespace api.Data.Seeders
{
    public static class FieldSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, IGeocodingService geocodingService, ILogger logger = null)
        {
            if (!await context.Fields.IgnoreQueryFilters().AnyAsync())
            {
                logger?.LogInformation("Seeding Fields...");
                var sport = await context.Sports.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.SportName == "Football");
                var owner = await context.Owners.IgnoreQueryFilters().FirstOrDefaultAsync();
                if (sport == null || owner == null)
                {
                    logger?.LogError("No Sport or Owner found for seeding Fields.");
                    return;
                }

                var fields = new[]
                {
                    new Field
                    {
                        SportId = sport.SportId,
                        OwnerId = owner.OwnerId,
                        FieldName = "Sân ABC",
                        Description = "Sân bóng đá chất lượng cao",
                        Address = "123 Đường ABC, Quận 1, TP.HCM",
                        OpenTime = new TimeSpan(6, 0, 0),
                        CloseTime = new TimeSpan(22, 0, 0),
                        Status = "Active",
                        Latitude = 10.7769,
                        Longitude = 106.7009,
                        City = "Ho Chi Minh",
                        District = "Quan 1",
                        AverageRating = 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                try
                {
                    await context.Fields.AddRangeAsync(fields);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Fields seeded successfully. Fields: {Count}", await context.Fields.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed Fields.");
                    throw;
                }
            }
            else
            {
                logger?.LogInformation("Fields already seeded. Skipping...");
            }
        }
    }
}