using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api.Models
{
    [Index(nameof(SportId))]
    [Index(nameof(OwnerId))]
    [Index(nameof(City))]
    [Index(nameof(District))]
    public class Field
    {
        public int FieldId { get; set; }
        public int SportId { get; set; }
        [StringLength(100)]
        public string? FieldName { get; set; }
        [StringLength(20)]
        public string? Phone { get; set; }
        [StringLength(500)]
        public string? Address { get; set; }
        [StringLength(50)]
        public string? OpenHours { get; set; }
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public int OwnerId { get; set; }

        [StringLength(20), RegularExpression("^(Active|Inactive|Deleted)$")]
        public string Status { get; set; } = "Active";

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        [StringLength(100)]
        public string? City { get; set; }
        [StringLength(100)]
        public string? District { get; set; }
        public decimal AverageRating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } // Hỗ trợ soft delete
        public DateTime? LastCalculatedRating { get; set; } // Theo dõi thời điểm cập nhật rating

        public Sport Sport { get; set; } = null!;
        public Owner Owner { get; set; } = null!;
        public ICollection<SubField> SubFields { get; set; } = new List<SubField>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<FieldImage> FieldImages { get; set; } = new List<FieldImage>();
        public ICollection<FieldAmenity> FieldAmenities { get; set; } = new List<FieldAmenity>();
        public ICollection<FieldDescription> FieldDescriptions { get; set; } = new List<FieldDescription>();
        public ICollection<FieldService> FieldServices { get; set; } = new List<FieldService>();
        public ICollection<FavoriteField> FavoriteFields { get; set; } = new List<FavoriteField>();
        public ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
    }
}