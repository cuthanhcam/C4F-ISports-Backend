using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class FieldAmenityDto
    {
        public int AmenityId { get; set; }

        [Required(ErrorMessage = "Tên tiện ích không được để trống")]
        [StringLength(100, ErrorMessage = "Tên tiện ích không được vượt quá 100 ký tự")]
        public string AmenityName { get; set; }
    }
}