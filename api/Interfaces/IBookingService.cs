using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos.Booking;

namespace api.Interfaces
{
    public interface IBookingService
    {
        Task<PreviewBookingResponseDto> PreviewBookingAsync(ClaimsPrincipal user, PreviewBookingRequestDto request);
        Task<SimpleBookingResponseDto> CreateSimpleBookingAsync(ClaimsPrincipal user, SimpleBookingRequestDto request);
        Task<CreateBookingResponseDto> CreateBookingAsync(ClaimsPrincipal user, CreateBookingRequestDto request);
        Task<AddBookingServiceResponseDto> AddBookingServiceAsync(ClaimsPrincipal user, int bookingId, AddBookingServiceRequestDto request);
        Task<BookingDetailsResponseDto> GetBookingByIdAsync(ClaimsPrincipal user, int bookingId);
        Task<UserBookingsResponseDto> GetUserBookingsAsync(ClaimsPrincipal user, string? status, DateTime? startDate, DateTime? endDate, int page, int pageSize);
        Task<BookingServicesResponseDto> GetBookingServicesAsync(ClaimsPrincipal user, int bookingId, int page, int pageSize);
        Task<UpdateBookingResponseDto> UpdateBookingAsync(ClaimsPrincipal user, int bookingId, UpdateBookingRequestDto request);
        Task<ConfirmBookingResponseDto> ConfirmBookingAsync(ClaimsPrincipal user, int bookingId);
        Task<RescheduleBookingResponseDto> RescheduleBookingAsync(ClaimsPrincipal user, int bookingId, RescheduleBookingRequestDto request);
        Task<CancelBookingResponseDto> CancelBookingAsync(ClaimsPrincipal user, int bookingId);
    }
}