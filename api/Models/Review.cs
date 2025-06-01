using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public int FieldId { get; set; }
        public int BookingId { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        [Required, StringLength(1000)]
        public required string Comment { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [StringLength(1000)]
        public string? OwnerReply { get; set; }

        public DateTime? ReplyDate { get; set; }
        public bool IsVisible { get; set; } = true;
        public DateTime? DeletedAt { get; set; }

        public User User { get; set; }
        public Field Field { get; set; }
        public Booking Booking { get; set; }
    }
}