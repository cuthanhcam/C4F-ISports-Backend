using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class FieldImage
    {
        public int FieldImageId { get; set; }
        public int FieldId { get; set; }

        [Required, StringLength(500), Url]
        public string ImageUrl { get; set; }

        [StringLength(500), Url]
        public string? Thumbnail { get; set; }

        public bool IsPrimary { get; set; }
        public DateTime UploadedAt { get; set; }

        public Field Field { get; set; }
    }
}