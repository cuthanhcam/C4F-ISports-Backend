using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class FieldServiceSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!context.FieldServices.Any())
            {
                var field = await context.Fields.FirstOrDefaultAsync(f => f.FieldId == 1);
                if (field == null)
                {
                    return;
                }

                var fieldServices = new List<FieldService>
                {
                    new FieldService
                    {
                        FieldId = field.FieldId,
                        ServiceName = "Thuê vợt",
                        Price = 20000,
                        Description = "Vợt chính hãng"
                    },
                    new FieldService
                    {
                        FieldId = field.FieldId,
                        ServiceName = "Thuê giày",
                        Price = 15000,
                        Description = "Giày cầu lông"
                    }
                };

                await context.FieldServices.AddRangeAsync(fieldServices);
                await context.SaveChangesAsync();
            }
        }
    }
}