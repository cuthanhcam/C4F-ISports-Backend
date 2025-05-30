using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class AvailabilityFilterDto
    {
        [Required(ErrorMessage = "Ngày là bắt buộc.")]
        public DateTime Date { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ID sân con phải là số nguyên dương.")]
        public int? SubFieldId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ID môn thể thao phải là số nguyên dương.")]
        public int? SportId { get; set; }

        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian bắt đầu phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string? StartTime { get; set; }

        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian kết thúc phải theo định dạng HH:mm và là bội số của 30 phút.")]
        [CustomValidation(typeof(AvailabilityFilterDto), nameof(ValidateTimeRange), ErrorMessage = "Thời gian kết thúc phải sau thời gian bắt đầu.")]
        public string? EndTime { get; set; }

        public static ValidationResult ValidateTimeRange(string endTime, ValidationContext context)
        {
            var instance = (AvailabilityFilterDto)context.ObjectInstance;
            if (!string.IsNullOrEmpty(instance.StartTime) && !string.IsNullOrEmpty(endTime) &&
                TimeSpan.TryParse(instance.StartTime, out var startTime) &&
                TimeSpan.TryParse(endTime, out var endTimeSpan))
            {
                if (endTimeSpan <= startTime)
                    return new ValidationResult("Thời gian kết thúc phải sau thời gian bắt đầu.");
            }
            return ValidationResult.Success;
        }
    }
}
namespace api.Dtos.Field
{
    public class AvailabilityResponseDto
    {
        public int SubFieldId { get; set; }

        [Required(ErrorMessage = "Tên sân con là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân con không được vượt quá 100 ký tự.")]
        public string SubFieldName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Danh sách khung giờ trống là bắt buộc.")]
        public List<AvailableSlotDto> AvailableSlots { get; set; } = new();
    }
}
namespace api.Dtos.Field
{
    public class AvailableSlotDto
    {
        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian bắt đầu phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string StartTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian kết thúc phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string EndTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá mỗi khung giờ là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá mỗi khung giờ phải là số không âm.")]
        public decimal PricePerSlot { get; set; }

        public bool IsAvailable { get; set; }

        [StringLength(200, ErrorMessage = "Lý do không trống không được vượt quá 200 ký tự.")]
        public string? UnavailableReason { get; set; }
    }
}
namespace api.Dtos.Field
{
    public class BookingFilterDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Số trang phải là số nguyên dương.")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Kích thước trang phải từ 1 đến 100.")]
        public int PageSize { get; set; } = 10;

        [StringLength(20, ErrorMessage = "Trạng thái không được vượt quá 20 ký tự.")]
        [RegularExpression("^(Pending|Confirmed|Cancelled)$", ErrorMessage = "Trạng thái phải là 'Pending', 'Confirmed' hoặc 'Cancelled'.")]
        public string? Status { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
namespace api.Dtos.Field
{
    public class BookingResponseDto
    {
        public int BookingId { get; set; }

        [Required(ErrorMessage = "ID sân con là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID sân con phải là số nguyên dương.")]
        public int SubFieldId { get; set; }

        [Required(ErrorMessage = "Tên sân con là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân con không được vượt quá 100 ký tự.")]
        public string SubFieldName { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID người dùng là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID người dùng phải là số nguyên dương.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày đặt sân là bắt buộc.")]
        public DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "Phải chỉ định ít nhất một khung giờ.")]
        [MinLength(1, ErrorMessage = "Phải chỉ định ít nhất một khung giờ.")]
        public List<BookingTimeSlotResponseDto> TimeSlots { get; set; } = new();

        [Required(ErrorMessage = "Danh sách dịch vụ là bắt buộc.")]
        public List<BookingServiceResponseDto> Services { get; set; } = new();

        [Required(ErrorMessage = "Tổng giá là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Tổng giá phải là số không âm.")]
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc.")]
        [StringLength(20, ErrorMessage = "Trạng thái không được vượt quá 20 ký tự.")]
        [RegularExpression("^(Pending|Confirmed|Cancelled)$", ErrorMessage = "Trạng thái phải là 'Pending', 'Confirmed' hoặc 'Cancelled'.")]
        public string Status { get; set; } = "Pending";

        [Required(ErrorMessage = "Trạng thái thanh toán là bắt buộc.")]
        [StringLength(20, ErrorMessage = "Trạng thái thanh toán không được vượt quá 20 ký tự.")]
        [RegularExpression("^(Pending|Paid|Failed)$", ErrorMessage = "Trạng thái thanh toán phải là 'Pending', 'Paid' hoặc 'Failed'.")]
        public string PaymentStatus { get; set; } = "Pending";

        [Required(ErrorMessage = "Ngày tạo là bắt buộc.")]
        public DateTime CreatedAt { get; set; }
    }
}
namespace api.Dtos.Field
{
    public class BookingServiceResponseDto
    {
        public int BookingServiceId { get; set; }

