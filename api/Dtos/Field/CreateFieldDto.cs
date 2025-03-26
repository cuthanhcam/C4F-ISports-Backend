using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Field
{
    public class CreateFieldDto
    {
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

        [Required(ErrorMessage = "Loại thể thao không được để trống")]
        public int SportId { get; set; }

        [Required(ErrorMessage = "Vĩ độ không được để trống")]
        [Range(-90, 90, ErrorMessage = "Vĩ độ không hợp lệ")]
        public decimal Latitude { get; set; }

        [Required(ErrorMessage = "Kinh độ không được để trống")]
        [Range(-180, 180, ErrorMessage = "Kinh độ không hợp lệ")]
        public decimal Longitude { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [RegularExpression("^(Active|Inactive|Maintenance)$", ErrorMessage = "Trạng thái không hợp lệ")]
        public string Status { get; set; }

        public List<int> AmenityIds { get; set; }
        public List<CreateFieldServiceDto> Services { get; set; }
        public List<CreateFieldPricingDto> Pricing { get; set; }
    }

    public class CreateFieldServiceDto
    {
        [Required(ErrorMessage = "Tên dịch vụ không được để trống")]
        [StringLength(100, ErrorMessage = "Tên dịch vụ không được vượt quá 100 ký tự")]
        public string ServiceName { get; set; }

        [StringLength(200, ErrorMessage = "Mô tả không được vượt quá 200 ký tự")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Giá không được để trống")]
        [Range(0, 10000000, ErrorMessage = "Giá không hợp lệ")]
        public decimal Price { get; set; }
    }

    public class CreateFieldPricingDto
    {
        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc không được để trống")]
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Ngày trong tuần không được để trống")]
        [Range(0, 6, ErrorMessage = "Ngày trong tuần không hợp lệ")]
        public int DayOfWeek { get; set; }

        [Required(ErrorMessage = "Giá không được để trống")]
        [Range(0, 10000000, ErrorMessage = "Giá không hợp lệ")]
        public decimal Price { get; set; }
    }
} 