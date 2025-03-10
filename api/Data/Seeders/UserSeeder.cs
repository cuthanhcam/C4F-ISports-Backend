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
                var userAccount = context.Accounts.FirstOrDefault(a => a.Email == "user@example.com");
                if (userAccount != null)
                {
                    context.Users.Add(
                        new User
                        {
                            AccountId = userAccount.AccountId, // Lấy AccountId tự động
                            FullName = "John Doe",
                            Email = "user@example.com",
                            Phone = "1234567890",
                            Gender = "Male",
                            DateOfBirth = new DateTime(1990, 1, 1),
                            AvatarUrl = "https://res.cloudinary.com/dboluzvfu/image/upload/v1234567890/avatar.jpg"
                        }
                    );
                }
            }
        }
    }
}