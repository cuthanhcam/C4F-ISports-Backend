using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using api.Interfaces;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class FieldSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, IGeocodingService geocodingService = null, ILogger logger = null)
        {
            if (!await context.Fields.AnyAsync())
            {
                logger?.LogInformation("Seeding Fields...");
                var football = await context.Sports.FirstOrDefaultAsync(s => s.SportName == "Bóng đá");
                var owner = await context.Owners.FirstOrDefaultAsync();
                if (football == null || owner == null)
                {
                    logger?.LogError("No Sport or Owner found for seeding Fields.");
                    return;
                }

                var fields = new[]
                {
                    new Field
                    {
                        SportId = football.SportId,
                        Sport = football,
                        OwnerId = owner.OwnerId,
                        Owner = owner,
                        FieldName = "Sân bóng Cầu Giấy",
                        Phone = "0909876543",
                        Address = "123 Đường Láng, Cầu Giấy, Hà Nội",
                        OpenHours = "06:00-23:00",
                        OpenTime = TimeSpan.Parse("06:00"),
                        CloseTime = TimeSpan.Parse("23:00"),
                        Status = "Active",
                        City = "Hà Nội",
                        District = "Cầu Giấy",
                        AverageRating = 4.5m,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Latitude = 0,
                        Longitude = 0
                    }
                };

                if (geocodingService != null)
                {
                    foreach (var field in fields)
                    {
                        try
                        {
                            var (latitude, longitude) = await geocodingService.GetCoordinatesFromAddressAsync(field.FieldName, field.Address);
                            field.Latitude = latitude;
                            field.Longitude = longitude;
                            logger?.LogInformation("Coordinates for {FieldName}: Lat={Lat}, Lng={Lng}", field.FieldName, latitude, longitude);
                        }
                        catch (Exception ex)
                        {
                            logger?.LogWarning("Failed to get coordinates for {FieldName}: {Error}. Using default coordinates.", field.FieldName, ex.Message);
                            field.Latitude = 21.030123m; // Fallback
                            field.Longitude = 105.801456m;
                        }
                    }
                }

                try
                {
                    await context.Fields.AddRangeAsync(fields);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Fields seeded successfully. Fields: {Count}", await context.Fields.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed Fields. StackTrace: {StackTrace}", ex.StackTrace);
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