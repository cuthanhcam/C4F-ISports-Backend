using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class BookingTimeSlotSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            // BookingTimeSlots are now handled in BookingSeeder
            // This is intentionally empty to avoid duplicating time slots
            logger?.LogInformation("BookingTimeSlots are now seeded as part of Booking. Skipping separate seeding.");
            return;
        }
    }
}