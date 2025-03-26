using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class FieldServiceDto
    {
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Tên dịch vụ không được để trống")]
        [StringLength(100, ErrorMessage = "Tên dịch vụ không được vượt quá 100 ký tự")]
        public string ServiceName { get; set; }
        
        [Required(ErrorMessage = "Giá không được để trống")]
        [Range(0, 10000000, ErrorMessage = "Giá không hợp lệ")]
        public decimal Price { get; set; }
    }
}