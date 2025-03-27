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
                        Email = "admin@gmail.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                        Role = "Admin",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Account
                    {
                        Email = "owner1@gmail.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                        Role = "Owner",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Account
                    {
                        Email = "user1@gmail.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                        Role = "User",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Account
                    {
                        Email = "owner2@gmail.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                        Role = "Owner",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Account
                    {
                        Email = "user2@gmail.com",
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