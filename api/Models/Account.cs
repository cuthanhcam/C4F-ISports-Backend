using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class Account
    {
        public int AccountId { get; set; } // Mã tài khoản (PK)
        public string Email { get; set; } // Email đăng nhập (UNIQUE)
        public string Password { get; set; } // Mật khẩu (hash)
        public string Role { get; set; } // Vai trò (User, Owner, Admin)
        public bool IsActive { get; set; } // Trạng thái tài khoản
        public DateTime CreatedAt { get; set; } // Ngày tạo tài khoản
        public DateTime? LastLogin { get; set; } // Lần đăng nhập cuối
        public string? RefreshToken { get; set; } // Token làm mới
        public DateTime? RefreshTokenExpiry { get; set; } // Thời gian hết hạn của token
        public string? ResetToken { get; set; } // Token đặt lại mật khẩu
        public DateTime? ResetTokenExpiry { get; set; } // Thời gian hết hạn của token

        public User? User { get; set; } // Quan hệ 1-1 với User (nếu là khách hàng)
        public Owner? Owner { get; set; } // Quan hệ 1-1 với Owner (nếu là chủ sân)
    }
}