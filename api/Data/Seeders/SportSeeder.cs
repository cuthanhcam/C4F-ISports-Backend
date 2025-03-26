using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class SportSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (!context.Sports.Any())
            {
                context.Sports.AddRange(
                    new Sport { SportName = "Football" },
                    new Sport { SportName = "Badminton" }
                );
                context.SaveChanges();
            }
        }
    }
}