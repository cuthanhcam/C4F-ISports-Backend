using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class FieldAmenitySeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!context.FieldAmenities.Any())
            {
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldId == 1);
                if (field == null)
                {
                    return;
                }

                var fieldAmenities = new List<FieldAmenity>
                {
                    new FieldAmenity { FieldId = field.FieldId, AmenityName = "Wifi", Description = "Miễn phí tốc độ cao" },
                    new FieldAmenity { FieldId = field.FieldId, AmenityName = "Quầy bar", Description = "Đồ uống đa dạng" }
                };

                await context.FieldAmenities.AddRangeAsync(fieldAmenities);
                await context.SaveChangesAsync();
            }
        }
    }
}