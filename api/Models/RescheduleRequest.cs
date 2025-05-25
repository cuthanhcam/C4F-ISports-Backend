using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api.Models
{
    [Index(nameof(BookingId))]
    public class RescheduleRequest
    {
        public int RescheduleRequestId { get; set; }
        public int BookingId { get; set; }
        public DateTime NewDate { get; set; }
        public TimeSpan NewStartTime { get; set; }
        public TimeSpan NewEndTime { get; set; }

        [StringLength(20), RegularExpression("^(Pending|Approved|Rejected)$")]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } // Hỗ trợ soft delete

        public Booking Booking { get; set; } = null!;
    }
}