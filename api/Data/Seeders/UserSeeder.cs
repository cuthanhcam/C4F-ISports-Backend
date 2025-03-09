using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class UserSeeder
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User { UserId = 1, AccountId = 2, FullName = "John Doe", Email = "user@example.com", Phone = "123456789", Gender = "Male", DateOfBirth = new DateTime(1995, 5, 20), AvatarUrl = "https://your-cloud-storage.com/user1.jpg" }
            );
        }
    }
}