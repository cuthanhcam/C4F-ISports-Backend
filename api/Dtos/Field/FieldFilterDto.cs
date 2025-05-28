using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Field
{
    public class FieldFilterDto
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        public string? City { get; set; }
        public string? District { get; set; }
        public int? SportId { get; set; }
        public string? Search { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double Radius { get; set; } = 10;
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string SortBy { get; set; } = "fieldId";
        public string SortOrder { get; set; } = "asc";
    }

    public class FieldResponseDto
    {
        public int FieldId { get; set; }
        public string FieldName { get; set; }
        public string? Description { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string OpenTime { get; set; }
        public string CloseTime { get; set; }
        public decimal AverageRating { get; set; }
        public int SportId { get; set; }
        public double? Distance { get; set; }
        public decimal MinPricePerSlot { get; set; }
        public decimal MaxPricePerSlot { get; set; }
        public List<SubFieldResponseDto> SubFields { get; set; }
        public List<FieldServiceResponseDto> Services { get; set; }
        public List<FieldAmenityResponseDto> Amenities { get; set; }
        public List<FieldImageResponseDto> Images { get; set; }
    }

    public class SubFieldResponseDto
    {
        public int SubFieldId { get; set; }
        public string SubFieldName { get; set; }
        public string FieldType { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; }
        public int Capacity { get; set; }
        public string OpenTime { get; set; }
        public string CloseTime { get; set; }
        public decimal DefaultPricePerSlot { get; set; }
        public List<PricingRuleResponseDto> PricingRules { get; set; }
        public int? Parent7aSideId { get; set; }
        public List<int> Child5aSideIds { get; set; }
    }

    public class PricingRuleResponseDto
    {
        public int PricingRuleId { get; set; }
        public List<string> AppliesToDays { get; set; }
        public List<TimeSlotResponseDto> TimeSlots { get; set; }
    }

    public class TimeSlotResponseDto
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public decimal PricePerSlot { get; set; }
    }

    public class FieldServiceResponseDto
    {
        public int FieldServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class FieldAmenityResponseDto
    {
        public int FieldAmenityId { get; set; }
        public string AmenityName { get; set; }
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
    }

    public class FieldImageResponseDto
    {
        public int FieldImageId { get; set; }
        public string ImageUrl { get; set; }
        public string? PublicId { get; set; }
        public string? Thumbnail { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class ValidateAddressDto
    {
        [Required]
        public string FieldName { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string District { get; set; }
    }

    public class ValidateAddressResponseDto
    {
        public bool IsValid { get; set; }
        public string FormattedAddress { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class CreateFieldDto
    {
        [Required, StringLength(100)]
        public string FieldName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required, StringLength(500)]
        public string Address { get; set; }

        [Required, StringLength(100)]
        public string City { get; set; }

        [Required, StringLength(100)]
        public string District { get; set; }

        [Required]
        public string OpenTime { get; set; }

        [Required]
        public string CloseTime { get; set; }

        [Required]
        public int SportId { get; set; }

        [Required]
        public List<CreateSubFieldDto> SubFields { get; set; }

        public List<CreateFieldServiceDto> Services { get; set; }
        public List<CreateFieldAmenityDto> Amenities { get; set; }
        public List<CreateFieldImageDto> Images { get; set; }
    }

    public class CreateSubFieldDto
    {
        [Required, StringLength(100)]
        public string SubFieldName { get; set; }

        [Required, StringLength(50)]
        public string FieldType { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int Capacity { get; set; }

        [Required]
        public string OpenTime { get; set; }

        [Required]
        public string CloseTime { get; set; }

        [Required]
        public decimal DefaultPricePerSlot { get; set; }

        public List<CreatePricingRuleDto> PricingRules { get; set; }
        public int? Parent7aSideId { get; set; }
        public List<int> Child5aSideIds { get; set; }
    }

    public class CreatePricingRuleDto
    {
        [Required]
        public List<string> AppliesToDays { get; set; }

        [Required]
        public List<CreateTimeSlotDto> TimeSlots { get; set; }
    }

    public class CreateTimeSlotDto
    {
        [Required]
        public string StartTime { get; set; }

        [Required]
        public string EndTime { get; set; }

        [Required]
        public decimal PricePerSlot { get; set; }
    }

    public class CreateFieldServiceDto
    {
        [Required, StringLength(100)]
        public string ServiceName { get; set; }

        [Required]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class CreateFieldAmenityDto
    {
        [Required, StringLength(100)]
        public string AmenityName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(500), Url]
        public string? IconUrl { get; set; }
    }

    public class CreateFieldImageDto
    {
        [Required, StringLength(500), Url]
        public string ImageUrl { get; set; }

        [StringLength(500)]
        public string? PublicId { get; set; }

        [StringLength(500), Url]
        public string? Thumbnail { get; set; }

        public bool IsPrimary { get; set; }
    }

    public class UpdateFieldDto : CreateFieldDto { }

    public class AvailabilityFilterDto
    {
        [Required]
        public DateTime Date { get; set; }

        public int? SubFieldId { get; set; }
        public int? SportId { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
    }

    public class AvailabilityResponseDto
    {
        public int SubFieldId { get; set; }
        public string SubFieldName { get; set; }
        public List<AvailableSlotDto> AvailableSlots { get; set; }
    }

    public class AvailableSlotDto
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public decimal PricePerSlot { get; set; }
        public bool IsAvailable { get; set; }
        public string? UnavailableReason { get; set; }
    }

    public class ReviewFilterDto
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        [Range(1, 5)]
        public int? MinRating { get; set; }

        public string SortBy { get; set; } = "createdAt";
        public string SortOrder { get; set; } = "desc";
    }

    public class ReviewResponseDto
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? OwnerReply { get; set; }
        public DateTime? ReplyDate { get; set; }
    }

    public class BookingFilterDto
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class BookingResponseDto
    {
        public int BookingId { get; set; }
        public int SubFieldId { get; set; }
        public string SubFieldName { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public DateTime BookingDate { get; set; }
        public List<BookingTimeSlotResponseDto> TimeSlots { get; set; }
        public List<BookingServiceResponseDto> Services { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class BookingTimeSlotResponseDto
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public decimal Price { get; set; }
    }

    public class BookingServiceResponseDto
    {
        public int BookingServiceId { get; set; }
        public int FieldServiceId { get; set; }
        public string ServiceName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
    }
}