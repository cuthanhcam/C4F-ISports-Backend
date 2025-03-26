using api.Data.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace api.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("SeedData");

            logger.LogInformation("Starting database seeding...");

            // Seed Accounts
            if (!context.Accounts.Any())
            {
                logger.LogInformation("Seeding Accounts...");
                AccountSeeder.Seed(context);
                logger.LogInformation("Accounts seeded successfully.");
            }

            // Seed Users
            if (!context.Users.Any())
            {
                logger.LogInformation("Seeding Users...");
                UserSeeder.Seed(context);
                logger.LogInformation("Users seeded successfully.");
            }

            // Seed Owners
            if (!context.Owners.Any())
            {
                logger.LogInformation("Seeding Owners...");
                OwnerSeeder.Seed(context);
                logger.LogInformation("Owners seeded successfully.");
            }

            // Seed Sports
            if (!context.Sports.Any())
            {
                logger.LogInformation("Seeding Sports...");
                SportSeeder.Seed(context);
                logger.LogInformation("Sports seeded successfully.");
            }

            // Seed Fields
            if (!context.Fields.Any())
            {
                logger.LogInformation("Seeding Fields...");
                FieldSeeder.Seed(context);
                logger.LogInformation("Fields seeded successfully.");
            }

            // Seed FieldDescriptions
            if (!context.FieldDescriptions.Any())
            {
                logger.LogInformation("Seeding FieldDescriptions...");
                FieldDescriptionSeeder.Seed(context);
                logger.LogInformation("FieldDescriptions seeded successfully.");
            }

            // Seed FieldImages
            if (!context.FieldImages.Any())
            {
                logger.LogInformation("Seeding FieldImages...");
                FieldImageSeeder.Seed(context);
                logger.LogInformation("FieldImages seeded successfully.");
            }

            // Seed FieldAmenities
            if (!context.FieldAmenities.Any())
            {
                logger.LogInformation("Seeding FieldAmenities...");
                FieldAmenitySeeder.Seed(context);
                logger.LogInformation("FieldAmenities seeded successfully.");
            }

            // Seed FieldPricings
            if (!context.FieldPricings.Any())
            {
                logger.LogInformation("Seeding FieldPricings...");
                FieldPricingSeeder.Seed(context);
                logger.LogInformation("FieldPricings seeded successfully.");
            }

            // Seed Services
            if (!context.Services.Any())
            {
                logger.LogInformation("Seeding Services...");
                ServiceSeeder.Seed(context);
                logger.LogInformation("Services seeded successfully.");
            }

            // Seed Bookings
            if (!context.Bookings.Any())
            {
                logger.LogInformation("Seeding Bookings...");
                BookingSeeder.Seed(context);
                logger.LogInformation("Bookings seeded successfully.");
            }

            // Seed Reviews
            if (!context.Reviews.Any())
            {
                logger.LogInformation("Seeding Reviews...");
                ReviewSeeder.Seed(context);
                logger.LogInformation("Reviews seeded successfully.");
            }

            logger.LogInformation("Database seeding completed.");
        }
    }
}