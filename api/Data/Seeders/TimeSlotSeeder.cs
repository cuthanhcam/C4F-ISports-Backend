using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class TimeSlotSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger = null)
        {
            if (!await context.Set<TimeSlot>().IgnoreQueryFilters().AnyAsync())
            {
                logger?.LogInformation("Seeding TimeSlots...");

                var timeSlots = new List<TimeSlot>();

                for (int hour = 5; hour < 12; hour++)
                {
                    timeSlots.Add(new TimeSlot
                    {
                        StartTime = new TimeSpan(hour, 0, 0),
                        EndTime = new TimeSpan(hour, 30, 0),
                        PricePerSlot = 100000m
                    });
                    timeSlots.Add(new TimeSlot
                    {
                        StartTime = new TimeSpan(hour, 30, 0),
                        EndTime = new TimeSpan(hour + 1, 0, 0),
                        PricePerSlot = 100000m
                    });
                }

                for (int hour = 12; hour < 17; hour++)
                {
                    timeSlots.Add(new TimeSlot
                    {
                        StartTime = new TimeSpan(hour, 0, 0),
                        EndTime = new TimeSpan(hour, 30, 0),
                        PricePerSlot = 150000m
                    });
                    timeSlots.Add(new TimeSlot
                    {
                        StartTime = new TimeSpan(hour, 30, 0),
                        EndTime = new TimeSpan(hour + 1, 0, 0),
                        PricePerSlot = 150000m
                    });
                }

                for (int hour = 17; hour < 23; hour++)
                {
                    timeSlots.Add(new TimeSlot
                    {
                        StartTime = new TimeSpan(hour, 0, 0),
                        EndTime = new TimeSpan(hour, 30, 0),
                        PricePerSlot = 200000m
                    });
                    timeSlots.Add(new TimeSlot
                    {
                        StartTime = new TimeSpan(hour, 30, 0),
                        EndTime = hour == 22 ? new TimeSpan(23, 0, 0) : new TimeSpan(hour + 1, 0, 0),
                        PricePerSlot = 200000m
                    });
                }

                try
                {
                    await context.Set<TimeSlot>().AddRangeAsync(timeSlots);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("TimeSlots seeded successfully. TimeSlots: {Count}", await context.Set<TimeSlot>().CountAsync());
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to seed TimeSlots.");
                    throw;
                }
            }
            else
            {
                logger?.LogInformation("TimeSlots already seeded. Skipping...");
            }
        }
    }
}