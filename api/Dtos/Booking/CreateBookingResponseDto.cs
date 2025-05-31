using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Booking
{
    public class CreateBookingResponseDto
    {
        public int MainBookingId { get; set; }
        public List<BookingItemResponseDto> Bookings { get; set; } = new List<BookingItemResponseDto>();
        public decimal TotalPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalPrice { get; set; }
        public string PaymentUrl { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}