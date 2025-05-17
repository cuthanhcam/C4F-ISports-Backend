using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
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
                var ownerAccount = await context.Accounts.FirstOrDefaultAsync(a => a.Email == "owner@gmail.com");
                if (ownerAccount == null)
                {
                    logger?.LogError("No Owner Account found for seeding Owners.");
                    return;
                }

                var owners = new[]
                {
                    new Owner
                    {
                        AccountId = ownerAccount.AccountId,
                        Account = ownerAccount, // Gán navigation property
                        FullName = "Trần Thị B",
                        Phone = "0912345678",
                        Description = "Chủ sân bóng uy tín tại Hà Nội.",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                await context.Owners.AddRangeAsync(owners);
                await context.SaveChangesAsync();
                logger?.LogInformation("Owners seeded successfully. Owners: {Count}", await context.Owners.CountAsync());
            }
        }
    }
}