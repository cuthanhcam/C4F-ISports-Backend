using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class Account
    {
        public int AccountId { get; set; }

        [Required, StringLength(256), EmailAddress]
        public required string Email { get; set; }

        [Required, StringLength(256)] // Mật khẩu băm
        public required string Password { get; set; }

        [Required, StringLength(50)]
        public required string Role { get; set; } // "Admin", "Owner", "User"

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLogin { get; set; }

        [StringLength(256)]
        public string? VerificationToken { get; set; }

        public DateTime? VerificationTokenExpiry { get; set; }

        [StringLength(256)]
        public string? ResetToken { get; set; }

        public DateTime? ResetTokenExpiry { get; set; }

        public User? User { get; set; }
        public Owner? Owner { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int SubFieldId { get; set; }
        public int? MainBookingId { get; set; } // Null nếu là booking chính

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        [Required, StringLength(20), RegularExpression("^(Confirmed|Pending|Cancelled)$")]
        public required string Status { get; set; } // "Confirmed", "Pending", "Cancelled"

        [Required, StringLength(20), RegularExpression("^(Paid|Pending|Failed)$")]
        public required string PaymentStatus { get; set; } // "Paid", "Pending", "Failed"

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsReminderSent { get; set; } = false;
        public int? PromotionId { get; set; }

        public User User { get; set; }
        public SubField SubField { get; set; }
        public Booking? MainBooking { get; set; }
        public ICollection<Booking> RelatedBookings { get; set; } = new List<Booking>();
        public Promotion? Promotion { get; set; }
        public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<BookingTimeSlot> TimeSlots { get; set; } = new List<BookingTimeSlot>();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class BookingService
    {
        public int BookingServiceId { get; set; }
        public int BookingId { get; set; }
        public int FieldServiceId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public Booking Booking { get; set; }
        public FieldService FieldService { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class BookingTimeSlot
    {
        public int BookingTimeSlotId { get; set; }
        public int BookingId { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        public decimal Price { get; set; }

        public Booking Booking { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class CloudinarySettings
    {
        public string CloudName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class FavoriteField
    {
        public int FavoriteId { get; set; }
        public int UserId { get; set; }
        public int FieldId { get; set; }

        [Required]
        public DateTime AddedDate { get; set; }

        public User User { get; set; }
        public Field Field { get; set; }
    }
}
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

        public Sport Sport { get; set; }
        public Owner Owner { get; set; }
        public ICollection<SubField> SubFields { get; set; } = new List<SubField>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<FieldImage> FieldImages { get; set; } = new List<FieldImage>();
        public ICollection<FieldAmenity> FieldAmenities { get; set; } = new List<FieldAmenity>();
        public ICollection<FieldDescription> FieldDescriptions { get; set; } = new List<FieldDescription>();
        public ICollection<FieldService> FieldServices { get; set; } = new List<FieldService>();
        public ICollection<FavoriteField> FavoriteFields { get; set; } = new List<FavoriteField>();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class FieldAmenity
    {
        public int FieldAmenityId { get; set; }
        public int FieldId { get; set; }

        [Required, StringLength(100)]
        public required string AmenityName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(500), Url]
        public string? IconUrl { get; set; }

        public Field Field { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class FieldDescription
    {
        public int FieldDescriptionId { get; set; }
        public int FieldId { get; set; }

        [Required, StringLength(2000)]
        public required string Description { get; set; }

        public Field Field { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class FieldImage
    {
        public int FieldImageId { get; set; }
        public int FieldId { get; set; }

        [Required, StringLength(500), Url]
        public required string ImageUrl { get; set; }

        [StringLength(500), Url]
        public string? Thumbnail { get; set; }

        public bool IsPrimary { get; set; }
        public DateTime UploadedAt { get; set; }

        public Field Field { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class FieldPricing
    {
        public int FieldPricingId { get; set; }
        public int SubFieldId { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required, Range(0, 6)]
        public int DayOfWeek { get; set; } // 0=Sunday, 1=Monday, ..., 6=Saturday

        [Required]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        public SubField SubField { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class FieldService
    {
        public int FieldServiceId { get; set; }
        public int FieldId { get; set; }

        [Required, StringLength(100)]
        public required string ServiceName { get; set; }

        [Required]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public Field Field { get; set; }
        public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }

        [Required, StringLength(100)]
        public required string Title { get; set; }

        [Required, StringLength(2000)]
        public required string Content { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        [StringLength(50)]
        public string? NotificationType { get; set; } // "Booking", "Promotion", "System"

        public User User { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class Owner
    {
        public int OwnerId { get; set; }
        public int AccountId { get; set; }

        [Required, StringLength(100)]
        public required string FullName { get; set; }

        [Required, StringLength(20)]
        public required string Phone { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Account Account { get; set; }
        public ICollection<Field> Fields { get; set; } = new List<Field>();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required, StringLength(50)]
        public required string PaymentMethod { get; set; } // "CreditCard", "BankTransfer", "Cash"

        [Required, StringLength(100)]
        public required string TransactionId { get; set; }

        [Required, StringLength(20)]
        public required string Status { get; set; } // "Success", "Pending", "Failed"

        [Required, StringLength(3)]
        public string Currency { get; set; } = "VND";

        public DateTime CreatedAt { get; set; }
        public DateTime? PaymentDate { get; set; }

        public Booking Booking { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class Promotion
    {
        public int PromotionId { get; set; }

        [Required, StringLength(50)]
        public required string Code { get; set; }

        [StringLength(500)]
        public required string Description { get; set; }

        [Required, StringLength(20)]
        public required string DiscountType { get; set; } // "Percentage", "Fixed"

        [Required]
        public decimal DiscountValue { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public decimal MinBookingValue { get; set; }
        public decimal MaxDiscountAmount { get; set; }
        public bool IsActive { get; set; }
        public int? UsageLimit { get; set; }
        public int UsageCount { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class RefreshToken
    {
        public int RefreshTokenId { get; set; }
        public int AccountId { get; set; }

        [Required, StringLength(256)]
        public required string Token { get; set; }

        [Required]
        public DateTime Expires { get; set; }

        [Required]
        public DateTime Created { get; set; }

        public DateTime? Revoked { get; set; }

        [StringLength(256)]
        public string? ReplacedByToken { get; set; }

        public Account Account { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public int FieldId { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        [Required, StringLength(1000)]
        public required string Comment { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [StringLength(1000)]
        public string? OwnerReply { get; set; }

        public DateTime? ReplyDate { get; set; }
        public bool IsVisible { get; set; } = true;

        public User User { get; set; }
        public Field Field { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class SearchHistory
    {
        public int SearchHistoryId { get; set; }
        public int UserId { get; set; }

        [Required, StringLength(500)]
        public required string SearchQuery { get; set; }

        [Required]
        public DateTime SearchDate { get; set; }

        public int? FieldId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public User User { get; set; }
        public Field? Field { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class Sport
    {
        public int SportId { get; set; }

        [Required, StringLength(50)]
        public required string SportName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? IconUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Field> Fields { get; set; } = new List<Field>();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class SubField
    {
        public int SubFieldId { get; set; }
        public int FieldId { get; set; }

        [Required, StringLength(100)]
        public required string SubFieldName { get; set; }

        [Required, StringLength(50)] // Validate in service: "5-a-side", "7-a-side", "Badminton"
        public required string FieldType { get; set; }

        [Required, StringLength(20)]
        public required string Status { get; set; } // "Active", "Inactive"

        [Required]
        public int Capacity { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public Field Field { get; set; }
        public ICollection<FieldPricing> FieldPricings { get; set; } = new List<FieldPricing>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
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