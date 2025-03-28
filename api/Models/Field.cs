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
        public string OpenHours { get; set; } // Khung giờ mở cửa (VD: 8:00 - 22:00)
        public int OwnerId { get; set; } // Liên kết với bảng Owners (FK)
        public string Status { get; set; } // Trạng thái sân (Active, Inactive, Maintenance)
        public decimal Latitude { get; set; } // Vĩ độ GPS
        public decimal Longitude { get; set; } // Kinh độ GPS
        public DateTime CreatedAt { get; set; } // Ngày tạo sân
        public DateTime UpdatedAt { get; set; } // Ngày cập nhật sân

        // Các thuộc tính điều hướng (Navigation Properties)
        public Sport Sport { get; set; }
        public Owner Owner { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<FieldImage> FieldImages { get; set; } = new List<FieldImage>();
        public ICollection<FieldPricing> FieldPricings { get; set; } = new List<FieldPricing>();
        public ICollection<FieldAmenity> FieldAmenities { get; set; } = new List<FieldAmenity>();
        public ICollection<FieldDescription> FieldDescriptions { get; set; } = new List<FieldDescription>();
        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<FavoriteField> FavoriteFields { get; set; } = new List<FavoriteField>();
    }
}