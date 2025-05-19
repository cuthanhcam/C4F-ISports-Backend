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
        public int FieldId { get; set; }

        [Required, StringLength(100)]
        public required string SubFieldName { get; set; }

        [Required, StringLength(50)] // Validate in service: "5-a-side", "7-a-side", "Badminton"
        public required string FieldType { get; set; }

        [Required, StringLength(20)]
        public required string Status { get; set; } // "Active", "Inactive"

        [Required]
        public int Capacity { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public Field Field { get; set; }
        public ICollection<FieldPricing> FieldPricings { get; set; } = new List<FieldPricing>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}