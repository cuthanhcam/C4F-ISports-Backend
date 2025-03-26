using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class FieldAmenitySeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (!context.FieldAmenities.Any())
            {
                var field1 = context.Fields.First(f => f.FieldName == "Football Field A");
                var field3 = context.Fields.First(f => f.FieldName == "Badminton Court X");

                context.FieldAmenities.AddRange(
                    new FieldAmenity { FieldId = field1.FieldId, AmenityName = "Wi-Fi" },
                    new FieldAmenity { FieldId = field1.FieldId, AmenityName = "Phòng thay đồ" },
                    new FieldAmenity { FieldId = field3.FieldId, AmenityName = "Máy lạnh" }
                );
                context.SaveChanges();
            }
        }
    }
}