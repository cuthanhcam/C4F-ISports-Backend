using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }

        [Required, StringLength(100)]
        public required string Title { get; set; }

        [Required, StringLength(2000)]
        public required string Content { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        [StringLength(50)]
        public string? NotificationType { get; set; } // "Booking", "Promotion", "System"

        public required User User { get; set; }
    }
}