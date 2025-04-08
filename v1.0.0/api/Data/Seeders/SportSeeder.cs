using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data.Seeders
{
    public static class SportSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!context.Sports.Any())
            {
                context.Sports.AddRange(
                    new Sport { SportName = "Football" },
                    new Sport { SportName = "Badminton" }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}