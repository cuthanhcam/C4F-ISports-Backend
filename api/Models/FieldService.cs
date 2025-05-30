using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class FieldService
    {
        public int FieldServiceId { get; set; }

        [Required]
        public int FieldId { get; set; }

        [Required, StringLength(100)]
        public string ServiceName { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        [ForeignKey("FieldId")]
        public Field Field { get; set; } = null!;
        public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
    }
}