using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    /// <summary>
    /// DTO để tạo ảnh sân.
    /// </summary>
    public class CreateFieldImageDto
    {
        [Required(ErrorMessage = "URL ảnh là bắt buộc.")]
        [StringLength(500, ErrorMessage = "URL ảnh không được vượt quá 500 ký tự.")]
        [Url(ErrorMessage = "URL ảnh không hợp lệ.")]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "PublicId không được vượt quá 500 ký tự.")]
        public string? PublicId { get; set; }

        [StringLength(500, ErrorMessage = "Thumbnail không được vượt quá 500 ký tự.")]
        [Url(ErrorMessage = "URL thumbnail không hợp lệ.")]
        public string? Thumbnail { get; set; }

        public bool IsPrimary { get; set; }
    }
}