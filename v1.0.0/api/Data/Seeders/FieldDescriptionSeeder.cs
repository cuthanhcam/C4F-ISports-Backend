using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class FieldDescriptionSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!context.FieldDescriptions.Any())
            {
                var field1 = context.Fields.FirstOrDefault(f => f.FieldName == "Football Field A");
                var field2 = context.Fields.FirstOrDefault(f => f.FieldName == "Football Field B");
                var field3 = context.Fields.FirstOrDefault(f => f.FieldName == "Badminton Court X");
                var field4 = context.Fields.FirstOrDefault(f => f.FieldName == "Badminton Court Y");

                var descriptions = new List<FieldDescription>();

                if (field1 != null)
                    descriptions.Add(new FieldDescription { FieldId = field1.FieldId, Description = "Sân bóng đẹp, rộng rãi, có khán đài." });
                if (field2 != null)
                    descriptions.Add(new FieldDescription { FieldId = field2.FieldId, Description = "Sân bóng chất lượng cao, gần trung tâm." });
                if (field3 != null)
                    descriptions.Add(new FieldDescription { FieldId = field3.FieldId, Description = "Sân cầu lông thoáng mát, sàn gỗ tốt." });
                if (field4 != null)
                    descriptions.Add(new FieldDescription { FieldId = field4.FieldId, Description = "Sân cầu lông hiện đại, đang bảo trì." });

                if (descriptions.Any())
                {
                    context.FieldDescriptions.AddRange(descriptions);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}