using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class AccountSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.Accounts.AnyAsync())
            {
                var accounts = new List<Account>
                {
                    new Account
                    {
                        Email = "admin@gmail.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                        Role = "Admin",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        VerificationToken = null,
                        VerificationTokenExpiry = null
                    },
                    new Account
                    {
                        Email = "owner@gmail.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("Owner@123"),
                        Role = "Owner",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        VerificationToken = null,
                        VerificationTokenExpiry = null
                    },
                    new Account
                    {
                        Email = "user@gmail.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("User@123"),
                        Role = "User",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        VerificationToken = null,
                        VerificationTokenExpiry = null
                    }
                };

                await context.Accounts.AddRangeAsync(accounts);
                await context.SaveChangesAsync();
            }
        }
    }
}