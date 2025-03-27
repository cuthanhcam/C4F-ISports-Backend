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
                var user1Account = context.Accounts.First(a => a.Email == "user1@gmail.com");
                var user2Account = context.Accounts.First(a => a.Email == "user2@gmail.com");

                context.Users.AddRange(
                    new User
                    {
                        AccountId = user1Account.AccountId,
                        FullName = "Le Van C",
                        Email = "user1@gmail.com",
                        Phone = "0923456789",
                        Gender = "Male",
                        DateOfBirth = new DateTime(1990, 1, 1),
                        AvatarUrl = "https://drive.google.com/file/d/1LwIlExLLkZrKXiytIWvamIOXDzLfB_kq/view?usp=drive_link"
                    },
                    new User
                    {
                        AccountId = user2Account.AccountId,
                        FullName = "Pham Thi D",
                        Email = "user2@gmail.com",
                        Phone = "0934567890",
                        Gender = "Female",
                        DateOfBirth = new DateTime(1995, 5, 5),
                        AvatarUrl = "https://drive.google.com/file/d/1LwIlExLLkZrKXiytIWvamIOXDzLfB_kq/view?usp=drive_link"
                    }
                );
                context.SaveChanges();
            }
        }
    }
}