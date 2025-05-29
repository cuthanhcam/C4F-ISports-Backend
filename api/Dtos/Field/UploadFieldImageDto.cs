using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class UploadFieldImageDto
    {
        [Required(ErrorMessage = "Tệp hình ảnh là bắt buộc.")]
        public IFormFile Image { get; set; } = null!;
        public bool IsPrimary { get; set; }
    }
}