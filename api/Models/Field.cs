using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class Field
    {
        public int FieldId { get; set; }
        public int SportId { get; set; }
        public string FieldName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string OpenHours { get; set; }
        public int OwnerId { get; set; }
        public string Status { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Sport Sport { get; set; }
        public Owner Owner { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<FieldImage> FieldImages { get; set; } = new List<FieldImage>();
        public ICollection<SubField> SubFields { get; set; } = new List<SubField>();
        public ICollection<FieldAmenity> FieldAmenities { get; set; } = new List<FieldAmenity>();
        public ICollection<FieldDescription> FieldDescriptions { get; set; } = new List<FieldDescription>();
        public ICollection<FieldService> FieldServices { get; set; } = new List<FieldService>(); // Đổi tên
        public ICollection<FavoriteField> FavoriteFields { get; set; } = new List<FavoriteField>();
    }
}