        [Required(ErrorMessage = "ID dịch vụ sân là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID dịch vụ sân phải là số nguyên dương.")]
        public int FieldServiceId { get; set; }

        [Required(ErrorMessage = "Tên dịch vụ là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên dịch vụ không được vượt quá 100 ký tự.")]
        public string ServiceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số lượng là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải là số nguyên dương.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Giá dịch vụ là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá dịch vụ phải là số không âm.")]
        public decimal Price { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }
    }
}
namespace api.Dtos.Field
{
    public class BookingTimeSlotResponseDto
    {
        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian bắt đầu phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string StartTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian kết thúc phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string EndTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải là số không âm.")]
        public decimal Price { get; set; }
    }
}
namespace api.Dtos.Field
{
    public class CreateFieldAmenityDto
    {
        [Required(ErrorMessage = "Tên tiện ích là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên tiện ích không được vượt quá 100 ký tự.")]
        public string AmenityName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }

        [StringLength(500, ErrorMessage = "URL biểu tượng không được vượt quá 500 ký tự.")]
        [Url(ErrorMessage = "URL biểu tượng phải là một URL hợp lệ.")]
        public string? IconUrl { get; set; }
    }
}
namespace api.Dtos.Field
{
    public class CreateFieldDto
    {
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

        [Required(ErrorMessage = "Thời gian mở cửa là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian mở cửa phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string OpenTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian đóng cửa là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian đóng cửa phải theo định dạng HH:mm và là bội số của 30 phút.")]
        [CustomValidation(typeof(CreateFieldDto), nameof(ValidateCloseTime), ErrorMessage = "Thời gian đóng cửa phải sau thời gian mở cửa.")]
        public string CloseTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID môn thể thao là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID môn thể thao phải là số nguyên dương.")]
        public int SportId { get; set; }

        [Required(ErrorMessage = "Phải có ít nhất một sân con.")]
        [MinLength(1, ErrorMessage = "Phải cung cấp ít nhất một sân con.")]
        [MaxLength(10, ErrorMessage = "Tối đa 10 sân con được phép.")]
        public List<CreateSubFieldDto> SubFields { get; set; } = new();

        [MaxLength(50, ErrorMessage = "Tối đa 50 dịch vụ được phép.")]
        public List<CreateFieldServiceDto> Services { get; set; } = new();

        [MaxLength(50, ErrorMessage = "Tối đa 50 tiện ích được phép.")]
        public List<CreateFieldAmenityDto> Amenities { get; set; } = new();

        [MaxLength(20, ErrorMessage = "Tối đa 20 hình ảnh được phép.")]
        // public List<CreateFieldImageDto> Images { get; set; } = new();
        public List<IFormFile> Images { get; set; } = new();


