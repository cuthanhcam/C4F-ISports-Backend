using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class Field
    {
        public int FieldId { get; set; } // Mã sân (PK)
        public int SportId { get; set; } // Liên kết với bảng Sports (FK)
        public string FieldName { get; set; } // Tên sân
        public string Phone { get; set; } // Số điện thoại đặt lịch
        public string Address { get; set; } // Địa chỉ sân
        public string OpenHours { get; set; } // Khung giờ mở cửa
        public int OwnerId { get; set; } // Liên kết với bảng Owners (FK)
        public string Status { get; set; } // Trạng thái sân (Active, Inactive, Maintenance)
        public decimal Latitude { get; set; } // Vĩ độ
        public decimal Longitude { get; set; } // Kinh độ
        public DateTime CreatedAt { get; set; } // Ngày tạo sân
        public DateTime UpdatedAt { get; set; } // Ngày cập nhật sân

        public Sport Sport { get; set; }
        public Owner Owner { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<FieldImage> FieldImages { get; set; }
        public ICollection<FieldPricing> FieldPricings { get; set; }
        public ICollection<FieldAmenity> FieldAmenities { get; set; }
        public ICollection<FieldDescription> FieldDescriptions { get; set; }
        public ICollection<Service> Services { get; set; }
        public ICollection<FavoriteField> FavoriteFields { get; set; }
    }
}