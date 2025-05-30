using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class CreateFieldAmenityDto
    {
        [Required(ErrorMessage = "Tên tiện ích là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên tiện ích không được vượt quá 100 ký tự.")]
        public string AmenityName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }

        [StringLength(500, ErrorMessage = "URL biểu tượng không được vượt quá 500 ký tự.")]
        [Url(ErrorMessage = "URL biểu tượng phải là một URL hợp lệ.")]
        public string? IconUrl { get; set; }
    }
}