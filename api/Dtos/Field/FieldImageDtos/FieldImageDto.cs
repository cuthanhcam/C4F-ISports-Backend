using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field.FieldImageDtos
{
    public class FieldImageDto
    {
        public int FieldImageId { get; set; }
        public int FieldId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? PublicId { get; set; }
        public string? Thumbnail { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}