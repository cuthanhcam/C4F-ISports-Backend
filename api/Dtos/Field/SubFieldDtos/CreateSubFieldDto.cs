using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field.SubFieldDtos
{
    public class CreateSubFieldDto
    {
        [Required(ErrorMessage = "Tên sân nhỏ là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân nhỏ không được vượt quá 100 ký tự.")]
        public string SubFieldName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại sân là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Loại sân không được vượt quá 50 ký tự.")]
        public string FieldType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sức chứa là bắt buộc.")]
        [Range(1, 100, ErrorMessage = "Sức chứa phải từ 1 đến 100 người.")]
        public int Capacity { get; set; }
    }
}