using System.ComponentModel.DataAnnotations;

namespace api.Dtos.User
{
    public class FavoriteFieldDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "FieldId phải là số dương.")]
        public int FieldId { get; set; }

        [Required(ErrorMessage = "Tên sân là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân không được vượt quá 100 ký tự.")]
        public string FieldName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ là bắt buộc.")]
        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự.")]
        public string Address { get; set; } = string.Empty;

        [Range(0, 5, ErrorMessage = "Điểm đánh giá trung bình phải từ 0 đến 5.")]
        public decimal? AverageRating { get; set; }
    }
}