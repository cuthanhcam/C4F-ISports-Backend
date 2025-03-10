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
                var field = context.Fields.FirstOrDefault(f => f.FieldName == "Football Field A");
                if (field != null)
                {
                    context.FieldImages.Add(
                        new FieldImage
                        {
                            FieldId = field.FieldId,
                            Thumbnail = "https://res.cloudinary.com/dboluzvfu/image/upload/v1234567890/thumb.jpg",
                            ImageUrl = "https://res.cloudinary.com/dboluzvfu/image/upload/v1234567890/field.jpg"
                        }
                    );
                }
            }
        }
    }
}