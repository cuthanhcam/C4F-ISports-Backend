using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class CreateFieldServiceDto
    {
        /// <summary>
        /// Tên của dịch vụ.
        /// </summary>
        [Required(ErrorMessage = "Tên dịch vụ là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên dịch vụ không được vượt quá 100 ký tự.")]
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// Giá của dịch vụ.
        /// </summary>
        [Required(ErrorMessage = "Giá dịch vụ là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá dịch vụ phải là số không âm.")]
        public decimal Price { get; set; }

        /// <summary>
        /// Mô tả của dịch vụ.
        /// </summary>
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }
    }
}