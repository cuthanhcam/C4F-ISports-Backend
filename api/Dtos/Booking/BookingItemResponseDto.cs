using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Booking
{
    public class BookingItemResponseDto
    {
        public int BookingId { get; set; }
        public int SubFieldId { get; set; }
        public string SubFieldName { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public List<TimeSlotResponseDto> TimeSlots { get; set; } = new List<TimeSlotResponseDto>();
        public List<BookingServiceResponseDto> Services { get; set; } = new List<BookingServiceResponseDto>();
        public decimal SubTotal { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
    }
}