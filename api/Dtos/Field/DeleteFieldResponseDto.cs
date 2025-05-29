using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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