using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class Account
    {
        public int AccountId { get; set; }

        [Required, StringLength(100)]
        public required string Email { get; set; }

        [Required, StringLength(256)] // Lưu password đã băm
        public required string Password { get; set; }

        [Required, StringLength(20)]
        public required string Role { get; set; } // "User" or "Owner"

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLogin { get; set; }

        [StringLength(256)]
        public string? VerificationToken { get; set; }

        public DateTime? VerificationTokenExpiry { get; set; }

        [StringLength(256)]
        public string? ResetToken { get; set; }

        public DateTime? ResetTokenExpiry { get; set; }
        public DateTime? DeletedAt { get; set; } // Hỗ trợ soft delete

        public User? User { get; set; }
        public Owner? Owner { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
namespace api.Models
{
    [Index(nameof(UserId))]
    [Index(nameof(SubFieldId))]
    [Index(nameof(PromotionId))]
    public class Booking
    {
        public int BookingId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int SubFieldId { get; set; }

        public int? MainBookingId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        public List<BookingTimeSlot> TimeSlots { get; set; } = new List<BookingTimeSlot>(); // Các slot 30 phút

        [Required]
        public decimal TotalPrice { get; set; }

        [Required, StringLength(20), RegularExpression("^(Confirmed|Pending|Cancelled)$")]
        public string Status { get; set; } = "Pending";

        [Required, StringLength(20), RegularExpression("^(Paid|Pending|Failed)$")]
        public string PaymentStatus { get; set; } = "Pending";

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } // Hỗ trợ soft delete
        public bool IsReminderSent { get; set; }
        public int? PromotionId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("SubFieldId")]
        public SubField SubField { get; set; } = null!;

        [ForeignKey("MainBookingId")]
        public Booking? MainBooking { get; set; }

        [ForeignKey("PromotionId")]
        public Promotion? Promotion { get; set; }

        public ICollection<Booking> RelatedBookings { get; set; } = new List<Booking>();
        public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<RescheduleRequest> RescheduleRequests { get; set; } = new List<RescheduleRequest>();
        
    }
}
namespace api.Models
{
    public class BookingService
    {
        public int BookingServiceId { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        public int FieldServiceId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [ForeignKey("BookingId")]
        public Booking Booking { get; set; } = null!;

        [ForeignKey("FieldServiceId")]
        public FieldService FieldService { get; set; } = null!;
    }
}
namespace api.Models
{
    public class BookingTimeSlot
    {
        public int BookingTimeSlotId { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        public decimal Price { get; set; } // Giá cho slot 30 phút

        public DateTime? DeletedAt { get; set; }

        [ForeignKey("BookingId")]
        public Booking Booking { get; set; } = null!;
    }
}
namespace api.Models
{
    public class CloudinarySettings
    {
        public string CloudName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
    }
}
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
namespace api.Models
{
    public class FieldAmenity
    {
        public int FieldAmenityId { get; set; }

        [Required]
        public int FieldId { get; set; }

        [Required, StringLength(100)]
        public string AmenityName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; } // Mô tả tiện ích

        [StringLength(500), Url]
        public string? IconUrl { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        [ForeignKey("FieldId")]
        public Field Field { get; set; } = null!;
    }
}
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
namespace api.Models
{
    public class FieldImage
    {
        public int FieldImageId { get; set; }

        [Required]
        public int FieldId { get; set; }

        [Required, StringLength(500), Url]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(500)]
        public string? PublicId { get; set; } // For Cloudinary

        [StringLength(500), Url]
        public string? Thumbnail { get; set; }

        public bool IsPrimary { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }

        [ForeignKey("FieldId")]
        public Field Field { get; set; } = null!;
    }
}
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
        public DateTime? DeletedAt { get; set; } // Hỗ trợ soft delete

        [Required, Range(0, 6)]
        public int DayOfWeek { get; set; } // 0=Sunday, 1=Monday, ..., 6=Saturday

        [Required]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        public SubField SubField { get; set; } = null;
    }
}
namespace api.Models
{
    public class FieldService
    {
        public int FieldServiceId { get; set; }

        [Required]
        public int FieldId { get; set; }

        [Required, StringLength(100)]
        public string ServiceName { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        [ForeignKey("FieldId")]
        public Field Field { get; set; } = null!;
        public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
    }
}
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
        public DateTime? DeletedAt { get; set; }

