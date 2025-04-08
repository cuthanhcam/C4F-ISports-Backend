using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class BookingService
    {
        public int BookingServiceId { get; set; }
        public int BookingId { get; set; }
        public int FieldServiceId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public Booking Booking { get; set; }
        public FieldService FieldService { get; set; }
    }
}