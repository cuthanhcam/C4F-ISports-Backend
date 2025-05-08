using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class Account
    {
        public int AccountId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public string? OAuthProvider { get; set; }
        public string? OAuthId { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? TokenExpiry { get; set; }

        public User? User { get; set; }
        public Owner? Owner { get; set; }
    }

    public class User
    {
        public int UserId { get; set; }
        public int AccountId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? AvatarUrl { get; set; }
        public decimal LoyaltyPoints { get; set; }

        public Account Account { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<FavoriteField> FavoriteFields { get; set; } = new List<FavoriteField>();
        public ICollection<SearchHistory> SearchHistories { get; set; } = new List<SearchHistory>();
    }

    public class Owner
    {
        public int OwnerId { get; set; }
        public int AccountId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Account Account { get; set; }
        public ICollection<Field> Fields { get; set; } = new List<Field>();
    }

    public class Sport
    {
        public int SportId { get; set; }
        public string SportName { get; set; }

        public ICollection<Field> Fields { get; set; } = new List<Field>();
    }

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
        public ICollection<SubField> SubFields { get; set; } = new List<SubField>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<FieldImage> FieldImages { get; set; } = new List<FieldImage>();
        public ICollection<FieldAmenity> FieldAmenities { get; set; } = new List<FieldAmenity>();
        public ICollection<FieldDescription> FieldDescriptions { get; set; } = new List<FieldDescription>();
        public ICollection<FieldService> FieldServices { get; set; } = new List<FieldService>();
        public ICollection<FavoriteField> FavoriteFields { get; set; } = new List<FavoriteField>();
    }

    public class SubField
    {
        public int SubFieldId { get; set; }
        public int FieldId { get; set; }
        public string SubFieldName { get; set; }
        public string FieldType { get; set; }
        public string Status { get; set; }

        public Field Field { get; set; }
        public ICollection<FieldPricing> FieldPricings { get; set; } = new List<FieldPricing>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }

    public class FieldPricing
    {
        public int FieldPricingId { get; set; }
        public int SubFieldId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int DayOfWeek { get; set; }
        public decimal Price { get; set; }

        public SubField SubField { get; set; }
    }

    public class FieldAmenity
    {
        public int FieldAmenityId { get; set; }
        public int FieldId { get; set; }
        public string AmenityName { get; set; }

        public Field Field { get; set; }
    }

    public class FieldDescription
    {
        public int FieldDescriptionId { get; set; }
        public int FieldId { get; set; }
        public string Description { get; set; }

        public Field Field { get; set; }
    }

    public class FieldImage
    {
        public int FieldImageId { get; set; }
        public int FieldId { get; set; }
        public string ImageUrl { get; set; }

        public Field Field { get; set; }
    }

    public class FieldService
    {
        public int FieldServiceId { get; set; }
        public int FieldId { get; set; }
        public string ServiceName { get; set; }
        public decimal Price { get; set; }

        public Field Field { get; set; }
        public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
    }

    public class Booking
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int SubFieldId { get; set; }
        public DateTime BookingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? PromotionId { get; set; }

        public User User { get; set; }
        public SubField SubField { get; set; }
        public Promotion? Promotion { get; set; }
        public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
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

    public class Payment
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public Booking Booking { get; set; }
    }

    public class Promotion
    {
        public int PromotionId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MinBookingValue { get; set; }
        public decimal MaxDiscountAmount { get; set; }
        public bool IsActive { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }

    public class Review
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public int FieldId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? OwnerReply { get; set; }
        public DateTime? ReplyDate { get; set; }

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

    public class FavoriteField
    {
        public int FavoriteId { get; set; }
        public int UserId { get; set; }
        public int FieldId { get; set; }
        public DateTime AddedDate { get; set; }

        public User User { get; set; }
        public Field Field { get; set; }
    }

    public class SearchHistory
    {
        public int SearchHistoryId { get; set; }
        public int UserId { get; set; }
        public string SearchQuery { get; set; }
        public DateTime SearchDate { get; set; }
        public int? FieldId { get; set; }

        public User User { get; set; }
        public Field? Field { get; set; }
    }
}