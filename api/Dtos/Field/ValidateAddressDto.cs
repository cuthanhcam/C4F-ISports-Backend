using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    /// <summary>
    /// DTO để xác thực địa chỉ sân.
    /// </summary>
    public class ValidateAddressDto
    {
        [Required(ErrorMessage = "Tên sân là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân không được vượt quá 100 ký tự.")]
        public string FieldName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ là bắt buộc.")]
        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thành phố là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Thành phố không được vượt quá 100 ký tự.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quận/Huyện là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Quận/Huyện không được vượt quá 100 ký tự.")]
        public string District { get; set; } = string.Empty;
    }
}