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
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal Price { get; set; }

        public Booking Booking { get; set; }
    }
}