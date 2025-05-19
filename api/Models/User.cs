using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class User
    {
        public int UserId { get; set; }
        public int AccountId { get; set; }

        [Required, StringLength(100)]
        public required string FullName { get; set; }

        [Required, StringLength(20)]
        public required string Phone { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; } // "Male", "Female", "Other"

        public DateTime? DateOfBirth { get; set; }

        [StringLength(500)]
        public string? AvatarUrl { get; set; }

        public decimal LoyaltyPoints { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? District { get; set; }

        public Account Account { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<FavoriteField> FavoriteFields { get; set; } = new List<FavoriteField>();
        public ICollection<SearchHistory> SearchHistories { get; set; } = new List<SearchHistory>();
    }
}