using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class AccountSeeder
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().HasData(
                new Account { AccountId = 1, Email = "admin@example.com", Password = "hashedpassword", Role = "Admin", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Account { AccountId = 2, Email = "user@example.com", Password = "hashedpassword", Role = "User", IsActive = true, CreatedAt = DateTime.UtcNow }
            );
        }
    }
}