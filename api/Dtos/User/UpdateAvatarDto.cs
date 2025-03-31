using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.User
{
    public class UpdateAvatarDto
    {
        [Required(ErrorMessage = "Vui lòng chọn file ảnh")]
        public IFormFile AvatarFile { get; set; }
    }
}