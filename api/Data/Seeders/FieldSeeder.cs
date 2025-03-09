using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class FieldSeeder
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Field>().HasData(
                new Field { FieldId = 1, SportId = 1, OwnerId = 1, FieldName = "Sân A", Phone = "0123456789", Address = "123 Đường ABC, TP.HCM", OpenHours = "06:00 - 22:00", Status = "Active", Latitude = 10.7769m, Longitude = 106.7009m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );
        }
    }
}