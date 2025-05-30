using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class FieldResponseDto
    {
        public int FieldId { get; set; }

        [Required(ErrorMessage = "Tên sân là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân không được vượt quá 100 ký tự.")]
        public string FieldName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc.")]
        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thành phố là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên thành phố không được vượt quá 100 ký tự.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quận/Huyện là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên quận/huyện không được vượt quá 100 ký tự.")]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vĩ độ là bắt buộc.")]
        [Range(-90, 90, ErrorMessage = "Vĩ độ phải nằm trong khoảng từ -90 đến 90.")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Kinh độ là bắt buộc.")]
        [Range(-180, 180, ErrorMessage = "Kinh độ phải nằm trong khoảng từ -180 đến 180.")]
        public double Longitude { get; set; }

        [Required(ErrorMessage = "Thời gian mở cửa là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian mở cửa phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string OpenTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian đóng cửa là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian đóng cửa phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string CloseTime { get; set; } = string.Empty;

        [Range(0, 5, ErrorMessage = "Điểm đánh giá phải từ 0 đến 5.")]
        public decimal AverageRating { get; set; }

        [Required(ErrorMessage = "ID môn thể thao là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID môn thể thao phải là số nguyên dương.")]
        public int SportId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Khoảng cách phải là số không âm.")]
        public double? Distance { get; set; }

        [Required(ErrorMessage = "Giá tối thiểu mỗi khung giờ là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá tối thiểu mỗi khung giờ phải là số không âm.")]
        public decimal MinPricePerSlot { get; set; }

        [Required(ErrorMessage = "Giá tối đa mỗi khung giờ là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá tối đa mỗi khung giờ phải là số không âm.")]
        public decimal MaxPricePerSlot { get; set; }

        [Required(ErrorMessage = "Danh sách sân con là bắt buộc.")]
        public List<SubFieldResponseDto> SubFields { get; set; } = new();

        [Required(ErrorMessage = "Danh sách dịch vụ là bắt buộc.")]
        public List<FieldServiceResponseDto> Services { get; set; } = new();

        [Required(ErrorMessage = "Danh sách tiện ích là bắt buộc.")]
        public List<FieldAmenityResponseDto> Amenities { get; set; } = new();

        [Required(ErrorMessage = "Danh sách hình ảnh là bắt buộc.")]
        public List<FieldImageResponseDto> Images { get; set; } = new();
    }
}