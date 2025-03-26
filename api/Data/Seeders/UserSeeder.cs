using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class UserSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (!context.Users.Any())
            {
                var user1Account = context.Accounts.First(a => a.Email == "user1@example.com");
                var user2Account = context.Accounts.First(a => a.Email == "user2@example.com");

                context.Users.AddRange(
                    new User
                    {
                        AccountId = user1Account.AccountId,
                        FullName = "Le Van C",
                        Email = "user1@example.com",
                        Phone = "0923456789",
                        Gender = "Male",
                        DateOfBirth = new DateTime(1990, 1, 1),
                        AvatarUrl = "https://example.com/avatar1.jpg"
                    },
                    new User
                    {
                        AccountId = user2Account.AccountId,
                        FullName = "Pham Thi D",
                        Email = "user2@example.com",
                        Phone = "0934567890",
                        Gender = "Female",
                        DateOfBirth = new DateTime(1995, 5, 5),
                        AvatarUrl = "https://example.com/avatar2.jpg"
                    }
                );
                context.SaveChanges();
            }
        }
    }
}