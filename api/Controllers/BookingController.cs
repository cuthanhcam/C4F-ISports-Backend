using System;
using System.Threading.Tasks;
using api.Dtos.Booking;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/bookings")]
    [ApiController]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(IBookingService bookingService, ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        // 6.1 Preview Booking
        [HttpPost("preview")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PreviewBooking([FromBody] PreviewBookingRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi xem trước đặt sân: {Errors}", ModelState);
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => new { field = e.ErrorMessage, message = e.ErrorMessage });
                    return BadRequest(new { error = "Invalid input", details = errors });
                }

                var result = await _bookingService.PreviewBookingAsync(User, request);
                _logger.LogInformation("Xem trước đặt sân thành công cho SubFieldId: {SubFieldId}", request.SubFieldId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi xem trước đặt sân");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi xem trước đặt sân");
                return BadRequest(new { error = "Invalid input", details = new[] { new { field = "request", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi xem trước đặt sân");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        // 6.2 Create Simple Booking
        [HttpPost("simple")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateSimpleBooking([FromBody] SimpleBookingRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi tạo đặt sân đơn giản: {Errors}", ModelState);
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => new { field = e.ErrorMessage, message = e.ErrorMessage });
                    return BadRequest(new { error = "Invalid input", details = errors });
                }

                var result = await _bookingService.CreateSimpleBookingAsync(User, request);
                _logger.LogInformation("Tạo đặt sân đơn giản thành công, BookingId: {BookingId}", result.BookingId);
                return CreatedAtAction(nameof(GetBookingById), new { bookingId = result.BookingId }, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi tạo đặt sân đơn giản");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi tạo đặt sân đơn giản");
                return BadRequest(new { error = "Invalid input", details = new[] { new { field = "request", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi tạo đặt sân đơn giản");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        // 6.3 Create Booking
        [HttpPost]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi tạo đặt sân: {Errors}", ModelState);
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => new { field = e.ErrorMessage, message = e.ErrorMessage });
                    return BadRequest(new { error = "Invalid input", details = errors });
                }

                var result = await _bookingService.CreateBookingAsync(User, request);
                _logger.LogInformation("Tạo đặt sân thành công, MainBookingId: {MainBookingId}", result.MainBookingId);
                return CreatedAtAction(nameof(GetBookingById), new { bookingId = result.MainBookingId }, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi tạo đặt sân");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi tạo đặt sân");
                return BadRequest(new { error = "Invalid input", details = new[] { new { field = "request", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi tạo đặt sân");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        // 6.4 Add Booking Service
        [HttpPost("{bookingId}/services")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddBookingService(int bookingId, [FromBody] AddBookingServiceRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi thêm dịch vụ: {Errors}", ModelState);
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => new { field = e.ErrorMessage, message = e.ErrorMessage });
                    return BadRequest(new { error = "Invalid input", details = errors });
                }

                var result = await _bookingService.AddBookingServiceAsync(User, bookingId, request);
                _logger.LogInformation("Thêm dịch vụ thành công, BookingServiceId: {BookingServiceId}", result.BookingServiceId);
                return CreatedAtAction(nameof(GetBookingServices), new { bookingId }, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi thêm dịch vụ");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi thêm dịch vụ");
                return BadRequest(new { error = "Invalid input", details = new[] { new { field = "request", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi thêm dịch vụ");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        // 6.5 Get Booking By Id
        [HttpGet("{bookingId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBookingById(int bookingId)
        {
            try
            {
                var result = await _bookingService.GetBookingByIdAsync(User, bookingId);
                _logger.LogInformation("Lấy chi tiết đặt sân thành công, BookingId: {BookingId}", bookingId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi lấy chi tiết đặt sân");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi lấy chi tiết đặt sân");
                return NotFound(new { error = "Resource not found", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi lấy chi tiết đặt sân");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        // 6.6 Get User Bookings
        [HttpGet]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserBookings(
            [FromQuery] string? status,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    _logger.LogWarning("Tham số phân trang không hợp lệ: page={Page}, pageSize={PageSize}", page, pageSize);
                    return BadRequest(new { error = "Invalid input", details = new[] { new { field = "pagination", message = "Page và pageSize phải là số dương." } } });
                }

                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                {
                    _logger.LogWarning("Khoảng thời gian không hợp lệ: startDate={StartDate}, endDate={EndDate}", startDate, endDate);
                    return BadRequest(new { error = "Invalid input", details = new[] { new { field = "dateRange", message = "startDate không được lớn hơn endDate." } } });
                }

                var result = await _bookingService.GetUserBookingsAsync(User, status, startDate, endDate, page, pageSize);
                _logger.LogInformation("Lấy danh sách đặt sân thành công");
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi lấy danh sách đặt sân");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi lấy danh sách đặt sân");
                return BadRequest(new { error = "Invalid input", details = new[] { new { field = "request", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi lấy danh sách đặt sân");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        // 6.7 Get Booking Services
        [HttpGet("{bookingId}/services")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBookingServices(int bookingId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    _logger.LogWarning("Tham số phân trang không hợp lệ: page={Page}, pageSize={PageSize}", page, pageSize);
                    return BadRequest(new { error = "Invalid input", details = new[] { new { field = "pagination", message = "Page và pageSize phải là số dương." } } });
                }

                var result = await _bookingService.GetBookingServicesAsync(User, bookingId, page, pageSize);
                _logger.LogInformation("Lấy danh sách dịch vụ thành công cho BookingId: {BookingId}", bookingId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi lấy danh sách dịch vụ");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi lấy danh sách dịch vụ");
                return NotFound(new { error = "Resource not found", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi lấy danh sách dịch vụ");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        // 6.8 Update Booking
        [HttpPut("{bookingId}")]
        [Authorize(Roles = "Owner")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBooking(int bookingId, [FromBody] UpdateBookingRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi cập nhật đặt sân: {Errors}", ModelState);
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => new { field = e.ErrorMessage, message = e.ErrorMessage });
                    return BadRequest(new { error = "Invalid input", details = errors });
                }

                var result = await _bookingService.UpdateBookingAsync(User, bookingId, request);
                _logger.LogInformation("Cập nhật đặt sân thành công, BookingId: {BookingId}", bookingId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi cập nhật đặt sân");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi cập nhật đặt sân");
                return NotFound(new { error = "Resource not found", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi cập nhật đặt sân");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        // 6.9 Confirm Booking
        [HttpPost("{bookingId}/confirm")]
        [Authorize(Roles = "Owner")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmBooking(int bookingId)
        {
            try
            {
                var result = await _bookingService.ConfirmBookingAsync(User, bookingId);
                _logger.LogInformation("Xác nhận đặt sân thành công, BookingId: {BookingId}", bookingId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi xác nhận đặt sân");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi xác nhận đặt sân");
                return BadRequest(new { error = "Invalid input", details = new[] { new { field = "bookingId", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi xác nhận đặt sân");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        // 6.10 Reschedule Booking
        [HttpPost("{bookingId}/reschedule")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RescheduleBooking(int bookingId, [FromBody] RescheduleBookingRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi đổi lịch đặt sân: {Errors}", ModelState);
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => new { field = e.ErrorMessage, message = e.ErrorMessage });
                    return BadRequest(new { error = "Invalid input", details = errors });
                }

                var result = await _bookingService.RescheduleBookingAsync(User, bookingId, request);
                _logger.LogInformation("Đổi lịch đặt sân thành công, BookingId: {BookingId}", bookingId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi đổi lịch đặt sân");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi đổi lịch đặt sân");
                return BadRequest(new { error = "Invalid input", details = new[] { new { field = "request", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi đổi lịch đặt sân");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        // 6.11 Cancel Booking
        [HttpPost("{bookingId}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            try
            {
                var result = await _bookingService.CancelBookingAsync(User, bookingId);
                _logger.LogInformation("Hủy đặt sân thành công, BookingId: {BookingId}", bookingId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi hủy đặt sân");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi hủy đặt sân");
                return BadRequest(new { error = "Invalid input", details = new[] { new { field = "bookingId", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi hủy đặt sân");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }
    }
}