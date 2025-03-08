using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Account
{
    public class LoginDto
    {
        public string EmailOrPhone { get; set; } // Email hoặc số điện thoại
        public string Password { get; set; } // Mật khẩu
    }
}