using System;
using System.Collections.Generic;

namespace api.Dtos.Booking
{
    public class BookingDto
    {
        public int BookingId { get; set; }
        public int SubFieldId { get; set; }
        public string FieldName { get; set; }
        public string SubFieldName { get; set; }
        public DateTime BookingDate { get; set; }
        public string StartTime { get; set; } // hh:mm
        public string EndTime { get; set; }   // hh:mm
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }    // Pending, Confirmed, Canceled, Completed
        public string PaymentStatus { get; set; } // Unpaid, Paid
        public List<BookingServiceDto> Services { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}