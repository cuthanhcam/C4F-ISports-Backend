using System;
using System.Collections.Generic;

namespace api.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int SubFieldId { get; set; }
        public int? MainBookingId { get; set; } // Null nếu là booking chính
        public DateTime BookingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User User { get; set; }
        public SubField SubField { get; set; }
        public List<BookingService> BookingServices { get; set; } = new List<BookingService>();
        public List<Booking> RelatedBookings { get; set; } // Các booking phụ
        public Booking MainBooking { get; set; } // Booking chính (nếu là booking phụ)
        public List<BookingTimeSlot> TimeSlots { get; set; } = new List<BookingTimeSlot>();
    }
}