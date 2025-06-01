using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Review
{
    public class CreateReviewDto
    {
        [Required(ErrorMessage = "FieldId là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "FieldId phải là số dương.")]
        public int FieldId { get; set; }

        [Required(ErrorMessage = "BookingId là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookingId phải là số dương.")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Rating là bắt buộc.")]
        [Range(1, 5, ErrorMessage = "Rating phải từ 1 đến 5.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Comment là bắt buộc.")]
        [StringLength(1000, ErrorMessage = "Comment không được vượt quá 1000 ký tự.")]
        public string Comment { get; set; } = string.Empty;
    }
}