using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int SubFieldId { get; set; }
        public int? MainBookingId { get; set; } // Null nếu là booking chính

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        [Required, StringLength(20), RegularExpression("^(Confirmed|Pending|Cancelled)$")]
        public required string Status { get; set; } // "Confirmed", "Pending", "Cancelled"

        [Required, StringLength(20), RegularExpression("^(Paid|Pending|Failed)$")]
        public required string PaymentStatus { get; set; } // "Paid", "Pending", "Failed"

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsReminderSent { get; set; } = false;
        public int? PromotionId { get; set; }

        public required User User { get; set; }
        public required SubField SubField { get; set; }
        public Booking? MainBooking { get; set; }
        public ICollection<Booking> RelatedBookings { get; set; } = new List<Booking>();
        public Promotion? Promotion { get; set; }
        public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<BookingTimeSlot> TimeSlots { get; set; } = new List<BookingTimeSlot>();
    }
}