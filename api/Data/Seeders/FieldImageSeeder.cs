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
                    new FieldImage { FieldId = field1.FieldId, ImageUrl = "https://drive.google.com/file/d/1FYAj_423LzrnzD70PUykfqRFUIbd-LVC/view?usp=drive_link" },
                    new FieldImage { FieldId = field2.FieldId, ImageUrl = "https://drive.google.com/file/d/11GsdxasHRWmAjYf9H4GTS3C2ArkXhjan/view?usp=drive_link" },
                    new FieldImage { FieldId = field3.FieldId, ImageUrl = "https://drive.google.com/file/d/16jmCurClrNNbXVJcl6_mR7gng4fjDTsy/view?usp=drive_link" }
                );
                context.SaveChanges();
            }
        }
    }
}