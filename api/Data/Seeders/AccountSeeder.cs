using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class AccountSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (!context.Accounts.Any())
            {
                context.Accounts.AddRange(
                    new Account
                    {
                        Email = "admin@example.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                        Role = "Admin",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Account
                    {
                        Email = "owner1@example.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                        Role = "Owner",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Account
                    {
                        Email = "user1@example.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                        Role = "User",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Account
                    {
                        Email = "owner2@example.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                        Role = "Owner",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Account
                    {
                        Email = "user2@example.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                        Role = "User",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                );
                context.SaveChanges();
            }
        }
    }
}