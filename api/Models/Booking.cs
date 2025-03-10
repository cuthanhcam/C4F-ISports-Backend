using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class Booking
    {
        public int BookingId { get; set; } // Mã đặt sân (PK)
        public int UserId { get; set; } // Người đặt (FK)
        public int FieldId { get; set; } // Sân được đặt (FK)
        public DateTime BookingDate { get; set; } // Ngày đặt sân
        public TimeSpan StartTime { get; set; } // Giờ bắt đầu
        public TimeSpan EndTime { get; set; } // Giờ kết thúc
        public decimal TotalPrice { get; set; } // Tổng tiền
        public string Status { get; set; } // Trạng thái (Pending, Confirmed, Canceled, Completed)
        public string PaymentStatus { get; set; } // Trạng thái thanh toán (Unpaid, Partially Paid, Paid)
        public DateTime CreatedAt { get; set; } // Ngày tạo đơn
        public DateTime UpdatedAt { get; set; } // Ngày cập nhật đơn

        public User User { get; set; } // Quan hệ với bảng User
        public Field Field { get; set; } // Quan hệ với bảng Field
    }
}