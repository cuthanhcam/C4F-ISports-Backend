using api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.Users.AnyAsync())
            {
                var userAccount = await context.Accounts.FirstOrDefaultAsync(a => a.Email == "user@gmail.com");
                if (userAccount != null)
                {
                    var users = new[]
                    {
                        new User
                        {
                            AccountId = userAccount.AccountId,
                            FullName = "Nguyễn Văn A",
                            Phone = "0901234567",
                            Gender = "Male",
                            DateOfBirth = new DateTime(1990, 1, 1),
                            AvatarUrl = "https://example.com/avatar1.png",
                            LoyaltyPoints = 100,
                            CreatedAt = DateTime.UtcNow,
                            City = "Hà Nội",
                            District = "Cầu Giấy"
                        }
                    };

                    await context.Users.AddRangeAsync(users);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}