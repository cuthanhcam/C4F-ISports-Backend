using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.Users.AnyAsync())
            {
                logger?.LogInformation("Seeding Users...");
                var userAccount = await context.Accounts.FirstOrDefaultAsync(a => a.Email == "user@gmail.com");
                if (userAccount == null)
                {
                    logger?.LogError("No User Account found for seeding Users.");
                    return;
                }

                var users = new[]
                {
                    new User
                    {
                        AccountId = userAccount.AccountId,
                        Account = userAccount,
                        FullName = "Nguyễn Văn A",
                        Phone = "0901234567",
                        Gender = "Male",
                        DateOfBirth = new DateTime(1990, 1, 1),
                        AvatarUrl = "https://example.com/avatar1.png",
                        LoyaltyPoints = 100,
                        CreatedAt = DateTime.UtcNow,
                        City = "Hà Nội",
                        District = "Cầu Giấy"
                    }
                };

                try
                {
                    await context.Users.AddRangeAsync(users);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Users seeded successfully. Users: {Count}", await context.Users.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed Users. StackTrace: {StackTrace}", ex.StackTrace);
                    throw;
                }
            }
            else
            {
                logger?.LogInformation("Users already seeded. Skipping...");
            }
        }
    }
}