        public static ValidationResult ValidateCloseTime(string closeTime, ValidationContext context)
        {
            var instance = (CreateFieldDto)context.ObjectInstance;
            if (TimeSpan.TryParse(instance.OpenTime, out var openTime) && TimeSpan.TryParse(closeTime, out var closeTimeSpan))
            {
                if (closeTimeSpan <= openTime)
                    return new ValidationResult("Thời gian đóng cửa phải sau thời gian mở cửa.");
            }
            return ValidationResult.Success;
        }
    }
}
namespace api.Dtos.Field
{
    public class CreateFieldImageDto
    {
        [Required(ErrorMessage = "URL hình ảnh là bắt buộc.")]
        [StringLength(500, ErrorMessage = "URL hình ảnh không được vượt quá 500 ký tự.")]
        [Url(ErrorMessage = "URL hình ảnh phải là một URL hợp lệ.")]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "ID công khai không được vượt quá 500 ký tự.")]
        public string? PublicId { get; set; }

        [StringLength(500, ErrorMessage = "URL hình ảnh thu nhỏ không được vượt quá 500 ký tự.")]
        [Url(ErrorMessage = "URL hình ảnh thu nhỏ phải là một URL hợp lệ.")]
        public string? Thumbnail { get; set; }

        public bool IsPrimary { get; set; }
    }
}
namespace api.Dtos.Field
{
    public class CreateFieldServiceDto
    {
        [Required(ErrorMessage = "Tên dịch vụ là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên dịch vụ không được vượt quá 100 ký tự.")]
        public string ServiceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá dịch vụ là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá dịch vụ phải là số không âm.")]
        public decimal Price { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }
    }
}
namespace api.Dtos.Field
{
    public class CreatePricingRuleDto
    {
        [Required(ErrorMessage = "Phải chỉ định ít nhất một ngày.")]
        [MinLength(1, ErrorMessage = "Phải chỉ định ít nhất một ngày.")]
        public List<string> AppliesToDays { get; set; } = new();
        
        [Required(ErrorMessage = "Phải chỉ định ít nhất một khung giờ.")]
        [MinLength(1, ErrorMessage = "Phải chỉ định ít nhất một khung giờ.")]
        [MaxLength(50, ErrorMessage = "Tối đa 50 khung giờ được phép.")]
        public List<CreateTimeSlotDto> TimeSlots { get; set; } = new();
    }
}
namespace api.Dtos.Field
{
    public class CreateSubFieldDto
    {
        [Required(ErrorMessage = "Tên sân con là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân con không được vượt quá 100 ký tự.")]
        public string SubFieldName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại sân là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Loại sân không được vượt quá 50 ký tự.")]
        [RegularExpression("^(5-a-side|7-a-side|11-a-side|Badminton)$", ErrorMessage = "Loại sân phải là '5-a-side', '7-a-side', '11-a-side' hoặc 'Badminton'.")]
        public string FieldType { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Sức chứa là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Sức chứa phải là số nguyên dương.")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Thời gian mở cửa là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian mở cửa phải theo định dạng HH:mm và là bội số của 30 phút.")]
        [CustomValidation(typeof(CreateSubFieldDto), nameof(ValidateSubFieldTime), ErrorMessage = "Thời gian hoạt động của sân con phải nằm trong thời gian hoạt động của sân chính.")]
        public string OpenTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian đóng cửa là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian đóng cửa phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string CloseTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá mặc định mỗi khung giờ là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá mặc định mỗi khung giờ phải là số không âm.")]
        public decimal DefaultPricePerSlot { get; set; }

        [MaxLength(20, ErrorMessage = "Tối đa 20 quy tắc giá được phép.")]
        public List<CreatePricingRuleDto> PricingRules { get; set; } = new();

        [Range(1, int.MaxValue, ErrorMessage = "ID sân 7-a-side cha phải là số nguyên dương.")]
        public int? Parent7aSideId { get; set; }

        [MaxLength(3, ErrorMessage = "Tối đa 3 sân 5-a-side con được phép.")]
        public List<int> Child5aSideIds { get; set; } = new();

