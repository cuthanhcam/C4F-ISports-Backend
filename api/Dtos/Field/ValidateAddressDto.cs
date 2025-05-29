using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class ValidateAddressDto
    {
        /// <summary>
        /// Tên của sân.
        /// </summary>
        [Required(ErrorMessage = "Tên sân là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân không được vượt quá 100 ký tự.")]
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ để xác thực.
        /// </summary>
        [Required(ErrorMessage = "Địa chỉ là bắt buộc.")]
        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự.")]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Thành phố của địa chỉ.
        /// </summary>
        [Required(ErrorMessage = "Thành phố là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên thành phố không được vượt quá 100 ký tự.")]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Quận/Huyện của địa chỉ.
        /// </summary>
        [Required(ErrorMessage = "Quận/Huyện là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên quận/huyện không được vượt quá 100 ký tự.")]
        public string District { get; set; } = string.Empty;
    }
}