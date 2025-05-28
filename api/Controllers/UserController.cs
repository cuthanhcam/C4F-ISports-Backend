using api.Dtos.User;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace api.Controllers
{
    /// <summary>
    /// Controller xử lý các yêu cầu quản lý người dùng.
    /// </summary>
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy thông tin hồ sơ của người dùng hiện tại.
        /// </summary>
        /// <returns>Thông tin hồ sơ người dùng.</returns>
        /// <response code="200">Trả về thông tin hồ sơ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        [HttpGet("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var profile = await _userService.GetProfileAsync(User);
                _logger.LogInformation("Lấy hồ sơ thành công cho người dùng");
                return Ok(profile);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi lấy hồ sơ người dùng");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi lấy hồ sơ người dùng");
                return BadRequest(new { error = "Invalid input", details = new[] { new { field = "account", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi lấy hồ sơ người dùng");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Cập nhật thông tin hồ sơ của người dùng hiện tại.
        /// </summary>
        /// <param name="updateProfileDto">Thông tin cần cập nhật.</param>
        /// <returns>Thông tin hồ sơ đã cập nhật.</returns>
        /// <response code="200">Cập nhật hồ sơ thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        [HttpPut("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi cập nhật hồ sơ: {Errors}", ModelState);
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => new { field = e.ErrorMessage.Contains("FullName") ? "fullName" : e.ErrorMessage.Contains("Phone") ? "phone" : "unknown", message = e.ErrorMessage });
                    return BadRequest(new { error = "Invalid input", details = errors });
                }

                var updatedProfile = await _userService.UpdateProfileAsync(User, updateProfileDto);
                _logger.LogInformation("Cập nhật hồ sơ thành công");
                return Ok(updatedProfile);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi cập nhật hồ sơ");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi cập nhật hồ sơ");
                return BadRequest(new { error = "Invalid input", details = new[] { new { field = "account", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi cập nhật hồ sơ");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Xóa tài khoản của người dùng hiện tại (soft delete).
        /// </summary>
        /// <returns>Thông báo xóa tài khoản thành công.</returns>
        /// <response code="200">Xóa tài khoản thành công.</response>
        /// <response code="400">Lỗi khi xóa tài khoản (ví dụ: tài khoản có đặt sân hoặc sân hoạt động).</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Người dùng không có quyền truy cập.</response>
        [HttpDelete("profile")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteProfile()
        {
            try
            {
                await _userService.DeleteProfileAsync(User);
                _logger.LogInformation("Profile deleted successfully");
                return Ok(new { message = "Profile deleted successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Authentication error while deleting profile");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Cannot delete profile: {Error}", ex.Message);
                return BadRequest(new { error = "Invalid request", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System error while deleting profile");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Lấy điểm loyalty của người dùng hiện tại.
        /// </summary>
        /// <returns>Thông tin điểm loyalty.</returns>
        /// <response code="200">Trả về điểm loyalty.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        [HttpGet("loyalty-points")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetLoyaltyPoints()
        {
            try
            {
                var loyaltyPoints = await _userService.GetLoyaltyPointsAsync(User);
                _logger.LogInformation("Lấy điểm loyalty thành công");
                return Ok(loyaltyPoints);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi lấy điểm loyalty");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi lấy điểm loyalty");
                return BadRequest(new { error = "Invalid input", details = new[] { new { field = "account", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi lấy điểm loyalty");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Lấy danh sách sân yêu thích của người dùng hiện tại.
        /// </summary>
        /// <param name="sort">Sắp xếp theo trường (FieldName:asc/desc).</param>
        /// <param name="page">Số trang (mặc định: 1).</param>
        /// <param name="pageSize">Số mục mỗi trang (mặc định: 10).</param>
        /// <returns>Danh sách sân yêu thích.</returns>
        /// <response code="200">Trả về danh sách sân yêu thích.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        [HttpGet("favorites")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetFavoriteFields(
            [FromQuery] string? sort,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    _logger.LogWarning("Tham số phân trang không hợp lệ: page={Page}, pageSize={PageSize}", page, pageSize);
                    return BadRequest(new { error = "Invalid input", details = new[] { new { field = "pagination", message = "Page and pageSize must be positive." } } });
                }

                var (data, total, pageResult, pageSizeResult) = await _userService.GetFavoriteFieldsAsync(User, sort, page, pageSize);
                _logger.LogInformation("Lấy danh sách sân yêu thích thành công");
                return Ok(new { data, total, page = pageResult, pageSize = pageSizeResult });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi lấy danh sách sân yêu thích");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi lấy danh sách sân yêu thích");
                return BadRequest(new { error = "Invalid input", details = new[] { new { field = "account", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi lấy danh sách sân yêu thích");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Thêm sân vào danh sách yêu thích của người dùng.
        /// </summary>
        /// <param name="addFavoriteFieldDto">Thông tin sân cần thêm.</param>
        /// <returns>Thông báo thêm sân thành công và ID của sân.</returns>
        /// <response code="201">Thêm sân thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ hoặc sân đã có trong danh sách.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="404">Sân không tồn tại.</response>
        [HttpPost("favorites")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddFavoriteField([FromBody] AddFavoriteFieldDto addFavoriteFieldDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi thêm sân yêu thích: {Errors}", ModelState);
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => new { field = e.ErrorMessage.Contains("FieldId") ? "fieldId" : "unknown", message = e.ErrorMessage });
                    return BadRequest(new { error = "Invalid input", details = errors });
                }

                var favoriteId = await _userService.AddFavoriteFieldAsync(User, addFavoriteFieldDto);
                _logger.LogInformation("Thêm sân yêu thích thành công, FieldId: {FieldId}", addFavoriteFieldDto.FieldId);
                return StatusCode(StatusCodes.Status201Created, new { fieldId = addFavoriteFieldDto.FieldId, message = "Field added to favorites" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi thêm sân yêu thích");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi thêm sân yêu thích");
                return ex.Message.Contains("Field not found")
                    ? NotFound(new { error = "Resource not found", message = ex.Message })
                    : BadRequest(new { error = "Invalid input", details = new[] { new { field = "fieldId", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi thêm sân yêu thích");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Xóa sân khỏi danh sách yêu thích của người dùng.
        /// </summary>
        /// <param name="fieldId">ID của sân cần xóa.</param>
        /// <returns>Thông báo xóa sân thành công.</returns>
        /// <response code="200">Xóa sân thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="404">Sân không có trong danh sách yêu thích.</response>
        [HttpDelete("favorites/{fieldId}")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveFavoriteField(int fieldId)
        {
            try
            {
                if (fieldId < 1)
                {
                    _logger.LogWarning("FieldId không hợp lệ: {FieldId}", fieldId);
                    return BadRequest(new { error = "Invalid input", details = new[] { new { field = "fieldId", message = "FieldId must be positive." } } });
                }

                await _userService.RemoveFavoriteFieldAsync(User, fieldId);
                _logger.LogInformation("Xóa sân yêu thích thành công, FieldId: {FieldId}", fieldId);
                return Ok(new { message = "Favorite field removed successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi xóa sân yêu thích");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi xóa sân yêu thích");
                return BadRequest(new { error = "Invalid input", details = new[] { new { field = "fieldId", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi xóa sân yêu thích");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Lấy lịch sử đặt sân của người dùng hiện tại.
        /// </summary>
        /// <param name="status">Lọc theo trạng thái (Confirmed, Pending, Cancelled).</param>
        /// <param name="startDate">Ngày bắt đầu lọc (YYYY-MM-DD).</param>
        /// <param name="endDate">Ngày kết thúc lọc (YYYY-MM-DD).</param>
        /// <param name="sort">Sắp xếp theo trường (BookingDate:asc/desc).</param>
        /// <param name="page">Số trang (mặc định: 1).</param>
        /// <param name="pageSize">Số mục mỗi trang (mặc định: 10).</param>
        /// <returns>Danh sách lịch sử đặt sân.</returns>
        /// <response code="200">Trả về lịch sử đặt sân.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        [HttpGet("bookings")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetBookingHistory(
            [FromQuery] string? status,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sort,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    _logger.LogWarning("Tham số phân trang không hợp lệ: page={Page}, pageSize={PageSize}", page, pageSize);
                    return BadRequest(new { error = "Invalid input", details = new[] { new { field = "pagination", message = "Page and pageSize must be positive." } } });
                }

                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                {
                    _logger.LogWarning("Khoảng thời gian không hợp lệ: startDate={StartDate}, endDate={EndDate}", startDate, endDate);
                    return BadRequest(new { error = "Invalid input", details = new[] { new { field = "dateRange", message = "startDate cannot be greater than endDate." } } });
                }

                var (data, total, pageResult, pageSizeResult) = await _userService.GetBookingHistoryAsync(User, status, startDate, endDate, sort, page, pageSize);
                _logger.LogInformation("Lấy lịch sử đặt sân thành công");
                return Ok(new { data, total, page = pageResult, pageSize = pageSizeResult });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi lấy lịch sử đặt sân");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi lấy lịch sử đặt sân");
                return BadRequest(new { error = "Invalid input", details = new[] { new { field = "account", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi lấy lịch sử đặt sân");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Lấy lịch sử tìm kiếm của người dùng hiện tại.
        /// </summary>
        /// <param name="startDate">Ngày bắt đầu lọc (YYYY-MM-DD).</param>
        /// <param name="endDate">Ngày kết thúc lọc (YYYY-MM-DD).</param>
        /// <param name="page">Số trang (mặc định: 1).</param>
        /// <param name="pageSize">Số mục mỗi trang (mặc định: 10).</param>
        /// <returns>Danh sách lịch sử tìm kiếm.</returns>
        /// <response code="200">Trả về lịch sử tìm kiếm.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Người dùng không có quyền truy cập.</response>
        [HttpGet("search-history")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetSearchHistory(
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
                    return BadRequest(new { error = "Invalid input", details = new[] { new { field = "pagination", message = "Page and pageSize must be positive." } } });
                }

                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                {
                    _logger.LogWarning("Khoảng thời gian không hợp lệ: startDate={StartDate}, endDate={EndDate}", startDate, endDate);
                    return BadRequest(new { error = "Invalid input", details = new[] { new { field = "dateRange", message = "startDate cannot be greater than endDate." } } });
                }

                var (data, total, pageResult, pageSizeResult) = await _userService.GetSearchHistoryAsync(User, startDate, endDate, page, pageSize);
                _logger.LogInformation("Lấy lịch sử tìm kiếm thành công");
                return Ok(new { data, totalCount = total, page = pageResult, pageSize = pageSizeResult, message = "Search history retrieved successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi lấy lịch sử tìm kiếm");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi lấy lịch sử tìm kiếm");
                return BadRequest(new { error = "Invalid input", details = new[] { new { field = "account", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi lấy lịch sử tìm kiếm");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Clears the search history of the current user.
        /// </summary>
        /// <returns>Confirmation message.</returns>
        /// <response code="200">Search history cleared successfully.</response>
        /// <response code="401">Unauthorized or invalid token.</response>
        /// <response code="403">Forbidden, user not authorized.</response>
        [HttpDelete("search-history")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ClearSearchHistory()
        {
            try
            {
                await _userService.ClearSearchHistoryAsync(User);
                _logger.LogInformation("Cleared search history successfully");
                return Ok(new { message = "Search history cleared successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Authentication failed while clearing search history");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System error while clearing search history");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Lấy danh sách đánh giá của người dùng hiện tại.
        /// </summary>
        /// <param name="sort">Sắp xếp theo trường (CreatedAt:asc/desc).</param>
        /// <param name="page">Số trang (mặc định: 1).</param>
        /// <param name="pageSize">Số mục mỗi trang (mặc định: 10).</param>
        /// <returns>Danh sách đánh giá.</returns>
        /// <response code="200">Trả về danh sách đánh giá.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        [HttpGet("reviews")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserReviews(
            [FromQuery] string? sort,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    _logger.LogWarning("Tham số phân trang không hợp lệ: page={Page}, pageSize={PageSize}", page, pageSize);
                    return BadRequest(new { error = "Invalid input", details = new[] { new { field = "pagination", message = "Page and pageSize must be positive." } } });
                }

                var (data, total, pageResult, pageSizeResult) = await _userService.GetUserReviewsAsync(User, sort, page, pageSize);
                _logger.LogInformation("Lấy danh sách đánh giá thành công");
                return Ok(new { data, total, page = pageResult, pageSize = pageSizeResult });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi lấy danh sách đánh giá");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi lấy danh sách đánh giá");
                return BadRequest(new { error = "Invalid input", details = new[] { new { field = "account", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi lấy danh sách đánh giá");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Tải lên ảnh đại diện mới cho người dùng.
        /// </summary>
        /// <param name="file">File ảnh tải lên.</param>
        /// <returns>URL của ảnh đại diện mới.</returns>
        /// <response code="200">Tải lên ảnh đại diện thành công.</response>
        /// <response code="400">File không hợp lệ hoặc không được cung cấp.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Người dùng không có quyền tải lên ảnh đại diện.</response>
        [HttpPost("avatar")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    _logger.LogWarning("Không có file được cung cấp");
                    return BadRequest(new { error = "Invalid input", details = new[] { new { field = "file", message = "Vui lòng chọn file ảnh." } } });
                }

                var avatarUrl = await _userService.UploadAvatarAsync(User, file);
                _logger.LogInformation("Tải lên ảnh đại diện thành công");
                return Ok(new { avatarUrl, message = "Avatar uploaded successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi tải lên ảnh đại diện");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi tải lên ảnh đại diện");
                return BadRequest(new { error = "Invalid input", details = new[] { new { field = "file", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi tải lên ảnh đại diện");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "An unexpected error occurred." });
            }
        }
    }
}