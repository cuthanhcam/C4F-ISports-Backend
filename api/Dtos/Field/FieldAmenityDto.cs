using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Field
{
    public class FieldAmenityDto
    {
        public string AmenityName { get; set; }
        public string Description { get; set; }
    }

    public class CreateFieldAmenityDto
    {
        [Required(ErrorMessage = "Tên tiện ích không được để trống")]
        [StringLength(100, ErrorMessage = "Tên tiện ích không được vượt quá 100 ký tự")]
        public string Name { get; set; }

        [StringLength(200, ErrorMessage = "Mô tả không được vượt quá 200 ký tự")]
        public string Description { get; set; }
    }

    public class UpdateFieldAmenityDto
    {
        [Required(ErrorMessage = "Tên tiện ích không được để trống")]
        [StringLength(100, ErrorMessage = "Tên tiện ích không được vượt quá 100 ký tự")]
        public string Name { get; set; }

        [StringLength(200, ErrorMessage = "Mô tả không được vượt quá 200 ký tự")]
        public string Description { get; set; }
    }
}