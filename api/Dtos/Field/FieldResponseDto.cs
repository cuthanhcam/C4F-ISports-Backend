using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class FieldResponseDto
    {
        /// <summary>
        /// Mã định danh duy nhất của sân.
        /// </summary>
        public int FieldId { get; set; }

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
        /// Vĩ độ của vị trí sân.
        /// </summary>
        [Required(ErrorMessage = "Vĩ độ là bắt buộc.")]
        [Range(-90, 90, ErrorMessage = "Vĩ độ phải nằm trong khoảng từ -90 đến 90.")]
        public double Latitude { get; set; }

        /// <summary>
        /// Kinh độ của vị trí sân.
        /// </summary>
        [Required(ErrorMessage = "Kinh độ là bắt buộc.")]
        [Range(-180, 180, ErrorMessage = "Kinh độ phải nằm trong khoảng từ -180 đến 180.")]
        public double Longitude { get; set; }

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
        /// Điểm đánh giá trung bình của sân.
        /// </summary>
        [Range(0, 5, ErrorMessage = "Điểm đánh giá phải từ 0 đến 5.")]
        public decimal AverageRating { get; set; }

        /// <summary>
        /// ID môn thể thao liên quan đến sân.
        /// </summary>
        [Required(ErrorMessage = "ID môn thể thao là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID môn thể thao phải là số nguyên dương.")]
        public int SportId { get; set; }

        /// <summary>
        /// Khoảng cách từ vị trí tìm kiếm (tính bằng km).
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Khoảng cách phải là số không âm.")]
        public double? Distance { get; set; }

        /// <summary>
        /// Giá tối thiểu mỗi khung giờ trong số các sân con.
        /// </summary>
        [Required(ErrorMessage = "Giá tối thiểu mỗi khung giờ là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá tối thiểu mỗi khung giờ phải là số không âm.")]
        public decimal MinPricePerSlot { get; set; }

        /// <summary>
        /// Giá tối đa mỗi khung giờ trong số các sân con.
        /// </summary>
        [Required(ErrorMessage = "Giá tối đa mỗi khung giờ là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá tối đa mỗi khung giờ phải là số không âm.")]
        public decimal MaxPricePerSlot { get; set; }

        /// <summary>
        /// Danh sách các sân con.
        /// </summary>
        [Required(ErrorMessage = "Danh sách sân con là bắt buộc.")]
        public List<SubFieldResponseDto> SubFields { get; set; } = new();

        /// <summary>
        /// Danh sách các dịch vụ của sân.
        /// </summary>
        [Required(ErrorMessage = "Danh sách dịch vụ là bắt buộc.")]
        public List<FieldServiceResponseDto> Services { get; set; } = new();

        /// <summary>
        /// Danh sách các tiện ích có sẵn tại sân.
        /// </summary>
        [Required(ErrorMessage = "Danh sách tiện ích là bắt buộc.")]
        public List<FieldAmenityResponseDto> Amenities { get; set; } = new();

        /// <summary>
        /// Danh sách các hình ảnh liên quan đến sân.
        /// </summary>
        [Required(ErrorMessage = "Danh sách hình ảnh là bắt buộc.")]
        public List<FieldImageResponseDto> Images { get; set; } = new();
    }
}