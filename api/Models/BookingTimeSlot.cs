using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class BookingTimeSlot
    {
        public int BookingTimeSlotId { get; set; }
        public int BookingId { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        public decimal Price { get; set; }

        public Booking Booking { get; set; }
    }
}