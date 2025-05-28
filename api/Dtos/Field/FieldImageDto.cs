using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    /// <summary>
    /// DTO chứa thông tin ảnh sân.
    /// </summary>
    public class FieldImageDto
    {
        public int FieldImageId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? PublicId { get; set; }
        public string? Thumbnail { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}