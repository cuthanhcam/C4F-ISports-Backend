using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class Field
    {
        public int FieldId { get; set; }
        public int SportId { get; set; }

        [Required, StringLength(100)]
        public required string FieldName { get; set; }

        [Required, StringLength(20)]
        public required string Phone { get; set; }

        [Required, StringLength(500)]
        public required string Address { get; set; }

        [Required, StringLength(100)] // Format: "HH:mm-HH:mm"
        public required string OpenHours { get; set; }

		// Nullable, used for precise time validation in backend
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }

        public int OwnerId { get; set; }

        [Required, StringLength(20)]
        public required string Status { get; set; } // "Active", "Inactive", "Maintenance"

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? District { get; set; }

        [Range(0, 5)]
        public decimal? AverageRating { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public required Sport Sport { get; set; }
        public required Owner Owner { get; set; }
        public ICollection<SubField> SubFields { get; set; } = new List<SubField>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<FieldImage> FieldImages { get; set; } = new List<FieldImage>();
        public ICollection<FieldAmenity> FieldAmenities { get; set; } = new List<FieldAmenity>();
        public ICollection<FieldDescription> FieldDescriptions { get; set; } = new List<FieldDescription>();
        public ICollection<FieldService> FieldServices { get; set; } = new List<FieldService>();
        public ICollection<FavoriteField> FavoriteFields { get; set; } = new List<FavoriteField>();
    }
}