using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Field
{
    public class FieldDto
    {
        public int FieldId { get; set; }

        [Required(ErrorMessage = "Tên sân không được để trống")]
        [StringLength(100, ErrorMessage = "Tên sân không được vượt quá 100 ký tự")]
        public string FieldName { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(10, ErrorMessage = "Số điện thoại không được vượt quá 10 ký tự")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Giờ mở cửa không được để trống")]
        [StringLength(100, ErrorMessage = "Giờ mở cửa không được vượt quá 100 ký tự")]
        public string OpenHours { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [RegularExpression("^(Active|Inactive|Maintenance)$", ErrorMessage = "Trạng thái không hợp lệ")]
        public string Status { get; set; }

        [Required(ErrorMessage = "Loại thể thao không được để trống")]
        public int SportId { get; set; }

        [Required(ErrorMessage = "Tên loại thể thao không được để trống")]
        public string SportName { get; set; }

        [Required(ErrorMessage = "Vĩ độ không được để trống")]
        [Range(-90, 90, ErrorMessage = "Vĩ độ không hợp lệ")]
        public decimal Latitude { get; set; }

        [Required(ErrorMessage = "Kinh độ không được để trống")]
        [Range(-180, 180, ErrorMessage = "Kinh độ không hợp lệ")]
        public decimal Longitude { get; set; }

        public List<string> Images { get; set; } // Danh sách URL ảnh
        public List<FieldAmenityDto> Amenities { get; set; } // Danh sách tiện ích
        public List<FieldServiceDto> Services { get; set; } // Danh sách dịch vụ
        public List<FieldPricingDto> Pricing { get; set; } // Danh sách bảng giá
    }
}