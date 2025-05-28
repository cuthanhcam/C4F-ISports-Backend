using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Sport
{
    public class CreateSportDto
    {
        [Required(ErrorMessage = "Tên môn thể thao là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên môn thể thao không được vượt quá 100 ký tự.")]
        public string SportName { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự.")]
        public string? Description { get; set; }
    }
}