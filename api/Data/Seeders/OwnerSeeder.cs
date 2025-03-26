using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class OwnerSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (!context.Owners.Any())
            {
                var owner1Account = context.Accounts.First(a => a.Email == "owner1@example.com");
                var owner2Account = context.Accounts.First(a => a.Email == "owner2@example.com");

                context.Owners.AddRange(
                    new Owner
                    {
                        AccountId = owner1Account.AccountId,
                        FullName = "Nguyen Van A",
                        Phone = "0901234567",
                        Email = "owner1@example.com",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Owner
                    {
                        AccountId = owner2Account.AccountId,
                        FullName = "Tran Thi B",
                        Phone = "0912345678",
                        Email = "owner2@example.com",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                );
                context.SaveChanges();
            }
        }
    }
}