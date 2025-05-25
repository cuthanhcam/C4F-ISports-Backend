using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api.Models
{
    [Index(nameof(BookingId))]
    [Index(nameof(TransactionId))]
    public class Payment
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(100)]
        public string? TransactionId { get; set; }

        [StringLength(20), RegularExpression("^(Success|Pending|Failed)$")]
        public string Status { get; set; } = "Pending";

        [StringLength(10)]
        public string? Currency { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? DeletedAt { get; set; } // Hỗ trợ soft delete

        public Booking Booking { get; set; } = null!;
        public ICollection<RefundRequest> RefundRequests { get; set; } = new List<RefundRequest>();
    }
}