using System;
using System.Collections.Generic;

namespace api.Dtos.Booking
{
    public class BookingDto
    {
        public int FieldId { get; set; }
        public int BookingId { get; set; }
        public int MainBookingId { get; set; }
        public int SubFieldId { get; set; }
        public string FieldName { get; set; }
        public string SubFieldName { get; set; }
        public DateTime BookingDate { get; set; }
        public List<TimeSlotPreviewDto> TimeSlots { get; set; } // Thêm danh sách TimeSlots
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public List<BookingServiceDto> Services { get; set; }
        public List<RelatedBookingDto> RelatedBookings { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class RelatedBookingDto
    {
        public int BookingId { get; set; }
        public int SubFieldId { get; set; }
        public string SubFieldName { get; set; }
        public List<TimeSlotPreviewDto> TimeSlots { get; set; } // Thêm danh sách TimeSlots
        public string Status { get; set; }
    }
}