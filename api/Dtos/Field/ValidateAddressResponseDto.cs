using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class ValidateAddressResponseDto
    {
        /// <summary>
        /// Cho biết địa chỉ có hợp lệ hay không.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Địa chỉ được định dạng sau khi xác thực.
        /// </summary>
        [Required(ErrorMessage = "Địa chỉ định dạng là bắt buộc.")]
        [StringLength(500, ErrorMessage = "Địa chỉ định dạng không được vượt quá 500 ký tự.")]
        public string FormattedAddress { get; set; } = string.Empty;

        /// <summary>
        /// Vĩ độ của địa chỉ đã xác thực.
        /// </summary>
        [Required(ErrorMessage = "Vĩ độ là bắt buộc.")]
        [Range(-90, 90, ErrorMessage = "Vĩ độ phải nằm trong khoảng từ -90 đến 90.")]
        public double Latitude { get; set; }

        /// <summary>
        /// Kinh độ của địa chỉ đã xác thực.
        /// </summary>
        [Required(ErrorMessage = "Kinh độ là bắt buộc.")]
        [Range(-180, 180, ErrorMessage = "Kinh độ phải nằm trong khoảng từ -180 đến 180.")]
        public double Longitude { get; set; }
    }
}