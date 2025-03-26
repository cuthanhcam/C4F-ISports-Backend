using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class FieldPricingSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (!context.FieldPricings.Any())
            {
                var field1 = context.Fields.First(f => f.FieldName == "Football Field A");
                var field2 = context.Fields.First(f => f.FieldName == "Football Field B");
                var field3 = context.Fields.First(f => f.FieldName == "Badminton Court X");

                context.FieldPricings.AddRange(
                    new FieldPricing { FieldId = field1.FieldId, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(12), DayOfWeek = 1, Price = 500000 },
                    new FieldPricing { FieldId = field1.FieldId, StartTime = TimeSpan.FromHours(18), EndTime = TimeSpan.FromHours(22), DayOfWeek = 1, Price = 700000 },
                    new FieldPricing { FieldId = field2.FieldId, StartTime = TimeSpan.FromHours(7), EndTime = TimeSpan.FromHours(11), DayOfWeek = 2, Price = 450000 },
                    new FieldPricing { FieldId = field3.FieldId, StartTime = TimeSpan.FromHours(6), EndTime = TimeSpan.FromHours(10), DayOfWeek = 3, Price = 200000 }
                );
                context.SaveChanges();
            }
        }
    }
}