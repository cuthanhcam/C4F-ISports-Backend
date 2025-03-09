using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class FieldSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (!context.Fields.Any())
            {
                var owner = context.Owners.FirstOrDefault(o => o.Email == "owner@example.com");
                var sport = context.Sports.FirstOrDefault(s => s.SportName == "Football");
                if (owner != null && sport != null)
                {
                    context.Fields.Add(
                        new Field
                        {
                            SportId = sport.SportId,
                            FieldName = "Football Field A",
                            Phone = "0123456789",
                            Address = "123 Main St, City",
                            OpenHours = "08:00-22:00",
                            OwnerId = owner.OwnerId,
                            Status = "Active",
                            Latitude = 10.7769m,
                            Longitude = 106.7009m,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    );
                }
            }
        }
    }
}