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
                await AccountSeeder.SeedAsync(context, logger);
                logger.LogInformation("AccountSeeder hoàn tất. Accounts: {Count}", await context.Accounts.IgnoreQueryFilters().CountAsync());
                
                await SportSeeder.SeedAsync(context, logger);
                logger.LogInformation("SportSeeder hoàn tất. Sports: {Count}", await context.Sports.IgnoreQueryFilters().CountAsync());

                await UserSeeder.SeedAsync(context, logger);
                logger.LogInformation("UserSeeder hoàn tất. Users: {Count}", await context.Users.IgnoreQueryFilters().CountAsync());

                await OwnerSeeder.SeedAsync(context, logger);
                logger.LogInformation("OwnerSeeder hoàn tất. Owners: {Count}", await context.Owners.IgnoreQueryFilters().CountAsync());

                await FieldSeeder.SeedAsync(context, geocodingService, logger);
                logger.LogInformation("FieldSeeder hoàn tất. Fields: {Count}", await context.Fields.IgnoreQueryFilters().CountAsync());

                await SubFieldSeeder.SeedAsync(context, logger);
                logger.LogInformation("SubFieldSeeder hoàn tất. SubFields: {Count}", await context.SubFields.IgnoreQueryFilters().CountAsync());

                await TimeSlotSeeder.SeedAsync(context, logger);
                logger.LogInformation("TimeSlotSeeder hoàn tất. TimeSlots: {Count}", await context.TimeSlots.IgnoreQueryFilters().CountAsync());

                await PricingRuleSeeder.SeedAsync(context, logger);
                logger.LogInformation("PricingRuleSeeder hoàn tất. PricingRules: {Count}", await context.PricingRules.IgnoreQueryFilters().CountAsync());

                await FieldPricingSeeder.SeedAsync(context, logger);
                logger.LogInformation("FieldPricingSeeder hoàn tất. FieldPricings: {Count}", await context.FieldPricings.IgnoreQueryFilters().CountAsync());

                await FieldAmenitySeeder.SeedAsync(context, logger);
                logger.LogInformation("FieldAmenitySeeder hoàn tất. FieldAmenities: {Count}", await context.FieldAmenities.IgnoreQueryFilters().CountAsync());

                await FieldDescriptionSeeder.SeedAsync(context, logger);
                logger.LogInformation("FieldDescriptionSeeder hoàn tất. FieldDescriptions: {Count}", await context.FieldDescriptions.IgnoreQueryFilters().CountAsync());

                await FieldImageSeeder.SeedAsync(context, cloudinaryService, logger);
                logger.LogInformation("FieldImageSeeder hoàn tất. FieldImages: {Count}", await context.FieldImages.IgnoreQueryFilters().CountAsync());

                await FieldServiceSeeder.SeedAsync(context, logger);
                logger.LogInformation("FieldServiceSeeder hoàn tất. FieldServices: {Count}", await context.FieldServices.IgnoreQueryFilters().CountAsync());

                await PromotionSeeder.SeedAsync(context, logger);
                logger.LogInformation("PromotionSeeder hoàn tất. Promotions: {Count}", await context.Promotions.IgnoreQueryFilters().CountAsync());

                await BookingSeeder.SeedAsync(context, logger);
                logger.LogInformation("BookingSeeder hoàn tất. Bookings: {Count}", await context.Bookings.IgnoreQueryFilters().CountAsync());

                await BookingServiceSeeder.SeedAsync(context, logger);
                logger.LogInformation("BookingServiceSeeder hoàn tất. BookingServices: {Count}", await context.BookingServices.IgnoreQueryFilters().CountAsync());

                await BookingTimeSlotSeeder.SeedAsync(context, logger);
                logger.LogInformation("BookingTimeSlotSeeder hoàn tất. BookingTimeSlots: {Count}", await context.BookingTimeSlots.IgnoreQueryFilters().CountAsync());

                await PaymentSeeder.SeedAsync(context, logger);
                logger.LogInformation("PaymentSeeder hoàn tất. Payments: {Count}", await context.Payments.IgnoreQueryFilters().CountAsync());

                await ReviewSeeder.SeedAsync(context, logger);
                logger.LogInformation("ReviewSeeder hoàn tất. Reviews: {Count}", await context.Reviews.IgnoreQueryFilters().CountAsync());

                await NotificationSeeder.SeedAsync(context, emailSender, logger);
                logger.LogInformation("NotificationSeeder hoàn tất. Notifications: {Count}", await context.Notifications.IgnoreQueryFilters().CountAsync());

                await FavoriteFieldSeeder.SeedAsync(context, logger);
                logger.LogInformation("FavoriteFieldSeeder hoàn tất. FavoriteFields: {Count}", await context.FavoriteFields.IgnoreQueryFilters().CountAsync());

                await SearchHistorySeeder.SeedAsync(context, cache, logger);
                logger.LogInformation("SearchHistorySeeder hoàn tất. SearchHistories: {Count}", await context.SearchHistories.IgnoreQueryFilters().CountAsync());

                await RefreshTokenSeeder.SeedAsync(context, logger);
                logger.LogInformation("RefreshTokenSeeder hoàn tất. RefreshTokens: {Count}", await context.RefreshTokens.IgnoreQueryFilters().CountAsync());
                
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