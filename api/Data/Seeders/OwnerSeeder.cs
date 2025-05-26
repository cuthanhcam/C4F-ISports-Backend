using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class OwnerSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.Owners.AnyAsync())
            {
                logger?.LogInformation("Seeding Owners...");
                var account = await context.Accounts.FirstOrDefaultAsync(a => a.Email == "owner@gmail.com");
                if (account == null)
                {
                    logger?.LogError("No Account found for seeding Owners.");
                    return;
                }

                var owners = new[]
                {
                    new Owner
                    {
                        OwnerId = 1,
                        AccountId = account.AccountId,
                        Account = account,
                        FullName = "Nguyen Van Chu",
                        Phone = "0123456789",
                        Description = "Chủ sở hữu sân thể thao ABC",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                try
                {
                    await context.Owners.AddRangeAsync(owners);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Owners seeded successfully. Owners: {Count}", await context.Owners.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed Owners. StackTrace: {StackTrace}", ex.StackTrace);
                    throw;
                }
            }
            else
            {
                logger?.LogInformation("Owners already seeded. Skipping...");
            }
        }
    }
}