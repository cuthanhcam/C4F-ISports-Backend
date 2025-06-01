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
            if (!await context.Owners.IgnoreQueryFilters().AnyAsync())
            {
                logger?.LogInformation("Seeding Owners...");
                var account = await context.Accounts.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Email == "owner@gmail.com");
                if (account == null)
                {
                    logger?.LogError("No Account found for seeding Owners.");
                    return;
                }

                var owners = new[]
                {
                    new Owner
                    {
                        AccountId = account.AccountId,
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
                    logger?.LogError(ex, "Failed to seed Owners.");
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