using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using BCrypt.Net;

namespace api.Data.Seeders
{
    public static class AccountSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.Accounts.IgnoreQueryFilters().AnyAsync())
            {
                logger?.LogInformation("Seeding Accounts...");
                var accounts = new List<Account>
        {
            new Account
            {
                Email = "admin@gmail.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Account
            {
                Email = "owner@gmail.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = "Owner",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Account
            {
                Email = "user@gmail.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

                try
                {
                    await context.Accounts.AddRangeAsync(accounts);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Accounts seeded successfully. Accounts: {Count}", await context.Accounts.CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed Accounts.");
                    throw;
                }
            }
            else
            {
                logger?.LogInformation("Accounts already seeded. Skipping...");
            }
        }
    }
}