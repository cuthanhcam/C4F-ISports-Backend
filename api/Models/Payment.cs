using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }      // Liên kết với Booking
        public decimal Amount { get; set; }     // Số tiền thanh toán
        public string PaymentMethod { get; set; }  // Phương thức thanh toán
        public string TransactionId { get; set; }  // Mã giao dịch thanh toán
        public string Status { get; set; }         // Trạng thái (Success, Failed, Pending)
        public DateTime CreatedAt { get; set; }    // Ngày tạo giao dịch

        public Booking Booking { get; set; }
    }
}