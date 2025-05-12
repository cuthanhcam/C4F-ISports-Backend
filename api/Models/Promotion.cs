using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class Promotion
    {
        public int PromotionId { get; set; }

        [Required, StringLength(50)]
        public string Code { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required, StringLength(20)]
        public string DiscountType { get; set; } // "Percentage", "Fixed"

        [Required]
        public decimal DiscountValue { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public decimal MinBookingValue { get; set; }
        public decimal MaxDiscountAmount { get; set; }
        public bool IsActive { get; set; }
        public int? UsageLimit { get; set; }
        public int UsageCount { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}