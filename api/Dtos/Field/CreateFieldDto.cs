using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    /// <summary>
    /// DTO để tạo sân thể thao mới.
    /// </summary>
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
        [StringLength(100, ErrorMessage = "Thành phố không được vượt quá 100 ký tự.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quận/Huyện là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Quận/Huyện không được vượt quá 100 ký tự.")]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giờ mở cửa là bắt buộc.")]
        public string OpenTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giờ đóng cửa là bắt buộc.")]
        public string CloseTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại sân là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "SportId phải là số dương.")]
        public int SportId { get; set; }

        [MaxLength(10, ErrorMessage = "Tối đa 10 sân con.")]
        public List<CreateSubFieldDto> SubFields { get; set; } = new List<CreateSubFieldDto>();

        [MaxLength(50, ErrorMessage = "Tối đa 50 dịch vụ.")]
        public List<CreateFieldServiceDto> Services { get; set; } = new List<CreateFieldServiceDto>();

        [MaxLength(50, ErrorMessage = "Tối đa 50 tiện ích.")]
        public List<CreateFieldAmenityDto> Amenities { get; set; } = new List<CreateFieldAmenityDto>();

        [MaxLength(50, ErrorMessage = "Tối đa 50 ảnh.")]
        public List<CreateFieldImageDto> Images { get; set; } = new List<CreateFieldImageDto>();
    }
}