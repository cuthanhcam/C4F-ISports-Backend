using api.Data.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace api.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("SeedData");

            logger.LogInformation("Starting database seeding...");

            await AccountSeeder.SeedAsync(context);
            logger.LogInformation("AccountSeeder completed.");

            await UserSeeder.SeedAsync(context);
            logger.LogInformation("UserSeeder completed.");

            await OwnerSeeder.SeedAsync(context);
            logger.LogInformation("OwnerSeeder completed.");

            await SportSeeder.SeedAsync(context);
            logger.LogInformation("SportSeeder completed.");

            await FieldSeeder.SeedAsync(context);
            logger.LogInformation("FieldSeeder completed.");

            await FieldDescriptionSeeder.SeedAsync(context);
            logger.LogInformation("FieldDescriptionSeeder completed.");

            await FieldImageSeeder.SeedAsync(context);
            logger.LogInformation("FieldImageSeeder completed.");

            await FieldAmenitySeeder.SeedAsync(context);
            logger.LogInformation("FieldAmenitySeeder completed.");

            await FieldServiceSeeder.SeedAsync(context);
            logger.LogInformation("FieldServiceSeeder completed.");

            await BookingSeeder.SeedAsync(context);
            logger.LogInformation("BookingSeeder completed.");

            await ReviewSeeder.SeedAsync(context);
            logger.LogInformation("ReviewSeeder completed.");

            logger.LogInformation("Database seeding completed.");
        }
    }
}