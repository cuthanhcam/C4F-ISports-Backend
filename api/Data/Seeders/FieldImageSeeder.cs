using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class FieldImageSeeder
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FieldImage>().HasData(
                new FieldImage { FieldImageId = 1, FieldId = 1, Thumbnail = "https://your-cloud-storage.com/field1-thumb.jpg", ImageUrl = "https://your-cloud-storage.com/field1.jpg" }
            );
        }
    }
}