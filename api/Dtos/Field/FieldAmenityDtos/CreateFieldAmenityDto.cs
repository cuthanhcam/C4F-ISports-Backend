using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field.FieldAmenityDtos
{
    public class CreateFieldAmenityDto
    {
        [Required(ErrorMessage = "Tên tiện ích là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên tiện ích không được vượt quá 100 ký tự.")]
        public string AmenityName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả tiện ích không được vượt quá 500 ký tự.")]
        public string Description { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "URL biểu tượng không được vượt quá 1000 ký tự.")]
        public string IconUrl { get; set; } = string.Empty;
    }
}