        public static ValidationResult ValidateSubFieldTime(string openTime, ValidationContext context)
        {
            var instance = (CreateSubFieldDto)context.ObjectInstance;
            var createFieldDto = context.ObjectInstance as CreateFieldDto;
            var updateFieldDto = context.ObjectInstance as UpdateFieldDto;
            
            string? fieldOpenTimeStr = createFieldDto?.OpenTime ?? updateFieldDto?.OpenTime;
            string? fieldCloseTimeStr = createFieldDto?.CloseTime ?? updateFieldDto?.CloseTime;
            
            if (fieldOpenTimeStr != null && fieldCloseTimeStr != null && 
                TimeSpan.TryParse(fieldOpenTimeStr, out var fieldOpenTime) &&
                TimeSpan.TryParse(fieldCloseTimeStr, out var fieldCloseTime) &&
                TimeSpan.TryParse(openTime, out var subFieldOpenTime) &&
                TimeSpan.TryParse(instance.CloseTime, out var subFieldCloseTime))
            {
                if (subFieldOpenTime < fieldOpenTime || subFieldCloseTime > fieldCloseTime)
                    return new ValidationResult("Thời gian hoạt động của sân con phải nằm trong thời gian hoạt động của sân chính.");
            }
            return ValidationResult.Success;
        }
    }
}
namespace api.Dtos.Field
{
    public class CreateTimeSlotDto
    {
        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian bắt đầu phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string StartTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian kết thúc phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string EndTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá mỗi khung giờ là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá mỗi khung giờ phải là số không âm.")]
        public decimal PricePerSlot { get; set; }
    }
}
namespace api.Dtos.Field
{
    public class DeleteFieldResponseDto
    {
        public int FieldId { get; set; }
        [Required(ErrorMessage = "Trạng thái là bắt buộc.")]
        public string Status { get; set; } = "Deleted";
        [Required(ErrorMessage = "Ngày xóa là bắt buộc.")]
        public DateTime DeletedAt { get; set; }
        public string Message { get; set; } = "Field deleted successfully";
    }
}
namespace api.Dtos.Field
{
    public class FieldAmenityResponseDto
    {
        public int FieldAmenityId { get; set; }

        [Required(ErrorMessage = "Tên tiện ích là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên tiện ích không được vượt quá 100 ký tự.")]
        public string AmenityName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }

        [StringLength(500, ErrorMessage = "URL biểu tượng không được vượt quá 500 ký tự.")]
        [Url(ErrorMessage = "URL biểu tượng phải là một URL hợp lệ.")]
        public string? IconUrl { get; set; }
    }
}
namespace api.Dtos.Field
{
    public class FieldFilterDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Số trang phải là số nguyên dương.")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Kích thước trang phải từ 1 đến 100.")]
        public int PageSize { get; set; } = 10;

        [StringLength(100, ErrorMessage = "Tên thành phố không được vượt quá 100 ký tự.")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "Tên quận/huyện không được vượt quá 100 ký tự.")]
        public string? District { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ID môn thể thao phải là số nguyên dương.")]
        public int? SportId { get; set; }

        [StringLength(200, ErrorMessage = "Từ khóa tìm kiếm không được vượt quá 200 ký tự.")]
        public string? Search { get; set; }

        [Range(-90, 90, ErrorMessage = "Vĩ độ phải nằm trong khoảng từ -90 đến 90.")]
        public double? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Kinh độ phải nằm trong khoảng từ -180 đến 180.")]
        public double? Longitude { get; set; }

        [Range(0, 1000, ErrorMessage = "Bán kính phải từ 0 đến 1000 km.")]
        public double Radius { get; set; } = 10;

        [Range(0, double.MaxValue, ErrorMessage = "Giá tối thiểu phải là số không âm.")]
        public decimal? MinPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá tối đa phải là số không âm.")]
        public decimal? MaxPrice { get; set; }

        [RegularExpression("^(fieldId|fieldName|averageRating|distance|price)$", ErrorMessage = "Trường sắp xếp phải là 'fieldId', 'fieldName', 'averageRating', 'distance' hoặc 'price'.")]
        public string SortBy { get; set; } = "fieldId";

        [RegularExpression("^(asc|desc)$", ErrorMessage = "Thứ tự sắp xếp phải là 'asc' hoặc 'desc'.")]
        public string SortOrder { get; set; } = "asc";
    }
}
namespace api.Dtos.Field
{
    public class FieldImageResponseDto
    {
        public int FieldImageId { get; set; }

