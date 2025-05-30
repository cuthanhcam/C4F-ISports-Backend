using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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