using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Field
{
    public class FieldServiceDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
    }

    public class CreateFieldServiceDto
    {
        [Required(ErrorMessage = "Tên dịch vụ không được để trống")]
        [StringLength(100, ErrorMessage = "Tên dịch vụ không được vượt quá 100 ký tự")]
        public string ServiceName { get; set; }

        [Required(ErrorMessage = "Giá không được để trống")]
        [Range(0, 10000000, ErrorMessage = "Giá không hợp lệ")]
        public decimal Price { get; set; }

        [StringLength(200, ErrorMessage = "Mô tả không được vượt quá 200 ký tự")]
        public string Description { get; set; }
    }

    public class UpdateFieldServiceDto
    {
        [Required(ErrorMessage = "Tên dịch vụ không được để trống")]
        [StringLength(100, ErrorMessage = "Tên dịch vụ không được vượt quá 100 ký tự")]
        public string ServiceName { get; set; } // Đổi Name thành ServiceName để đồng bộ với CreateFieldServiceDto

        [Required(ErrorMessage = "Giá không được để trống")]
        [Range(0, 10000000, ErrorMessage = "Giá không hợp lệ")]
        public decimal Price { get; set; }

        [StringLength(200, ErrorMessage = "Mô tả không được vượt quá 200 ký tự")]
        public string Description { get; set; }
    }
}