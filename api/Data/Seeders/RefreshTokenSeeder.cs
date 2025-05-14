using api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class RefreshTokenSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.RefreshTokens.AnyAsync())
            {
                var account = await context.Accounts.FirstOrDefaultAsync(a => a.Email == "user@gmail.com");
                if (account != null)
                {
                    var refreshTokens = new[]
                    {
                        new RefreshToken
                        {
                            AccountId = account.AccountId,
                            Token = "sample_refresh_token_" + Guid.NewGuid().ToString(),
                            Expires = DateTime.UtcNow.AddDays(7),
                            Created = DateTime.UtcNow
                        }
                    };

                    await context.RefreshTokens.AddRangeAsync(refreshTokens);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}