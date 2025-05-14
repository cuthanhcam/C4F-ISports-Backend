using api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class OwnerSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.Owners.AnyAsync())
            {
                var ownerAccount = await context.Accounts.FirstOrDefaultAsync(a => a.Email == "owner@gmail.com");
                if (ownerAccount != null)
                {
                    var owners = new[]
                    {
                        new Owner
                        {
                            AccountId = ownerAccount.AccountId,
                            FullName = "Trần Thị B",
                            Phone = "0912345678",
                            Description = "Chủ sân bóng uy tín tại Hà Nội.",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    await context.Owners.AddRangeAsync(owners);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}