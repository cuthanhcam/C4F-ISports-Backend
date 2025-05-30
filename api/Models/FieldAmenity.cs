using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class FieldAmenity
    {
        public int FieldAmenityId { get; set; }

        [Required]
        public int FieldId { get; set; }

        [Required, StringLength(100)]
        public string AmenityName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; } // Mô tả tiện ích

        [StringLength(500), Url]
        public string? IconUrl { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        [ForeignKey("FieldId")]
        public Field Field { get; set; } = null!;
    }
}