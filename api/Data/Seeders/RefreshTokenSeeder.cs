using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class RefreshTokenSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.RefreshTokens.AnyAsync())
            {
                logger?.LogInformation("Seeding RefreshTokens...");
                var account = await context.Accounts.FirstOrDefaultAsync(a => a.Email == "user@gmail.com");
                if (account == null)
                {
                    logger?.LogError("No Account found for seeding RefreshTokens.");
                    return;
                }

                var refreshTokens = new[]
                {
                    new RefreshToken
                    {
                        AccountId = account.AccountId,
                        Account = account, // Gán navigation property
                        Token = "sample_refresh_token_" + Guid.NewGuid().ToString(),
                        Expires = DateTime.UtcNow.AddDays(7),
                        Created = DateTime.UtcNow
                    }
                };

                await context.RefreshTokens.AddRangeAsync(refreshTokens);
                await context.SaveChangesAsync();
                logger?.LogInformation("RefreshTokens seeded successfully. RefreshTokens: {Count}", await context.RefreshTokens.CountAsync());
            }
        }
    }
}