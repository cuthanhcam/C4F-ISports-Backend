using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class RefreshTokenSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.RefreshTokens.IgnoreQueryFilters().AnyAsync())
            {
                logger?.LogInformation("Seeding RefreshTokens...");
                var account = await context.Accounts.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Email == "user@gmail.com");
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
                        Token = "sample-refresh-token-123",
                        Expires = DateTime.UtcNow.AddDays(7),
                        Created = DateTime.UtcNow
                    }
                };

                try
                {
                    await context.RefreshTokens.AddRangeAsync(refreshTokens);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("RefreshTokens seeded successfully. RefreshTokens: {Count}", await context.RefreshTokens.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed RefreshTokens.");
                    throw;
                }
            }
            else
            {
                logger?.LogInformation("RefreshTokens already seeded. Skipping...");
            }
        }
    }
}