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
                        Email = "user@example.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                        Role = "User",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        LastLogin = null,
                        ResetToken = null,
                        ResetTokenExpiry = null,
                        VerificationToken = null,
                        VerificationTokenExpiry = null
                    },
                    new Account
                    {
                        Email = "owner@example.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                        Role = "Owner",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        LastLogin = null,
                        ResetToken = null,
                        ResetTokenExpiry = null,
                        VerificationToken = null,
                        VerificationTokenExpiry = null
                    },
                    new Account
                    {
                        Email = "admin@example.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                        Role = "Admin",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        LastLogin = null,
                        ResetToken = null,
                        ResetTokenExpiry = null,
                        VerificationToken = null,
                        VerificationTokenExpiry = null
                    }
                );

                context.SaveChanges(); // Lưu thay đổi vào database
            }
        }
    }
}