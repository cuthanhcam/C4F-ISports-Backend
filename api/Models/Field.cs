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

        [Required]
        public int SportId { get; set; }

        [Required, StringLength(100)]
        public string FieldName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; } // Thêm mô tả sân

        [Required, StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required]
        public TimeSpan OpenTime { get; set; } // Chuẩn hóa thành TimeSpan

        [Required]
        public TimeSpan CloseTime { get; set; } // Chuẩn hóa thành TimeSpan

        [Required]
        public int OwnerId { get; set; }

        [Required, StringLength(20), RegularExpression("^(Active|Inactive|Deleted)$")]
        public string Status { get; set; } = "Active";

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required, StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string District { get; set; } = string.Empty;

        public decimal AverageRating { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } // Hỗ trợ soft delete
        public DateTime? LastCalculatedRating { get; set; }

        [ForeignKey("SportId")]
        public Sport Sport { get; set; } = null!;

        [ForeignKey("OwnerId")]
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