using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using api.Data.Seeders;
using api.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var cloudinaryService = scope.ServiceProvider.GetRequiredService<ICloudinaryService>();
            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
            var geocodingService = scope.ServiceProvider.GetRequiredService<IGeocodingService>();
            var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("SeedData");

            logger.LogInformation("Bắt đầu seeding cơ sở dữ liệu...");

            try
            {
                logger.LogInformation("Database migrations will be applied in Program.cs.");

                await AccountSeeder.SeedAsync(context, logger);
                logger.LogInformation("AccountSeeder hoàn tất. Accounts: {Count}", await context.Accounts.CountAsync());

                await SportSeeder.SeedAsync(context, logger);
                logger.LogInformation("SportSeeder hoàn tất. Sports: {Count}", await context.Sports.CountAsync());

                await UserSeeder.SeedAsync(context, logger);
                logger.LogInformation("UserSeeder hoàn tất. Users: {Count}", await context.Users.CountAsync());

                await OwnerSeeder.SeedAsync(context, logger);
                logger.LogInformation("OwnerSeeder hoàn tất. Owners: {Count}", await context.Owners.CountAsync());

                await FieldSeeder.SeedAsync(context, geocodingService, logger);
                logger.LogInformation("FieldSeeder hoàn tất. Fields: {Count}", await context.Fields.CountAsync());

                await SubFieldSeeder.SeedAsync(context, logger);
                logger.LogInformation("SubFieldSeeder hoàn tất. SubFields: {Count}", await context.SubFields.CountAsync());

                await FieldPricingSeeder.SeedAsync(context, logger);
                logger.LogInformation("FieldPricingSeeder hoàn tất. FieldPricings: {Count}", await context.FieldPricings.CountAsync());

                await FieldAmenitySeeder.SeedAsync(context, logger);
                logger.LogInformation("FieldAmenitySeeder hoàn tất. FieldAmenities: {Count}", await context.FieldAmenities.CountAsync());

                await FieldDescriptionSeeder.SeedAsync(context, logger);
                logger.LogInformation("FieldDescriptionSeeder hoàn tất. FieldDescriptions: {Count}", await context.FieldDescriptions.CountAsync());

                await FieldImageSeeder.SeedAsync(context, cloudinaryService, logger);
                logger.LogInformation("FieldImageSeeder hoàn tất. FieldImages: {Count}", await context.FieldImages.CountAsync());

                await FieldServiceSeeder.SeedAsync(context, logger);
                logger.LogInformation("FieldServiceSeeder hoàn tất. FieldServices: {Count}", await context.FieldServices.CountAsync());

                await PromotionSeeder.SeedAsync(context, logger);
                logger.LogInformation("PromotionSeeder hoàn tất. Promotions: {Count}", await context.Promotions.CountAsync());

                await BookingSeeder.SeedAsync(context, logger);
                logger.LogInformation("BookingSeeder hoàn tất. Bookings: {Count}", await context.Bookings.CountAsync());

                await BookingServiceSeeder.SeedAsync(context, logger);
                logger.LogInformation("BookingServiceSeeder hoàn tất. BookingServices: {Count}", await context.BookingServices.CountAsync());

                await BookingTimeSlotSeeder.SeedAsync(context, logger);
                logger.LogInformation("BookingTimeSlotSeeder hoàn tất. BookingTimeSlots: {Count}", await context.BookingTimeSlots.CountAsync());

                await PaymentSeeder.SeedAsync(context, logger);
                logger.LogInformation("PaymentSeeder hoàn tất. Payments: {Count}", await context.Payments.CountAsync());

                await ReviewSeeder.SeedAsync(context, logger);
                logger.LogInformation("ReviewSeeder hoàn tất. Reviews: {Count}", await context.Reviews.CountAsync());

                await NotificationSeeder.SeedAsync(context, emailSender, logger);
                logger.LogInformation("NotificationSeeder hoàn tất. Notifications: {Count}", await context.Notifications.CountAsync());

                await FavoriteFieldSeeder.SeedAsync(context, logger);
                logger.LogInformation("FavoriteFieldSeeder hoàn tất. FavoriteFields: {Count}", await context.FavoriteFields.CountAsync());

                await SearchHistorySeeder.SeedAsync(context, cache, logger);
                logger.LogInformation("SearchHistorySeeder hoàn tất. SearchHistories: {Count}", await context.SearchHistories.CountAsync());

                await RefreshTokenSeeder.SeedAsync(context, logger);
                logger.LogInformation("RefreshTokenSeeder hoàn tất. RefreshTokens: {Count}", await context.RefreshTokens.CountAsync());

                logger.LogInformation("Seeding cơ sở dữ liệu hoàn tất thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Đã xảy ra lỗi trong quá trình seeding cơ sở dữ liệu. StackTrace: {StackTrace}", ex.StackTrace);
                throw;
            }
        }
    }
}