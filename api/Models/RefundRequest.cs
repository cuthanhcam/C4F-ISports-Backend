using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api.Models
{
    [Index(nameof(PaymentId))]
    public class RefundRequest
    {
        public int RefundRequestId { get; set; }
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }

        [StringLength(1000)]
        public string? Reason { get; set; }

        [StringLength(20), RegularExpression("^(Pending|Approved|Rejected)$")]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } // Hỗ trợ soft delete

        public Payment Payment { get; set; } = null!;
    }
}