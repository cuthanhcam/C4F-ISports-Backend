using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.User
{
    public class AddFavoriteFieldDto
    {
        [Required(ErrorMessage = "FieldId là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "FieldId phải là số dương")]
        public int FieldId { get; set; }
    }
}