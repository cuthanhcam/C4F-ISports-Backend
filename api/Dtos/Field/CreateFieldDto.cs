using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class CreateFieldDto
    {
        /// <summary>
        /// Tên của sân.
        /// </summary>
        [Required(ErrorMessage = "Tên sân là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân không được vượt quá 100 ký tự.")]
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả của sân.
        /// </summary>
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }

        /// <summary>
        /// Địa chỉ của sân.
        /// </summary>
        [Required(ErrorMessage = "Địa chỉ là bắt buộc.")]
        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự.")]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Thành phố nơi sân tọa lạc.
        /// </summary>
        [Required(ErrorMessage = "Thành phố là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên thành phố không được vượt quá 100 ký tự.")]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Quận/Huyện nơi sân tọa lạc.
        /// </summary>
        [Required(ErrorMessage = "Quận/Huyện là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên quận/huyện không được vượt quá 100 ký tự.")]
        public string District { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian mở cửa của sân (HH:mm).
        /// </summary>
        [Required(ErrorMessage = "Thời gian mở cửa là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian mở cửa phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string OpenTime { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian đóng cửa của sân (HH:mm).
        /// </summary>
        [Required(ErrorMessage = "Thời gian đóng cửa là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian đóng cửa phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string CloseTime { get; set; } = string.Empty;

        /// <summary>
        /// ID môn thể thao liên quan đến sân.
        /// </summary>
        [Required(ErrorMessage = "ID môn thể thao là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID môn thể thao phải là số nguyên dương.")]
        public int SportId { get; set; }

        /// <summary>
        /// Danh sách các sân con cần tạo.
        /// </summary>
        [Required(ErrorMessage = "Phải có ít nhất một sân con.")]
        [MinLength(1, ErrorMessage = "Phải cung cấp ít nhất một sân con.")]
        [MaxLength(10, ErrorMessage = "Tối đa 10 sân con được phép.")]
        public List<CreateSubFieldDto> SubFields { get; set; } = new();

        /// <summary>
        /// Danh sách các dịch vụ liên kết với sân.
        /// </summary>
        [MaxLength(50, ErrorMessage = "Tối đa 50 dịch vụ được phép.")]
        public List<CreateFieldServiceDto> Services { get; set; } = new();

        /// <summary>
        /// Danh sách các tiện ích liên kết với sân.
        /// </summary>
        [MaxLength(50, ErrorMessage = "Tối đa 50 tiện ích được phép.")]
        public List<CreateFieldAmenityDto> Amenities { get; set; } = new();

        /// <summary>
        /// Danh sách các hình ảnh liên kết với sân.
        /// </summary>
        [MaxLength(20, ErrorMessage = "Tối đa 20 hình ảnh được phép.")]
        public List<CreateFieldImageDto> Images { get; set; } = new();
    }
}