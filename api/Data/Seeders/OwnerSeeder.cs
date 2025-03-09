using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class OwnerSeeder
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Owner>().HasData(
                new Owner { OwnerId = 1, AccountId = 1, FullName = "Admin Owner", Phone = "987654321", Email = "admin@example.com" }
            );
        }
    }
}