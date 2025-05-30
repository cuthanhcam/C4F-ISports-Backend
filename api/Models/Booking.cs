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

        [Required]
        public int UserId { get; set; }

        [Required]
        public int SubFieldId { get; set; }

        public int? MainBookingId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        public List<BookingTimeSlot> TimeSlots { get; set; } = new List<BookingTimeSlot>(); // Các slot 30 phút

        [Required]
        public decimal TotalPrice { get; set; }

        [Required, StringLength(20), RegularExpression("^(Confirmed|Pending|Cancelled)$")]
        public string Status { get; set; } = "Pending";

        [Required, StringLength(20), RegularExpression("^(Paid|Pending|Failed)$")]
        public string PaymentStatus { get; set; } = "Pending";

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } // Hỗ trợ soft delete
        public bool IsReminderSent { get; set; }
        public int? PromotionId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("SubFieldId")]
        public SubField SubField { get; set; } = null!;

        [ForeignKey("MainBookingId")]
        public Booking? MainBooking { get; set; }

        [ForeignKey("PromotionId")]
        public Promotion? Promotion { get; set; }

        public ICollection<Booking> RelatedBookings { get; set; } = new List<Booking>();
        public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<RescheduleRequest> RescheduleRequests { get; set; } = new List<RescheduleRequest>();
        
    }
}