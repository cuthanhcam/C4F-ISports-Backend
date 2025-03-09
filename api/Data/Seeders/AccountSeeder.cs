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
                        // Không chỉ định AccountId
                        Email = "user@example.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                        Role = "User",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        LastLogin = null,
                        RefreshToken = null,
                        RefreshTokenExpiry = null
                    },
                    new Account
                    {
                        // Không chỉ định AccountId
                        Email = "owner@example.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                        Role = "Owner",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        LastLogin = null,
                        RefreshToken = null,
                        RefreshTokenExpiry = null
                    },
                    new Account
                    {
                        // Không chỉ định AccountId
                        Email = "admin@example.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                        Role = "Admin",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        LastLogin = null,
                        RefreshToken = null,
                        RefreshTokenExpiry = null
                    }
                );
            }
        }
    }
}