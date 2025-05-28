using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    /// <summary>
    /// DTO chứa thông tin đặt sân.
    /// </summary>
    public class BookingDto
    {
        public int BookingId { get; set; }
        public int SubFieldId { get; set; }
        public string SubFieldName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public List<BookingTimeSlotDto> TimeSlots { get; set; } = new List<BookingTimeSlotDto>();
        public List<BookingServiceDto> Services { get; set; } = new List<BookingServiceDto>();
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}