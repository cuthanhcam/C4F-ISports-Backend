using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class FieldDescriptionSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (!context.FieldDescriptions.Any())
            {
                var field1 = context.Fields.First(f => f.FieldName == "Football Field A");
                var field2 = context.Fields.First(f => f.FieldName == "Football Field B");
                var field3 = context.Fields.First(f => f.FieldName == "Badminton Court X");
                var field4 = context.Fields.First(f => f.FieldName == "Badminton Court Y");

                context.FieldDescriptions.AddRange(
                    new FieldDescription { FieldId = field1.FieldId, Description = "Sân bóng đẹp, rộng rãi, có khán đài." },
                    new FieldDescription { FieldId = field2.FieldId, Description = "Sân bóng chất lượng cao, gần trung tâm." },
                    new FieldDescription { FieldId = field3.FieldId, Description = "Sân cầu lông thoáng mát, sàn gỗ tốt." },
                    new FieldDescription { FieldId = field4.FieldId, Description = "Sân cầu lông hiện đại, đang bảo trì." }
                );
                context.SaveChanges();
            }
        }
    }
}