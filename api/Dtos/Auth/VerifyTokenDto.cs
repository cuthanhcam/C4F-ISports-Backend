using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Auth
{
    public class VerifyTokenDto
    {
        [Required(ErrorMessage = "Token is required")]
        public required string Token { get; set; }
    }
}