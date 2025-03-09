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

            if (!context.Accounts.Any())
            {
                logger.LogInformation("Seeding Accounts...");
                AccountSeeder.Seed(context);
                context.SaveChanges();
                logger.LogInformation("Accounts seeded successfully.");
            }

            if (!context.Users.Any())
            {
                logger.LogInformation("Seeding Users...");
                UserSeeder.Seed(context);
                context.SaveChanges();
                logger.LogInformation("Users seeded successfully.");
            }

            if (!context.Owners.Any())
            {
                logger.LogInformation("Seeding Owners...");
                OwnerSeeder.Seed(context);
                context.SaveChanges();
                logger.LogInformation("Owners seeded successfully.");
            }

            if (!context.Sports.Any())
            {
                logger.LogInformation("Seeding Sports...");
                SportSeeder.Seed(context);
                context.SaveChanges();
                logger.LogInformation("Sports seeded successfully.");
            }

            if (!context.Fields.Any())
            {
                logger.LogInformation("Seeding Fields...");
                FieldSeeder.Seed(context);
                context.SaveChanges();
                logger.LogInformation("Fields seeded successfully.");
            }

            if (!context.FieldImages.Any())
            {
                logger.LogInformation("Seeding FieldImages...");
                FieldImageSeeder.Seed(context);
                context.SaveChanges();
                logger.LogInformation("FieldImages seeded successfully.");
            }

            if (!context.Bookings.Any())
            {
                logger.LogInformation("Seeding Bookings...");
                BookingSeeder.Seed(context);
                context.SaveChanges();
                logger.LogInformation("Bookings seeded successfully.");
            }

            logger.LogInformation("Database seeding completed.");
        }
    }
}