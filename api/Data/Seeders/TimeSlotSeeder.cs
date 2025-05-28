using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Data.Seeders
{
    public static class TimeSlotSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
        {
            try
            {
                if (await context.TimeSlots.AnyAsync())
                {
                    logger.LogInformation("TimeSlots đã tồn tại, bỏ qua seeding.");
                    return;
                }

                logger.LogInformation("Bắt đầu seeding TimeSlots...");

                // Create standard time slots from 5:00 to 23:00 with 30-minute intervals
                var timeSlots = new List<TimeSlot>();
                
                // Morning prices (lower)
                for (int hour = 5; hour < 12; hour++)
                {
                    timeSlots.Add(new TimeSlot
                    {
                        StartTime = new TimeSpan(hour, 0, 0),
                        EndTime = new TimeSpan(hour, 30, 0),
                        PricePerSlot = 100000M // 100k VND for morning slots
                    });
                    
                    timeSlots.Add(new TimeSlot
                    {
                        StartTime = new TimeSpan(hour, 30, 0),
                        EndTime = new TimeSpan(hour + 1, 0, 0),
                        PricePerSlot = 100000M
                    });
                }
                
                // Afternoon prices (medium)
                for (int hour = 12; hour < 17; hour++)
                {
                    timeSlots.Add(new TimeSlot
                    {
                        StartTime = new TimeSpan(hour, 0, 0),
                        EndTime = new TimeSpan(hour, 30, 0),
                        PricePerSlot = 150000M // 150k VND for afternoon slots
                    });
                    
                    timeSlots.Add(new TimeSlot
                    {
                        StartTime = new TimeSpan(hour, 30, 0),
                        EndTime = new TimeSpan(hour + 1, 0, 0),
                        PricePerSlot = 150000M
                    });
                }
                
                // Evening prices (higher)
                for (int hour = 17; hour < 23; hour++)
                {
                    timeSlots.Add(new TimeSlot
                    {
                        StartTime = new TimeSpan(hour, 0, 0),
                        EndTime = new TimeSpan(hour, 30, 0),
                        PricePerSlot = 200000M // 200k VND for evening slots (peak hours)
                    });
                    
                    timeSlots.Add(new TimeSlot
                    {
                        StartTime = new TimeSpan(hour, 30, 0),
                        EndTime = hour == 22 ? new TimeSpan(23, 0, 0) : new TimeSpan(hour + 1, 0, 0),
                        PricePerSlot = 200000M
                    });
                }

                await context.TimeSlots.AddRangeAsync(timeSlots);
                await context.SaveChangesAsync();

                logger.LogInformation("Đã seed {Count} TimeSlots thành công.", timeSlots.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi seed TimeSlots: {Message}", ex.Message);
                throw;
            }
        }
    }
}