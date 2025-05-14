using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using api.Data.Seeders;

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

            logger.LogInformation("Bắt đầu seeding cơ sở dữ liệu...");

            try
            {
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database đã được đảm bảo.");

                await AccountSeeder.SeedAsync(context);
                logger.LogInformation("AccountSeeder hoàn tất.");

                await SportSeeder.SeedAsync(context);
                logger.LogInformation("SportSeeder hoàn tất.");

                await UserSeeder.SeedAsync(context);
                logger.LogInformation("UserSeeder hoàn tất.");

                await OwnerSeeder.SeedAsync(context);
                logger.LogInformation("OwnerSeeder hoàn tất.");

                await FieldSeeder.SeedAsync(context);
                logger.LogInformation("FieldSeeder hoàn tất.");

                await SubFieldSeeder.SeedAsync(context);
                logger.LogInformation("SubFieldSeeder hoàn tất.");

                await FieldPricingSeeder.SeedAsync(context);
                logger.LogInformation("FieldPricingSeeder hoàn tất.");

                await FieldAmenitySeeder.SeedAsync(context);
                logger.LogInformation("FieldAmenitySeeder hoàn tất.");

                await FieldDescriptionSeeder.SeedAsync(context);
                logger.LogInformation("FieldDescriptionSeeder hoàn tất.");

                await FieldImageSeeder.SeedAsync(context);
                logger.LogInformation("FieldImageSeeder hoàn tất.");

                await FieldServiceSeeder.SeedAsync(context);
                logger.LogInformation("FieldServiceSeeder hoàn tất.");

                await PromotionSeeder.SeedAsync(context);
                logger.LogInformation("PromotionSeeder hoàn tất.");

                await BookingSeeder.SeedAsync(context);
                logger.LogInformation("BookingSeeder hoàn tất.");

                await BookingServiceSeeder.SeedAsync(context);
                logger.LogInformation("BookingServiceSeeder hoàn tất.");

                await BookingTimeSlotSeeder.SeedAsync(context);
                logger.LogInformation("BookingTimeSlotSeeder hoàn tất.");

                await PaymentSeeder.SeedAsync(context);
                logger.LogInformation("PaymentSeeder hoàn tất.");

                await ReviewSeeder.SeedAsync(context);
                logger.LogInformation("ReviewSeeder hoàn tất.");

                await NotificationSeeder.SeedAsync(context);
                logger.LogInformation("NotificationSeeder hoàn tất.");

                await FavoriteFieldSeeder.SeedAsync(context);
                logger.LogInformation("FavoriteFieldSeeder hoàn tất.");

                await SearchHistorySeeder.SeedAsync(context);
                logger.LogInformation("SearchHistorySeeder hoàn tất.");

                await RefreshTokenSeeder.SeedAsync(context);
                logger.LogInformation("RefreshTokenSeeder hoàn tất.");

                logger.LogInformation("Seeding cơ sở dữ liệu hoàn tất thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Đã xảy ra lỗi trong quá trình seeding cơ sở dữ liệu.");
                throw;
            }
        }
    }
}