using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class SubField
    {
        public int SubFieldId { get; set; }

        [Required]
        public int FieldId { get; set; }

        [Required, StringLength(100)]
        public string SubFieldName { get; set; } = string.Empty;

        [Required, StringLength(50)] // "5-a-side", "7-a-side", "Badminton", ...
        public string FieldType { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Status { get; set; } = "Active";

        [Required]
        public int Capacity { get; set; }

        [StringLength(500)]
        public string? Description { get; set; } // Thêm mô tả sân con

        [Required]
        public TimeSpan OpenTime { get; set; } // Thêm khung giờ hoạt động

        [Required]
        public TimeSpan CloseTime { get; set; } // Thêm khung giờ hoạt động

        [Required]
        public decimal DefaultPricePerSlot { get; set; } // Giá mặc định cho slot 30 phút

        public int? Parent7aSideId { get; set; } // Sân 7 nếu sân 5 thuộc sân 7

        [ForeignKey("Parent7aSideId")]
        public SubField? Parent7aSide { get; set; }

        public List<int> Child5aSideIds { get; set; } = new List<int>(); // Danh sách sân 5 thuộc sân 7

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }


        [ForeignKey("FieldId")]
        public Field Field { get; set; } = null!;


        public ICollection<SubField> Child5aSides { get; set; } = new List<SubField>();
        public ICollection<PricingRule> PricingRules { get; set; } = new List<PricingRule>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<FieldPricing> PricingSchedules { get; set; } = new List<FieldPricing>();
    }
}