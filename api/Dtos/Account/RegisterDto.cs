using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Account
{
    public class RegisterDto
    {
        public string Email { get; set; } // Email (bắt buộc)
        public string PhoneNumber { get; set; } // Số điện thoại (bắt buộc)
        public string Password { get; set; } // Mật khẩu (bắt buộc)
    }
}