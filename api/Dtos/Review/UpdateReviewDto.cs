using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Review
{
    public class UpdateReviewDto
    {
        [Required(ErrorMessage = "Rating là bắt buộc.")]
        [Range(1, 5, ErrorMessage = "Rating phải từ 1 đến 5.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Comment là bắt buộc.")]
        [StringLength(1000, ErrorMessage = "Comment không được vượt quá 1000 ký tự.")]
        public string Comment { get; set; } = string.Empty;
    }
}