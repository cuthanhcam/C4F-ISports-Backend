using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api.Models
{
    [Index(nameof(AccountId))]
    public class User
    {
        public int UserId { get; set; }
        public int AccountId { get; set; }

        [StringLength(100)] // Bỏ [Required] vì sẽ dùng DTO
        public string? FullName { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(10), RegularExpression("^(Male|Female|Other)?$", ErrorMessage = "Gender must be Male, Female, or Other")]
        public string? Gender { get; set; } // "Male", "Female", "Other"

        public DateTime? DateOfBirth { get; set; }

        [StringLength(500)]
        public string? AvatarUrl { get; set; }

        public decimal LoyaltyPoints { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } // Hỗ trợ soft delete

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? District { get; set; }

        public Account Account { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<FavoriteField> FavoriteFields { get; set; } = new List<FavoriteField>();
        public ICollection<SearchHistory> SearchHistories { get; set; } = new List<SearchHistory>();
    }
}