using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Field
{
    public class SubFieldDto
    {
        public int SubFieldId { get; set; }
        public string Name { get; set; }
        public string Size { get; set; } // Giữ Size để đồng bộ với model
        public decimal PricePerHour { get; set; }
        public string Status { get; set; }
    }

    public class CreateSubFieldDto
    {
        [Required(ErrorMessage = "Tên sân nhỏ không được để trống")]
        [StringLength(50, ErrorMessage = "Tên sân nhỏ không vượt quá 50 ký tự")]
        public string SubFieldName { get; set; }

        [Required(ErrorMessage = "Kích thước không được để trống")]
        [StringLength(20, ErrorMessage = "Kích thước không vượt quá 20 ký tự")]
        public string Size { get; set; }

        [Required(ErrorMessage = "Giá mỗi giờ không được để trống")]
        [Range(0, 10000000, ErrorMessage = "Giá không hợp lệ")]
        public decimal PricePerHour { get; set; }
    }

    public class UpdateSubFieldDto
    {
        [Required(ErrorMessage = "Tên sân nhỏ không được để trống")]
        [StringLength(50, ErrorMessage = "Tên sân nhỏ không vượt quá 50 ký tự")]
        public string SubFieldName { get; set; }

        [Required(ErrorMessage = "Kích thước không được để trống")]
        [StringLength(20, ErrorMessage = "Kích thước không vượt quá 20 ký tự")]
        public string Size { get; set; }

        [Required(ErrorMessage = "Giá mỗi giờ không được để trống")]
        [Range(0, 10000000, ErrorMessage = "Giá không hợp lệ")]
        public decimal PricePerHour { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [RegularExpression("^(Active|Inactive|Maintenance)$", ErrorMessage = "Trạng thái không hợp lệ")]
        public string Status { get; set; }
    }
}