        [Required(ErrorMessage = "URL hình ảnh là bắt buộc.")]
        [StringLength(500, ErrorMessage = "URL hình ảnh không được vượt quá 500 ký tự.")]
        [Url(ErrorMessage = "URL hình ảnh phải là một URL hợp lệ.")]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "ID công khai không được vượt quá 500 ký tự.")]
        public string? PublicId { get; set; }

        [StringLength(500, ErrorMessage = "URL hình ảnh thu nhỏ không được vượt quá 500 ký tự.")]
        [Url(ErrorMessage = "URL hình ảnh thu nhỏ phải là một URL hợp lệ.")]
        public string? Thumbnail { get; set; }

        public bool IsPrimary { get; set; }

        [Required(ErrorMessage = "Ngày tải lên là bắt buộc.")]
        public DateTime UploadedAt { get; set; }
    }
}
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
namespace api.Dtos.Field
{
    public class FieldServiceResponseDto
    {
        public int FieldServiceId { get; set; }

        [Required(ErrorMessage = "Tên dịch vụ là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên dịch vụ không được vượt quá 100 ký tự.")]
        public string ServiceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá dịch vụ là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá dịch vụ phải là số không âm.")]
        public decimal Price { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}
namespace api.Dtos.Field
{
    public class PricingRuleResponseDto
    {
        public int PricingRuleId { get; set; }

        [Required(ErrorMessage = "Phải chỉ định ít nhất một ngày.")]
        [MinLength(1, ErrorMessage = "Phải chỉ định ít nhất một ngày.")]
        public List<string> AppliesToDays { get; set; } = new();

        [Required(ErrorMessage = "Phải chỉ định ít nhất một khung giờ.")]
        [MinLength(1, ErrorMessage = "Phải chỉ định ít nhất một khung giờ.")]
        public List<TimeSlotResponseDto> TimeSlots { get; set; } = new();
    }
}
namespace api.Dtos.Field
{
    public class ReviewFilterDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Số trang phải là số nguyên dương.")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Kích thước trang phải từ 1 đến 100.")]
        public int PageSize { get; set; } = 10;

        [Range(1, 5, ErrorMessage = "Điểm đánh giá tối thiểu phải từ 1 đến 5.")]
        public int? MinRating { get; set; }

        [RegularExpression("^(createdAt|rating)$", ErrorMessage = "Trường sắp xếp phải là 'createdAt' hoặc 'rating'.")]
        public string SortBy { get; set; } = "createdAt";

        [RegularExpression("^(asc|desc)$", ErrorMessage = "Thứ tự sắp xếp phải là 'asc' hoặc 'desc'.")]
        public string SortOrder { get; set; } = "desc";
    }
}
namespace api.Dtos.Field
{
    public class ReviewResponseDto
    {
        public int ReviewId { get; set; }

        [Required(ErrorMessage = "ID người dùng là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID người dùng phải là số nguyên dương.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Điểm đánh giá là bắt buộc.")]
        [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Bình luận là bắt buộc.")]
        [StringLength(1000, ErrorMessage = "Bình luận không được vượt quá 1000 ký tự.")]
        public string Comment { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày tạo là bắt buộc.")]
        public DateTime CreatedAt { get; set; }

        [StringLength(1000, ErrorMessage = "Phản hồi của chủ sân không được vượt quá 1000 ký tự.")]
        public string? OwnerReply { get; set; }

        public DateTime? ReplyDate { get; set; }
    }
}
namespace api.Dtos.Field
{
    public class SubFieldResponseDto
    {
        public int SubFieldId { get; set; }

