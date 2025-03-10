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
                var ownerAccount = context.Accounts.FirstOrDefault(a => a.Email == "owner@example.com");
                if (ownerAccount != null)
                {
                    context.Owners.Add(
                        new Owner
                        {
                            AccountId = ownerAccount.AccountId, // Lấy AccountId tự động
                            FullName = "Jane Smith",
                            Phone = "0987654321",
                            Email = "owner@example.com"
                        }
                    );
                }
            }
        }
    }
}