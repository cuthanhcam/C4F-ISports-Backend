using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    /// <summary>
    /// DTO chứa thông tin khung giờ trống.
    /// </summary>
    public class TimeSlotAvailabilityDto
    {
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public decimal PricePerSlot { get; set; }
        public bool IsAvailable { get; set; }
        public string? UnavailableReason { get; set; }
    }
}