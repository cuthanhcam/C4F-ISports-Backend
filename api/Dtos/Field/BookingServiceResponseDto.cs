using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class BookingServiceResponseDto
    {
        public int BookingServiceId { get; set; }

        [Required(ErrorMessage = "ID dịch vụ sân là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID dịch vụ sân phải là số nguyên dương.")]
        public int FieldServiceId { get; set; }

        [Required(ErrorMessage = "Tên dịch vụ là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên dịch vụ không được vượt quá 100 ký tự.")]
        public string ServiceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số lượng là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải là số nguyên dương.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Giá dịch vụ là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá dịch vụ phải là số không âm.")]
        public decimal Price { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }
    }
}