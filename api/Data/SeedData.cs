using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using api.Data.Seeders;
using Microsoft.EntityFrameworkCore;

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
                // Remove EnsureCreatedAsync to rely on migrations
                logger.LogInformation("Database migrations will be applied in Program.cs.");

                await AccountSeeder.SeedAsync(context);
                logger.LogInformation("AccountSeeder hoàn tất. Accounts: {Count}", await context.Accounts.CountAsync());

                await SportSeeder.SeedAsync(context);
                logger.LogInformation("SportSeeder hoàn tất. Sports: {Count}", await context.Sports.CountAsync());

                await UserSeeder.SeedAsync(context);
                logger.LogInformation("UserSeeder hoàn tất. Users: {Count}", await context.Users.CountAsync());

                await OwnerSeeder.SeedAsync(context);
                logger.LogInformation("OwnerSeeder hoàn tất. Owners: {Count}", await context.Owners.CountAsync());

                await FieldSeeder.SeedAsync(context);
                logger.LogInformation("FieldSeeder hoàn tất. Fields: {Count}", await context.Fields.CountAsync());

                await SubFieldSeeder.SeedAsync(context);
                logger.LogInformation("SubFieldSeeder hoàn tất. SubFields: {Count}", await context.SubFields.CountAsync());

                await FieldPricingSeeder.SeedAsync(context);
                logger.LogInformation("FieldPricingSeeder hoàn tất. FieldPricings: {Count}", await context.FieldPricings.CountAsync());

                await FieldAmenitySeeder.SeedAsync(context);
                logger.LogInformation("FieldAmenitySeeder hoàn tất. FieldAmenities: {Count}", await context.FieldAmenities.CountAsync());

                await FieldDescriptionSeeder.SeedAsync(context);
                logger.LogInformation("FieldDescriptionSeeder hoàn tất. FieldDescriptions: {Count}", await context.FieldDescriptions.CountAsync());

                await FieldImageSeeder.SeedAsync(context);
                logger.LogInformation("FieldImageSeeder hoàn tất. FieldImages: {Count}", await context.FieldImages.CountAsync());

                await FieldServiceSeeder.SeedAsync(context);
                logger.LogInformation("FieldServiceSeeder hoàn tất. FieldServices: {Count}", await context.FieldServices.CountAsync());

                await PromotionSeeder.SeedAsync(context);
                logger.LogInformation("PromotionSeeder hoàn tất. Promotions: {Count}", await context.Promotions.CountAsync());

                await BookingSeeder.SeedAsync(context);
                logger.LogInformation("BookingSeeder hoàn tất. Bookings: {Count}", await context.Bookings.CountAsync());

                await BookingServiceSeeder.SeedAsync(context);
                logger.LogInformation("BookingServiceSeeder hoàn tất. BookingServices: {Count}", await context.BookingServices.CountAsync());

                await BookingTimeSlotSeeder.SeedAsync(context);
                logger.LogInformation("BookingTimeSlotSeeder hoàn tất. BookingTimeSlots: {Count}", await context.BookingTimeSlots.CountAsync());

                await PaymentSeeder.SeedAsync(context);
                logger.LogInformation("PaymentSeeder hoàn tất. Payments: {Count}", await context.Payments.CountAsync());

                await ReviewSeeder.SeedAsync(context);
                logger.LogInformation("ReviewSeeder hoàn tất. Reviews: {Count}", await context.Reviews.CountAsync());

                await NotificationSeeder.SeedAsync(context);
                logger.LogInformation("NotificationSeeder hoàn tất. Notifications: {Count}", await context.Notifications.CountAsync());

                await FavoriteFieldSeeder.SeedAsync(context);
                logger.LogInformation("FavoriteFieldSeeder hoàn tất. FavoriteFields: {Count}", await context.FavoriteFields.CountAsync());

                await SearchHistorySeeder.SeedAsync(context);
                logger.LogInformation("SearchHistorySeeder hoàn tất. SearchHistories: {Count}", await context.SearchHistories.CountAsync());

                await RefreshTokenSeeder.SeedAsync(context);
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