        public Account Account { get; set; }
        public ICollection<Field> Fields { get; set; } = new List<Field>();
    }
}
namespace api.Models
{
    [Index(nameof(BookingId))]
    [Index(nameof(TransactionId))]
    public class Payment
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(100)]
        public string? TransactionId { get; set; }

        [StringLength(20), RegularExpression("^(Success|Pending|Failed)$")]
        public string Status { get; set; } = "Pending";

        [StringLength(10)]
        public string? Currency { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? DeletedAt { get; set; } // Hỗ trợ soft delete

        public Booking Booking { get; set; } = null!;
        public ICollection<RefundRequest> RefundRequests { get; set; } = new List<RefundRequest>();
    }
}
namespace api.Models
{
    public class PricingRule
    {
        public int PricingRuleId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        [Required]
        public int SubFieldId { get; set; }

        [ForeignKey("SubFieldId")]
        public SubField SubField { get; set; } = null!;

        [Required]
        public List<string> AppliesToDays { get; set; } = new List<string>(); // Monday, Tuesday, ...
        
        public ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
    }
}
namespace api.Models
{
    [Index(nameof(Code), IsUnique = true)]
    public class Promotion
    {
        public int PromotionId { get; set; }
        [StringLength(50)]
        public string? Code { get; set; }
        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(20), RegularExpression("^(Percentage|Fixed)$")]
        public string? DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinBookingValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public bool IsActive { get; set; } = true;
        public int? UsageLimit { get; set; }
        public int UsageCount { get; set; }
        public int? FieldId { get; set; } // Liên kết với sân
        public DateTime? DeletedAt { get; set; } // Hỗ trợ soft delete

        public Field? Field { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
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
namespace api.Models
{
    [Index(nameof(PaymentId))]
    public class RefundRequest
    {
        public int RefundRequestId { get; set; }
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }

        [StringLength(1000)]
        public string? Reason { get; set; }

        [StringLength(20), RegularExpression("^(Pending|Approved|Rejected)$")]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } // Hỗ trợ soft delete

        public Payment Payment { get; set; } = null!;
    }
}
namespace api.Models
{
    [Index(nameof(BookingId))]
    public class RescheduleRequest
    {
        public int RescheduleRequestId { get; set; }
        public int BookingId { get; set; }
        public DateTime NewDate { get; set; }
        public TimeSpan NewStartTime { get; set; }
        public TimeSpan NewEndTime { get; set; }

        [StringLength(20), RegularExpression("^(Pending|Approved|Rejected)$")]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } // Hỗ trợ soft delete

        public Booking Booking { get; set; } = null!;
    }
}
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
namespace api.Models
{
    [Index(nameof(UserId))]
    [Index(nameof(SearchDateTime))]
    public class SearchHistory
    {
        [Key]
        public int SearchId { get; set; } 

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(500)]
        public string Keyword { get; set; }

        [Required]
        public DateTime SearchDateTime { get; set; }

        public int? FieldId { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public DateTime? DeletedAt { get; set; }

        [ForeignKey("UserId")]
        public User Account { get; set; }

        [ForeignKey("FieldId")]
        public Field? Field { get; set; }

        public SearchHistory()
        {
            SearchDateTime = DateTime.UtcNow;
        }
    }
}
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
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public ICollection<Field> Fields { get; set; } = new List<Field>();
    }
}
namespace api.Models
{
    public class SubField
    {
        public int SubFieldId { get; set; }

        [Required]
        public int FieldId { get; set; }

        [Required, StringLength(100)]
        public string SubFieldName { get; set; } = string.Empty;

        [Required, StringLength(50)] // "5-a-side", "7-a-side", "Badminton", ...
        public string FieldType { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Status { get; set; } = "Active";

        [Required]
        public int Capacity { get; set; }

        [StringLength(500)]
        public string? Description { get; set; } // Thêm mô tả sân con

        [Required]
        public TimeSpan OpenTime { get; set; } // Thêm khung giờ hoạt động

        [Required]
        public TimeSpan CloseTime { get; set; } // Thêm khung giờ hoạt động

        [Required]
        public decimal DefaultPricePerSlot { get; set; } // Giá mặc định cho slot 30 phút

        public int? Parent7aSideId { get; set; } // Sân 7 nếu sân 5 thuộc sân 7

        [ForeignKey("Parent7aSideId")]
        public SubField? Parent7aSide { get; set; }

        public List<int> Child5aSideIds { get; set; } = new List<int>(); // Danh sách sân 5 thuộc sân 7

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }


        [ForeignKey("FieldId")]
        public Field Field { get; set; } = null!;


        public ICollection<SubField> Child5aSides { get; set; } = new List<SubField>();
        public ICollection<PricingRule> PricingRules { get; set; } = new List<PricingRule>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<FieldPricing> PricingSchedules { get; set; } = new List<FieldPricing>();
    }
}
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