using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Auth
{
    public class RestoreAccountDto
    {
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Token là bắt buộc.")]
        public required string Token { get; set; }
    }
}