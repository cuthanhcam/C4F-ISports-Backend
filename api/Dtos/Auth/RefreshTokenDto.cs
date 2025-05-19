using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Auth
{
    public class RefreshTokenDto
    {   
        [Required(ErrorMessage = "Refresh token là bắt buộc.")]
        public required string RefreshToken { get; set; }
    }
}