        [Required(ErrorMessage = "Tên sân con là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân con không được vượt quá 100 ký tự.")]
        public string SubFieldName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại sân là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Loại sân không được vượt quá 50 ký tự.")]
        public string FieldType { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc.")]
        [RegularExpression("^(Active|Inactive|Maintenance)$", ErrorMessage = "Trạng thái phải là 'Active', 'Inactive' hoặc 'Maintenance'.")]
        public string Status { get; set; } = "Active";

        [Required(ErrorMessage = "Sức chứa là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Sức chứa phải là số nguyên dương.")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Thời gian mở cửa là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian mở cửa phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string OpenTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian đóng cửa là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian đóng cửa phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string CloseTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá mặc định mỗi khung giờ là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá mặc định mỗi khung giờ phải là số không âm.")]
        public decimal DefaultPricePerSlot { get; set; }

        [Required(ErrorMessage = "Danh sách quy tắc giá là bắt buộc.")]
        public List<PricingRuleResponseDto> PricingRules { get; set; } = new();

        [Range(1, int.MaxValue, ErrorMessage = "ID sân 7-a-side cha phải là số nguyên dương.")]
        public int? Parent7aSideId { get; set; }

        [MaxLength(3, ErrorMessage = "Tối đa 3 sân 5-a-side con được phép.")]
        public List<int> Child5aSideIds { get; set; } = new();
    }
}
namespace api.Dtos.Field
{
    public class TimeSlotResponseDto
    {
        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian bắt đầu phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string StartTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian kết thúc phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string EndTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá mỗi khung giờ là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá mỗi khung giờ phải là số không âm.")]
        public decimal PricePerSlot { get; set; }
    }
}
namespace api.Dtos.Field
{
    public class UpdateFieldDto
    {
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

        [Required(ErrorMessage = "Thời gian mở cửa là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian mở cửa phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string OpenTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian đóng cửa là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian đóng cửa phải theo định dạng HH:mm và là bội số của 30 phút.")]
        [CustomValidation(typeof(UpdateFieldDto), nameof(ValidateCloseTime), ErrorMessage = "Thời gian đóng cửa phải sau thời gian mở cửa.")]
        public string CloseTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID môn thể thao là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID môn thể thao phải là số nguyên dương.")]
        public int SportId { get; set; }

        [Required(ErrorMessage = "Phải có ít nhất một sân con.")]
        [MinLength(1, ErrorMessage = "Phải cung cấp ít nhất một sân con.")]
        [MaxLength(10, ErrorMessage = "Tối đa 10 sân con được phép.")]
        public List<UpdateSubFieldDto> SubFields { get; set; } = new();

        [MaxLength(50, ErrorMessage = "Tối đa 50 dịch vụ được phép.")]
        public List<UpdateFieldServiceDto> Services { get; set; } = new();

        [MaxLength(50, ErrorMessage = "Tối đa 50 tiện ích được phép.")]
        public List<UpdateFieldAmenityDto> Amenities { get; set; } = new();

        [MaxLength(20, ErrorMessage = "Tối đa 20 hình ảnh được phép.")]
        public List<IFormFile> Images { get; set; } = new();

        public static ValidationResult ValidateCloseTime(string closeTime, ValidationContext context)
        {
            var instance = (UpdateFieldDto)context.ObjectInstance;
            if (TimeSpan.TryParse(instance.OpenTime, out var openTime) && TimeSpan.TryParse(closeTime, out var closeTimeSpan))
            {
                if (closeTimeSpan <= openTime)
                    return new ValidationResult("Thời gian đóng cửa phải sau thời gian mở cửa.");
            }
            return ValidationResult.Success;
        }
    }
}
namespace api.Dtos.Field
{
    public class UploadFieldImageDto
    {
        [Required(ErrorMessage = "Tệp hình ảnh là bắt buộc.")]
        public IFormFile Image { get; set; } = null!;
        public bool IsPrimary { get; set; }
    }
}
namespace api.Dtos.Field
{
    public class ValidateAddressDto
    {
        [Required(ErrorMessage = "Tên sân là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân không được vượt quá 100 ký tự.")]
        public string FieldName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ là bắt buộc.")]
        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thành phố là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên thành phố không được vượt quá 100 ký tự.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quận/Huyện là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên quận/huyện không được vượt quá 100 ký tự.")]
        public string District { get; set; } = string.Empty;
    }
}
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
namespace api.Dtos.Field
{
    public class UpdateFieldAmenityDto
    {
        public int? FieldAmenityId { get; set; } // ID của FieldAmenity, dùng để xác định thực thể cần cập nhật

