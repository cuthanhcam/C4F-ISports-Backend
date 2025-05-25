using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api.Models
{
    [Index(nameof(UserId))]
    [Index(nameof(SubFieldId))]
    [Index(nameof(PromotionId))]
    public class Booking
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int SubFieldId { get; set; }
        public int? MainBookingId { get; set; }

        public DateTime BookingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal TotalPrice { get; set; }

        [StringLength(20), RegularExpression("^(Confirmed|Pending|Cancelled)$")]
        public string Status { get; set; } = "Pending";

        [StringLength(20), RegularExpression("^(Paid|Pending|Failed)$")]
        public string PaymentStatus { get; set; } = "Pending";

        [StringLength(500)]
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } // Hỗ trợ soft delete
        public bool IsReminderSent { get; set; }
        public int? PromotionId { get; set; }

        public User User { get; set; } = null!;
        public SubField SubField { get; set; } = null!;
        public Booking? MainBooking { get; set; }
        public Promotion? Promotion { get; set; }
        public ICollection<Booking> RelatedBookings { get; set; } = new List<Booking>();
        public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<BookingTimeSlot> TimeSlots { get; set; } = new List<BookingTimeSlot>();
        public ICollection<RescheduleRequest> RescheduleRequests { get; set; } = new List<RescheduleRequest>();
    }
}