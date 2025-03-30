using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Field
{
    public class FieldDto
    {
        public int FieldId { get; set; }
        public string FieldName { get; set; }
        public int SportId { get; set; }
        public string SportName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string OpenHours { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Status { get; set; }
        public List<string> Images { get; set; }
        public List<FieldAmenityDto> Amenities { get; set; }
        public List<FieldServiceDto> Services { get; set; }
        public List<SubFieldDto> SubFields { get; set; }
    }

    public class FieldAvailabilityDto
    {
        public int FieldId { get; set; }
        public string FieldName { get; set; }
        public string Date { get; set; }
        public string OpenHours { get; set; }
        public List<FieldServiceAvailabilityDto> Services { get; set; }
        public List<SubFieldAvailabilityDto> SubFields { get; set; }
    }

    public class FieldServiceAvailabilityDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class SubFieldAvailabilityDto
    {
        public int SubFieldId { get; set; }
        public string Name { get; set; }
        public decimal PricePerHour { get; set; }
        public List<TimeSlotDto> Slots { get; set; }
    }

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

        // [Required(ErrorMessage = "Vĩ độ không được để trống")]
        // [Range(-90, 90, ErrorMessage = "Vĩ độ không hợp lệ")]
        // public decimal Latitude { get; set; }

        // [Required(ErrorMessage = "Kinh độ không được để trống")]
        // [Range(-180, 180, ErrorMessage = "Kinh độ không hợp lệ")]
        // public decimal Longitude { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [RegularExpression("^(Active|Inactive|Maintenance)$", ErrorMessage = "Trạng thái không hợp lệ")]
        public string Status { get; set; }

        public List<CreateFieldAmenityDto> Amenities { get; set; } // Đổi từ List<string> thành List<CreateFieldAmenityDto>
        public List<CreateFieldServiceDto> Services { get; set; }
        public List<CreateSubFieldDto> SubFields { get; set; }
    }

    public class UpdateFieldDto
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

        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [RegularExpression("^(Active|Inactive|Maintenance)$", ErrorMessage = "Trạng thái không hợp lệ")]
        public string Status { get; set; }

        // [Required(ErrorMessage = "Vĩ độ không được để trống")]
        // [Range(-90, 90, ErrorMessage = "Vĩ độ không hợp lệ")]
        // public decimal Latitude { get; set; }

        // [Required(ErrorMessage = "Kinh độ không được để trống")]
        // [Range(-180, 180, ErrorMessage = "Kinh độ không hợp lệ")]
        // public decimal Longitude { get; set; }

        public List<UpdateFieldAmenityDto> Amenities { get; set; } // Đổi từ List<string> thành List<UpdateFieldAmenityDto>
        public List<UpdateFieldServiceDto> Services { get; set; }
        public List<UpdateSubFieldDto> SubFields { get; set; }
    }

    public class FieldFilterDto
    {
        public int? SportId { get; set; }
        public string? Location { get; set; }
        public string? Status { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Sort { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class FieldSearchDto
    {
        [StringLength(100, ErrorMessage = "Từ khóa tìm kiếm không được vượt quá 100 ký tự")]
        public string? SearchTerm { get; set; }

        [StringLength(100, ErrorMessage = "Địa điểm không được vượt quá 100 ký tự")]
        public string Location { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ID môn thể thao không hợp lệ")]
        public int? SportId { get; set; }

        public DateTime? Time { get; set; }

        [Range(-90, 90, ErrorMessage = "Vĩ độ phải từ -90 đến 90")]
        public decimal? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Kinh độ phải từ -180 đến 180")]
        public decimal? Longitude { get; set; }

        [Range(0.1, 100, ErrorMessage = "Bán kính phải từ 0.1 đến 100 km")]
        public decimal? Radius { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá tối thiểu không hợp lệ")]
        public decimal? MinPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá tối đa không hợp lệ")]
        public decimal? MaxPrice { get; set; }

        [RegularExpression("^(name|rating|price|distance)(:asc|:desc)?$",
            ErrorMessage = "Sắp xếp không hợp lệ. Ví dụ hợp lệ: 'name:asc', 'price:desc'")]
        public string Sort { get; set; } = "name:asc";

        [Range(1, int.MaxValue, ErrorMessage = "Số trang phải lớn hơn 0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Kích thước trang phải từ 1 đến 100")]
        public int PageSize { get; set; } = 10;
    }

    public class FieldReportDto
    {
        [Required(ErrorMessage = "Lý do báo cáo không được để trống")]
        [StringLength(50, ErrorMessage = "Lý do báo cáo không được vượt quá 50 ký tự")]
        public string Reason { get; set; }

        [Required(ErrorMessage = "Mô tả không được để trống")]
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string Description { get; set; }
    }
}