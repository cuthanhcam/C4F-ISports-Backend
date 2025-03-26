using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class FieldImageSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (!context.FieldImages.Any())
            {
                var field1 = context.Fields.First(f => f.FieldName == "Football Field A");
                var field2 = context.Fields.First(f => f.FieldName == "Football Field B");
                var field3 = context.Fields.First(f => f.FieldName == "Badminton Court X");

                context.FieldImages.AddRange(
                    new FieldImage { FieldId = field1.FieldId, ImageUrl = "https://example.com/field1.jpg" },
                    new FieldImage { FieldId = field2.FieldId, ImageUrl = "https://example.com/field2.jpg" },
                    new FieldImage { FieldId = field3.FieldId, ImageUrl = "https://example.com/field3.jpg" }
                );
                context.SaveChanges();
            }
        }
    }
}