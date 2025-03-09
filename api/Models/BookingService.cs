using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class BookingService
    {
        public int BookingServiceId { get; set; }
        public int BookingId { get; set; }  // Liên kết với Booking
        public int ServiceId { get; set; }  // Liên kết với Service
        public int Quantity { get; set; }   // Số lượng dịch vụ đặt
        public decimal Price { get; set; }  // Giá tại thời điểm đặt

        public Booking Booking { get; set; }
        public Service Service { get; set; }
    }
}