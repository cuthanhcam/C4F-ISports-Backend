using System;
using System.Threading.Tasks;
using api.Dtos.Booking;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    /// <summary>
    /// Controller xử lý các yêu cầu liên quan đến quản lý đặt sân.
    /// </summary>
    [Route("api/bookings")]
    [ApiController]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingController> _logger;

        /// <summary>
        /// Khởi tạo BookingController với các dependencies cần thiết.
        /// </summary>
        /// <param name="bookingService">Service xử lý logic đặt sân.</param>
        /// <param name="logger">Logger để ghi log hệ thống.</param>
        public BookingController(IBookingService bookingService, ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        /// <summary>
        /// Xem trước thông tin đặt sân bao gồm giá và tính khả dụng.
        /// </summary>
        /// <param name="request">Thông tin đặt sân cần xem trước.</param>
        /// <returns>Thông tin giá và tính khả dụng của đặt sân.</returns>
        /// <response code="200">Trả về thông tin xem trước đặt sân.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
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

        /// <summary>
        /// Tạo đặt sân đơn giản không kèm dịch vụ hoặc mã khuyến mãi.
        /// </summary>
        /// <param name="request">Thông tin đặt sân cần tạo.</param>
        /// <returns>Thông tin đặt sân đã tạo.</returns>
        /// <response code="201">Tạo đặt sân thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
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

        /// <summary>
        /// Tạo một hoặc nhiều đặt sân cho nhiều sân con với nhiều khung giờ, có thể kèm dịch vụ và mã khuyến mãi.
        /// </summary>
        /// <param name="request">Thông tin các đặt sân cần tạo.</param>
        /// <returns>Thông tin các đặt sân đã tạo và URL thanh toán.</returns>
        /// <response code="201">Tạo đặt sân thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="404">Không tìm thấy sân con hoặc dịch vụ.</response>
        /// <response code="409">Khung giờ đã được đặt.</response>
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

        /// <summary>
        /// Thêm dịch vụ vào đặt sân đã tạo.
        /// </summary>
        /// <param name="bookingId">ID của đặt sân cần thêm dịch vụ.</param>
        /// <param name="request">Thông tin dịch vụ cần thêm.</param>
        /// <returns>Thông tin dịch vụ đã thêm.</returns>
        /// <response code="201">Thêm dịch vụ thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Không có quyền thêm dịch vụ.</response>
        /// <response code="404">Không tìm thấy đặt sân.</response>
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

        /// <summary>
        /// Lấy thông tin chi tiết của một đặt sân.
        /// </summary>
        /// <param name="bookingId">ID của đặt sân cần lấy thông tin.</param>
        /// <returns>Thông tin chi tiết đặt sân.</returns>
        /// <response code="200">Trả về thông tin chi tiết đặt sân.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Không có quyền xem đặt sân.</response>
        /// <response code="404">Không tìm thấy đặt sân.</response>
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

        /// <summary>
        /// Lấy danh sách đặt sân của người dùng hiện tại.
        /// </summary>
        /// <param name="status">Lọc theo trạng thái (Confirmed, Pending, Cancelled).</param>
        /// <param name="startDate">Ngày bắt đầu lọc.</param>
        /// <param name="endDate">Ngày kết thúc lọc.</param>
        /// <param name="page">Số trang (mặc định: 1).</param>
        /// <param name="pageSize">Số mục mỗi trang (mặc định: 10).</param>
        /// <returns>Danh sách đặt sân của người dùng.</returns>
        /// <response code="200">Trả về danh sách đặt sân.</response>
        /// <response code="400">Tham số không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
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

        /// <summary>
        /// Lấy danh sách dịch vụ của đặt sân.
        /// </summary>
        /// <param name="bookingId">ID của đặt sân cần lấy dịch vụ.</param>
        /// <param name="page">Số trang (mặc định: 1).</param>
        /// <param name="pageSize">Số mục mỗi trang (mặc định: 10).</param>
        /// <returns>Danh sách dịch vụ của đặt sân.</returns>
        /// <response code="200">Trả về danh sách dịch vụ.</response>
        /// <response code="400">Tham số không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Không có quyền xem dịch vụ.</response>
        /// <response code="404">Không tìm thấy đặt sân.</response>
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

        /// <summary>
        /// Cập nhật thông tin đặt sân (chỉ dành cho chủ sân).
        /// </summary>
        /// <param name="bookingId">ID của đặt sân cần cập nhật.</param>
        /// <param name="request">Thông tin cập nhật.</param>
        /// <returns>Thông tin đặt sân sau khi cập nhật.</returns>
        /// <response code="200">Cập nhật đặt sân thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Không có quyền cập nhật đặt sân.</response>
        /// <response code="404">Không tìm thấy đặt sân.</response>
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

        /// <summary>
        /// Xác nhận đặt sân (chỉ dành cho chủ sân).
        /// </summary>
        /// <param name="bookingId">ID của đặt sân cần xác nhận.</param>
        /// <returns>Thông tin đặt sân sau khi xác nhận.</returns>
        /// <response code="200">Xác nhận đặt sân thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Không có quyền xác nhận đặt sân.</response>
        /// <response code="404">Không tìm thấy đặt sân.</response>
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

        /// <summary>
        /// Đổi lịch đặt sân (chỉ dành cho người dùng).
        /// </summary>
        /// <param name="bookingId">ID của đặt sân cần đổi lịch.</param>
        /// <param name="request">Thông tin lịch mới.</param>
        /// <returns>Thông tin đặt sân sau khi đổi lịch.</returns>
        /// <response code="200">Đổi lịch đặt sân thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Không có quyền đổi lịch đặt sân.</response>
        /// <response code="404">Không tìm thấy đặt sân.</response>
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

        /// <summary>
        /// Hủy đặt sân (dành cho người dùng hoặc chủ sân).
        /// </summary>
        /// <param name="bookingId">ID của đặt sân cần hủy.</param>
        /// <returns>Thông tin đặt sân sau khi hủy.</returns>
        /// <response code="200">Hủy đặt sân thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Không có quyền hủy đặt sân.</response>
        /// <response code="404">Không tìm thấy đặt sân.</response>
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