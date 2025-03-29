using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos;
using api.Dtos.Booking;
using api.Interfaces;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace api.Controllers
{
    [Route("api/booking")]
    [ApiController]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingController> _logger;

        // Constructor duy nhất
        public BookingController(IBookingService bookingService, ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto createBookingDto)
        {
            try
            {
                _logger.LogInformation("Creating booking for user: {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var booking = await _bookingService.CreateBookingAsync(User, createBookingDto);
                return CreatedAtAction(nameof(GetBooking), new { id = booking.BookingId }, booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return BadRequest(new { Message = ex.Message, InnerException = ex.InnerException?.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBookings(
            [FromQuery] string status = null,
            [FromQuery] int? fieldId = null,
            [FromQuery] string sort = "BookingDate:desc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Getting bookings for user: {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var bookings = await _bookingService.GetBookingsAsync(User, status, fieldId, sort, page, pageSize);
            return Ok(bookings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBooking(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(User, id);
            if (booking == null) return NotFound("Không tìm thấy đơn đặt sân.");
            return Ok(booking);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] UpdateBookingDto updateBookingDto)
        {
            try
            {
                var booking = await _bookingService.UpdateBookingAsync(User, id, updateBookingDto);
                if (booking == null) return NotFound("Không tìm thấy đơn đặt sân.");
                return Ok(booking);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            try
            {
                await _bookingService.CancelBookingAsync(User, id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusDto statusDto)
        {
            var booking = await _bookingService.UpdateBookingStatusAsync(User, id, statusDto.Status);
            if (booking == null) return Forbid("Không có quyền cập nhật trạng thái.");
            return Ok(booking);
        }

        [HttpGet("{id}/services")]
        public async Task<IActionResult> GetBookingServices(int id)
        {
            var services = await _bookingService.GetBookingServicesAsync(User, id);
            if (services == null) return NotFound("Không tìm thấy đơn đặt sân.");
            return Ok(services);
        }

        [HttpPost("preview")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> PreviewBooking([FromBody] CreateBookingDto createBookingDto)
        {
            try
            {
                var preview = await _bookingService.PreviewBookingAsync(createBookingDto);
                return Ok(preview);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}