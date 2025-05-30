using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class ValidateAddressResponseDto
    {
        public bool IsValid { get; set; }

        [Required(ErrorMessage = "Địa chỉ định dạng là bắt buộc.")]
        [StringLength(500, ErrorMessage = "Địa chỉ định dạng không được vượt quá 500 ký tự.")]
        public string FormattedAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vĩ độ là bắt buộc.")]
        [Range(-90, 90, ErrorMessage = "Vĩ độ phải nằm trong khoảng từ -90 đến 90.")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Kinh độ là bắt buộc.")]
        [Range(-180, 180, ErrorMessage = "Kinh độ phải nằm trong khoảng từ -180 đến 180.")]
        public double Longitude { get; set; }
    }
}