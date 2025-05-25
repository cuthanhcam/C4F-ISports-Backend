using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Models
{
    public class User
    {
        public int UserId { get; set; }
        public int AccountId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string AvatarUrl { get; set; }
        public string Gender { get; set; } // "Male", "Female", "Other"
        public DateTime? DateOfBirth { get; set; }
        public int LoyaltyPoints { get; set; } // For loyalty program
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Account Account { get; set; }
        public List<Booking> Bookings { get; set; }
        public List<FavoriteField> FavoriteFields { get; set; }
        public List<Review> Reviews { get; set; }
        public List<Notification> Notifications { get; set; }
        public List<SearchHistory> SearchHistories { get; set; }
    }

    public class Booking
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int SubFieldId { get; set; }
        public int? MainBookingId { get; set; } // For complex bookings
        public DateTime BookingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } // "Pending", "Confirmed", "Cancelled"
        public string PaymentStatus { get; set; } // "Pending", "Paid", "Refunded"
        public string Notes { get; set; }
        public int? PromotionId { get; set; }
        public bool IsReminderSent { get; set; } // For email reminder BackgroundService
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public User User { get; set; }
        public SubField SubField { get; set; }
        public Promotion Promotion { get; set; }
        public List<BookingTimeSlot> TimeSlots { get; set; }
        public List<BookingService> BookingServices { get; set; }
        public List<Payment> Payments { get; set; }
    }

    public class Owner
    {
        public int OwnerId { get; set; }
        public int AccountId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Account Account { get; set; }
        public List<Field> Fields { get; set; }
    }

    public class Field
    {
        public int FieldId { get; set; }
        public int OwnerId { get; set; }
        public int SportId { get; set; }
        public string FieldName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public double AverageRating { get; set; }
        public string Status { get; set; } // "Active", "Inactive", "Pending"
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Owner Owner { get; set; }
        public Sport Sport { get; set; }
        public List<SubField> SubFields { get; set; }
        public List<FieldPricing> Pricing { get; set; }
        public List<FieldImage> Images { get; set; }
        public List<FieldService> Services { get; set; }
        public List<FieldAmenity> Amenities { get; set; }
        public List<FieldDescription> Descriptions { get; set; }
        public List<Review> Reviews { get; set; }
        public List<FavoriteField> FavoriteFields { get; set; }
    }

    public class SubField
    {
        public int SubFieldId { get; set; }
        public int FieldId { get; set; }
        public string SubFieldName { get; set; }
        public string Type { get; set; } // "5-a-side", "7-a-side", etc.
        public string Status { get; set; } // "Active", "Inactive"
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Field Field { get; set; }
        public List<Booking> Bookings { get; set; }
        public List<FieldPricing> Pricing { get; set; }
    }

    public class Booking
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int SubFieldId { get; set; }
        public int? MainBookingId { get; set; } // For complex bookings
        public DateTime BookingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } // "Pending", "Confirmed", "Cancelled"
        public string PaymentStatus { get; set; } // "Pending", "Paid", "Refunded"
        public string Notes { get; set; }
        public int? PromotionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public User User { get; set; }
        public SubField SubField { get; set; }
        public Promotion Promotion { get; set; }
        public List<BookingTimeSlot> TimeSlots { get; set; }
        public List<BookingService> BookingServices { get; set; }
        public List<Payment> Payments { get; set; }
    }

    public class BookingTimeSlot
    {
        public int BookingTimeSlotId { get; set; }
        public int BookingId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal Price { get; set; }
        public Booking Booking { get; set; }
    }

    public class BookingService
    {
        public int BookingServiceId { get; set; }
        public int BookingId { get; set; }
        public int FieldServiceId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public Booking Booking { get; set; }
        public FieldService FieldService { get; set; }
    }

    public class FieldPricing
    {
        public int FieldPricingId { get; set; }
        public int FieldId { get; set; }
        public int? SubFieldId { get; set; }
        public string PricingType { get; set; } // "Hourly", "Fixed"
        public decimal Price { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string DayOfWeek { get; set; }
        public Field Field { get; set; }
        public SubField SubField { get; set; }
    }

    public class Review
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public int FieldId { get; set; }
        public int Rating { get; set; } // 1-5
        public string Comment { get; set; }
        public string OwnerReply { get; set; }
        public DateTime? ReplyDate { get; set; }
        public bool IsVisible { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public User User { get; set; }
        public Field Field { get; set; }
    }

    public class Notification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; }
    }

    public class Promotion
    {
        public int PromotionId { get; set; }
        public int? FieldId { get; set; } // Added to support field-specific promotions
        public string Code { get; set; }
        public string Description { get; set; }
        public string DiscountType { get; set; } // "Percentage", "Fixed"
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? MinBookingValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public bool IsActive { get; set; }
        public int? UsageLimit { get; set; }
        public int UsageCount { get; set; }
        public Field Field { get; set; }
        public List<Booking> Bookings { get; set; }
    }

    public class Payment
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } // "VNPay", "BankCard"
        public string TransactionId { get; set; }
        public string Status { get; set; } // "Pending", "Completed", "Failed"
        public string Currency { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaymentDate { get; set; }
        public Booking Booking { get; set; }
    }

    public class RefreshToken
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool Revoked { get; set; }
        public string ReplacedByToken { get; set; }
        public Account Account { get; set; }
    }

    public class SearchHistory
    {
        public int SearchHistoryId { get; set; }
        public int UserId { get; set; }
        public string SearchQuery { get; set; }
        public DateTime SearchedAt { get; set; }
        public User User { get; set; }
    }

    public class FavoriteField
    {
        public int FavoriteId { get; set; }
        public int UserId { get; set; }
        public int FieldId { get; set; }
        public DateTime AddedDate { get; set; }
        public User User { get; set; }
        public Field Field { get; set; }
    }

    public class Sport
    {
        public int SportId { get; set; }
        public string SportName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public List<Field> Fields { get; set; }
    }
}