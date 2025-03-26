using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class User
    {
        public int UserId { get; set; } // Mã user (PK)
        public int AccountId { get; set; } // Liên kết với tài khoản (FK)
        public string FullName { get; set; } // Tên đầy đủ
        public string Email { get; set; } // Email (duy nhất)
        public string Phone { get; set; } // Số điện thoại
        public string? Gender { get; set; } // Giới tính
        public DateTime? DateOfBirth { get; set; } // Ngày sinh
        public string? AvatarUrl { get; set; } // Ảnh đại diện

        public Account Account { get; set; } // Quan hệ với bảng Account
        public ICollection<Booking> Bookings { get; set; } // Quan hệ với bảng Booking
        public ICollection<Review> Reviews { get; set; } // Quan hệ với bảng Review
        public ICollection<Notification> Notifications { get; set; } // Quan hệ với bảng Notification
        public ICollection<FavoriteField> FavoriteFields { get; set; } // Quan hệ với bảng FavoriteField
    }
}