using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; }

        [Required, StringLength(2000)]
        public string Content { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        [StringLength(50)]
        public string? NotificationType { get; set; } // "Booking", "Promotion", "System"

        public User User { get; set; }
    }
}