        [Required(ErrorMessage = "Tên tiện ích là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên tiện ích không được vượt quá 100 ký tự.")]
        public string AmenityName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }

        [StringLength(500, ErrorMessage = "URL biểu tượng không được vượt quá 500 ký tự.")]
        [Url(ErrorMessage = "URL biểu tượng phải là một URL hợp lệ.")]
        public string? IconUrl { get; set; }
    }
}
namespace api.Dtos.Field
{
    public class UpdateFieldServiceDto
    {
        public int? FieldServiceId { get; set; } // ID của FieldService, dùng để xác định thực thể cần cập nhật

        [Required(ErrorMessage = "Tên dịch vụ là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên dịch vụ không được vượt quá 100 ký tự.")]
        public string ServiceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá dịch vụ là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá dịch vụ phải là số không âm.")]
        public decimal Price { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }
    }
}
namespace api.Dtos.Field
{
    public class UpdatePricingRuleDto
    {
        public int? PricingRuleId { get; set; } // ID của PricingRule, dùng để xác định thực thể cần cập nhật

        [Required(ErrorMessage = "Phải chỉ định ít nhất một ngày.")]
        [MinLength(1, ErrorMessage = "Phải chỉ định ít nhất một ngày.")]
        public List<string> AppliesToDays { get; set; } = new();

        [Required(ErrorMessage = "Phải chỉ định ít nhất một khung giờ.")]
        [MinLength(1, ErrorMessage = "Phải chỉ định ít nhất một khung giờ.")]
        [MaxLength(50, ErrorMessage = "Tối đa 50 khung giờ được phép.")]
        public List<UpdateTimeSlotDto> TimeSlots { get; set; } = new();
    }
}
namespace api.Dtos.Field
{
    public class UpdateSubFieldDto
    {
        public int? SubFieldId { get; set; } // ID của SubField, dùng để xác định thực thể cần cập nhật

        [Required(ErrorMessage = "Tên sân con là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân con không được vượt quá 100 ký tự.")]
        public string SubFieldName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại sân là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Loại sân không được vượt quá 50 ký tự.")]
        [RegularExpression("^(5-a-side|7-a-side|11-a-side|Badminton)$", ErrorMessage = "Loại sân phải là '5-a-side', '7-a-side', '11-a-side' hoặc 'Badminton'.")]
        public string FieldType { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Sức chứa là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Sức chứa phải là số nguyên dương.")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Thời gian mở cửa là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian mở cửa phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string OpenTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian đóng cửa là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian đóng cửa phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string CloseTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá mặc định mỗi khung giờ là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá mặc định mỗi khung giờ phải là số không âm.")]
        public decimal DefaultPricePerSlot { get; set; }

        [MaxLength(20, ErrorMessage = "Tối đa 20 quy tắc giá được phép.")]
        public List<UpdatePricingRuleDto> PricingRules { get; set; } = new();

        [Range(1, int.MaxValue, ErrorMessage = "ID sân 7-a-side cha phải là số nguyên dương.")]
        public int? Parent7aSideId { get; set; }

        [MaxLength(3, ErrorMessage = "Tối đa 3 sân 5-a-side con được phép.")]
        public List<int> Child5aSideIds { get; set; } = new();
    }
}
namespace api.Dtos.Field
{
    public class UpdateTimeSlotDto
    {
        public int? TimeSlotId { get; set; } // ID của TimeSlot, dùng để xác định thực thể cần cập nhật

        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian bắt đầu phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string StartTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian kết thúc phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string EndTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá mỗi khung giờ là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá mỗi khung giờ phải là số không âm.")]
        public decimal PricePerSlot { get; set; }
    }
}