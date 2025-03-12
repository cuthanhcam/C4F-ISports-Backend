using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Auth
{
    public class RegisterDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // Owner, User
        public string FullName { get; set; }
        public string Phone { get; set; }
    }
}