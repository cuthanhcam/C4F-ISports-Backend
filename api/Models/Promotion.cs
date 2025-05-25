using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api.Models
{
    [Index(nameof(Code), IsUnique = true)]
    public class Promotion
    {
        public int PromotionId { get; set; }
        [StringLength(50)]
        public string? Code { get; set; }
        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(20), RegularExpression("^(Percentage|Fixed)$")]
        public string? DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinBookingValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public bool IsActive { get; set; } = true;
        public int? UsageLimit { get; set; }
        public int UsageCount { get; set; }
        public int? FieldId { get; set; } // Liên kết với sân
        public DateTime? DeletedAt { get; set; } // Hỗ trợ soft delete

        public Field? Field { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}