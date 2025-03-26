using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class ServiceSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (!context.Services.Any())
            {
                var field1 = context.Fields.First(f => f.FieldName == "Football Field A");
                var field3 = context.Fields.First(f => f.FieldName == "Badminton Court X");

                context.Services.AddRange(
                    new Service { FieldId = field1.FieldId, ServiceName = "Nước uống", Price = 20000 },
                    new Service { FieldId = field3.FieldId, ServiceName = "Thuê vợt", Price = 50000 }
                );
                context.SaveChanges();
            }
        }
    }
}