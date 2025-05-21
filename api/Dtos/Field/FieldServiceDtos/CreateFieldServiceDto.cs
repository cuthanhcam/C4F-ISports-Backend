using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field.FieldServiceDtos
{
    public class CreateFieldServiceDto
    {
        [Required(ErrorMessage = "Tên dịch vụ là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên dịch vụ không được vượt quá 100 ký tự.")]
        public string ServiceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá dịch vụ là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá dịch vụ phải là số không âm.")]
        public decimal Price { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả dịch vụ không được vượt quá 500 ký tự.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Trạng thái dịch vụ là bắt buộc.")]
        public bool IsActive { get; set; }
    }
}