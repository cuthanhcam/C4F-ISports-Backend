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
                var account = await context.Accounts.FirstOrDefaultAsync(a => a.Email == "user@gmail.com");
                if (account == null)
                {
                    logger?.LogError("No Account found for seeding Users.");
                    return;
                }

                var users = new[]
                {
                    new User
                    {
                        UserId = 1,
                        AccountId = account.AccountId,
                        Account = account,
                        FullName = "Tran Thi Khach",
                        Phone = "0987654321",
                        Gender = "Female",
                        DateOfBirth = new DateTime(1995, 5, 20),
                        City = "Ho Chi Minh",
                        District = "Quan 1",
                        LoyaltyPoints = 0,
                        CreatedAt = DateTime.UtcNow
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