using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos;
using api.Dtos.Booking;

namespace api.Services
{
    public interface IBookingService
    {
        Task<BookingDto> CreateBookingAsync(ClaimsPrincipal user, CreateBookingDto createBookingDto);
        Task<PaginatedResponse<BookingDto>> GetBookingsAsync(ClaimsPrincipal user, string status, int? fieldId, string sort, int page, int pageSize);
        Task<BookingDto> GetBookingByIdAsync(ClaimsPrincipal user, int bookingId);
        Task<BookingDto> UpdateBookingAsync(ClaimsPrincipal user, int bookingId, UpdateBookingDto updateBookingDto);
        Task CancelBookingAsync(ClaimsPrincipal user, int bookingId);
        Task<BookingDto> UpdateBookingStatusAsync(ClaimsPrincipal user, int bookingId, string status);
        Task<List<BookingServiceDto>> GetBookingServicesAsync(ClaimsPrincipal user, int bookingId);
        Task<BookingPreviewDto> PreviewBookingAsync(CreateBookingDto createBookingDto);
    }
}