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
        [StringLength(50, ErrorMessage = "Tên tiện ích không được vượt quá 50 ký tự")]
        public string AmenityName { get; set; }

        [StringLength(50, ErrorMessage = "Icon không được vượt quá 50 ký tự")]
        public string Icon { get; set; }
    }
}