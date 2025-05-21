using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field.FieldDescriptionDtos
{
    public class UpdateFieldDescriptionDto
    {
        [Required(ErrorMessage = "Nội dung mô tả là bắt buộc.")]
        [StringLength(1000, ErrorMessage = "Nội dung mô tả không được vượt quá 1000 ký tự.")]
        public string Description { get; set; } = string.Empty;
    }
}