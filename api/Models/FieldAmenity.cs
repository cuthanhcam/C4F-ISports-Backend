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
        public int FieldId { get; set; }

        [Required, StringLength(100)]
        public required string AmenityName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(500), Url]
        public string? IconUrl { get; set; }

        public required Field Field { get; set; }
    }
}