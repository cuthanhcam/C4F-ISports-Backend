using api.Dtos.User;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.Tasks;

namespace api.Controllers
{
    /// <summary>
    /// Controller xử lý các yêu cầu quản lý người dùng.
    /// </summary>
    [Route("api/users")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("api")]
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy hồ sơ người dùng");
                return Unauthorized(new { Error = ex.Message });
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
                    return BadRequest(ModelState);
                }

                var updatedProfile = await _userService.UpdateProfileAsync(User, updateProfileDto);
                _logger.LogInformation("Cập nhật hồ sơ thành công");
                return Ok(new { Message = "Cập nhật hồ sơ thành công", User = updatedProfile });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật hồ sơ");
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa tài khoản của người dùng hiện tại.
        /// </summary>
        /// <returns>Thông báo xóa tài khoản thành công.</returns>
        /// <response code="200">Xóa tài khoản thành công.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="400">Lỗi khi xóa tài khoản (ví dụ: tài khoản Owner có sân hoạt động).</response>
        [HttpDelete("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteProfile()
        {
            try
            {
                await _userService.DeleteProfileAsync(User);
                _logger.LogInformation("Xóa tài khoản thành công");
                return Ok(new { Message = "Xóa tài khoản thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa tài khoản");
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy lịch sử đặt sân của người dùng.
        /// </summary>
        /// <param name="status">Lọc theo trạng thái (Confirmed, Pending, Cancelled).</param>
        /// <param name="sort">Sắp xếp theo trường (BookingDate:asc/desc).</param>
        /// <param name="page">Số trang (mặc định: 1).</param>
        /// <param name="pageSize">Số mục mỗi trang (mặc định: 10).</param>
        /// <returns>Danh sách lịch sử đặt sân.</returns>
        /// <response code="200">Trả về lịch sử đặt sân.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        [HttpGet("bookings")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetBookingHistory(
            [FromQuery] string? status,
            [FromQuery] string? sort,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    _logger.LogWarning("Tham số phân trang không hợp lệ: page={Page}, pageSize={PageSize}", page, pageSize);
                    return BadRequest(new { Error = "Tham số phân trang không hợp lệ" });
                }

                var (data, total, pageResult, pageSizeResult) = await _userService.GetBookingHistoryAsync(User, status, sort, page, pageSize);
                _logger.LogInformation("Lấy lịch sử đặt sân thành công");
                return Ok(new { Data = data, Total = total, Page = pageResult, PageSize = pageSizeResult });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lịch sử đặt sân");
                return Unauthorized(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách sân yêu thích của người dùng.
        /// </summary>
        /// <param name="sort">Sắp xếp theo trường (FieldName:asc/desc).</param>
        /// <param name="page">Số trang (mặc định: 1).</param>
        /// <param name="pageSize">Số mục mỗi trang (mặc định: 10).</param>
        /// <returns>Danh sách sân yêu thích.</returns>
        /// <response code="200">Trả về danh sách sân yêu thích.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        [HttpGet("favorites")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
                    return BadRequest(new { Error = "Tham số phân trang không hợp lệ" });
                }

                var (data, total, pageResult, pageSizeResult) = await _userService.GetFavoriteFieldsAsync(User, sort, page, pageSize);
                _logger.LogInformation("Lấy danh sách sân yêu thích thành công");
                return Ok(new { Data = data, Total = total, Page = pageResult, PageSize = pageSizeResult });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách sân yêu thích");
                return Unauthorized(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Thêm sân vào danh sách yêu thích.
        /// </summary>
        /// <param name="addFavoriteFieldDto">Thông tin sân cần thêm.</param>
        /// <returns>Thông báo thêm sân thành công và ID của mục yêu thích.</returns>
        /// <response code="201">Thêm sân thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ hoặc sân đã có trong danh sách.</response>
        /// <response code="404">Sân không tồn tại.</response>
        [HttpPost("favorites")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddFavoriteField([FromBody] AddFavoriteFieldDto addFavoriteFieldDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi thêm sân yêu thích: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                var favoriteId = await _userService.AddFavoriteFieldAsync(User, addFavoriteFieldDto);
                _logger.LogInformation("Thêm sân yêu thích thành công, FavoriteId: {FavoriteId}", favoriteId);
                return StatusCode(StatusCodes.Status201Created, new { Message = "Thêm sân vào danh sách yêu thích thành công", FavoriteId = favoriteId });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm sân yêu thích");
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi thêm sân yêu thích");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Lỗi hệ thống khi thêm sân yêu thích." });
            }
        }

        /// <summary>
        /// Xóa sân khỏi danh sách yêu thích.
        /// </summary>
        /// <param name="fieldId">ID của sân cần xóa.</param>
        /// <returns>Thông báo xóa sân thành công.</returns>
        /// <response code="200">Xóa sân thành công.</response>
        /// <response code="404">Sân không có trong danh sách yêu thích.</response>
        [HttpDelete("favorites/{fieldId}")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveFavoriteField(int fieldId)
        {
            try
            {
                await _userService.RemoveFavoriteFieldAsync(User, fieldId);
                _logger.LogInformation("Xóa sân yêu thích thành công, FieldId: {FieldId}", fieldId);
                return Ok(new { Message = "Xóa sân khỏi danh sách yêu thích thành công" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sân yêu thích");
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi xóa sân yêu thích");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Lỗi hệ thống khi xóa sân yêu thích." });
            }
        }

        /// <summary>
        /// Lấy danh sách đánh giá của người dùng.
        /// </summary>
        /// <param name="sort">Sắp xếp theo trường (CreatedAt:asc/desc).</param>
        /// <param name="page">Số trang (mặc định: 1).</param>
        /// <param name="pageSize">Số mục mỗi trang (mặc định: 10).</param>
        /// <returns>Danh sách đánh giá.</returns>
        /// <response code="200">Trả về danh sách đánh giá.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        [HttpGet("reviews")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
                    return BadRequest(new { Error = "Tham số phân trang không hợp lệ" });
                }

                var (data, total, pageResult, pageSizeResult) = await _userService.GetUserReviewsAsync(User, sort, page, pageSize);
                _logger.LogInformation("Lấy danh sách đánh giá thành công");
                return Ok(new { Data = data, Total = total, Page = pageResult, PageSize = pageSizeResult });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách đánh giá");
                return Unauthorized(new { Error = ex.Message });
            }
        }
    }
}