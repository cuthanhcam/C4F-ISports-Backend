using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class Owner
    {
        public int OwnerId { get; set; } // Mã chủ sân (PK)
        public int AccountId { get; set; } // Liên kết với tài khoản (FK)
        public string FullName { get; set; } // Tên chủ sân
        public string Phone { get; set; } // Số điện thoại
        public string Email { get; set; } // Email

        public Account Account { get; set; } // Quan hệ với bảng Account
        public ICollection<Field> Fields { get; set; }
    }
}