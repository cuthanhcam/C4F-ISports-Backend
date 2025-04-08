using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }         // Liên kết với User
        public string Title { get; set; }       // Tiêu đề thông báo
        public string Content { get; set; }     // Nội dung chi tiết
        public bool IsRead { get; set; }        // Trạng thái đã đọc
        public DateTime CreatedAt { get; set; } // Ngày tạo thông báo

        public User User { get; set; }
    }
}