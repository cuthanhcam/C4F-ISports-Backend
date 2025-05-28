using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Sport
{
    public class SportDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "SportId phải là số dương.")]
        public int SportId { get; set; }

        [Required(ErrorMessage = "Tên môn thể thao là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên môn thể thao không được vượt quá 100 ký tự.")]
        public string SportName { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự.")]
        public string? Description { get; set; }

        [StringLength(500, ErrorMessage = "URL ảnh không được vượt quá 500 ký tự.")]
        [Url(ErrorMessage = "URL ảnh không hợp lệ.")]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [StringLength(500, ErrorMessage = "Thông điệp không được vượt quá 500 ký tự.")]
        public string? Message { get; set; }
    }
}