using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Booking
{
    public class PreviewBookingResponseDto
    {
        public int SubFieldId { get; set; }
        public DateTime BookingDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal ServicePrice { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsAvailable { get; set; }
    }
}