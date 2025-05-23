using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class FieldImage
    {
        public int FieldImageId { get; set; }
        public int FieldId { get; set; }

        [Required, StringLength(500), Url]
        public required string ImageUrl { get; set; }

        [StringLength(500)]
        public string? PublicId { get; set; } // Added for Cloudinary

        [StringLength(500), Url]
        public string? Thumbnail { get; set; }

        public bool IsPrimary { get; set; }
        public DateTime UploadedAt { get; set; }

        public Field Field { get; set; }
    }
}