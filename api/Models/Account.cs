using System;
using System.Collections.Generic;

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
        public string? ResetToken { get; set; } // Token đặt lại mật khẩu
        public DateTime? ResetTokenExpiry { get; set; } // Thời gian hết hạn của reset token
        public string? VerificationToken { get; set; } // Token xác thực email
        public DateTime? VerificationTokenExpiry { get; set; } // Thời gian hết hạn của verification token

        public User? User { get; set; } // Quan hệ 1-1 với User
        public Owner? Owner { get; set; } // Quan hệ 1-1 với Owner
        public ICollection<RefreshToken> RefreshTokens { get; set; } // Quan hệ 1-nhiều với RefreshToken
    }
}