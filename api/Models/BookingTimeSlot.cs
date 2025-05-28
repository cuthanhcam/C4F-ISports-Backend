using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class BookingTimeSlot
    {
        public int BookingTimeSlotId { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        public decimal Price { get; set; } // Giá cho slot 30 phút

        public DateTime? DeletedAt { get; set; }

        [ForeignKey("BookingId")]
        public Booking Booking { get; set; } = null!;
    }
}