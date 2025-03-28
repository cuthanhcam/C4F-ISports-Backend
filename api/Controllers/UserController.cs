using System;
using System.Threading.Tasks;
using api.Dtos.User;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System.Linq;
using api.Dtos;
using CloudinaryDotNet;
using api.Services;
using Microsoft.AspNetCore.RateLimiting;

namespace api.Controllers
{
    [Route("api/users")]
    [ApiController]
    [EnableRateLimiting("auth")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly CloudinaryService _cloudinaryService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, CloudinaryService cloudinaryService, ILogger<UserController> logger)
        {
            _userService = userService;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        [HttpGet("profile")]
        [ProducesResponseType(typeof(UserProfileResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                _logger.LogInformation("Getting user profile for user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                var profile = await _userService.GetUserProfileAsync(User);
                return Ok(profile); // Trả về UserProfileResponseDto trực tiếp
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile for user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return BadRequest(new { Error = ex.Message }); // Trả về thông báo lỗi chi tiết hơn
            }
        }

        [HttpPut("profile")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
        {
            try
            {
                if (updateProfileDto == null)
                {
                    return BadRequest(new { Error = "Dữ liệu cập nhật không hợp lệ" });
                }

                _logger.LogInformation("Updating user profile for user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _userService.UpdateUserProfileAsync(User, updateProfileDto);
                return Ok(new { Success = true, Message = "Cập nhật thông tin thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile for user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("bookings")]
        [ProducesResponseType(typeof(PaginatedResponse<BookingResponseDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetBookings(
            [FromQuery] string status = null,
            [FromQuery] DateTime? date = null,
            [FromQuery] string sort = "BookingDate:desc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    return BadRequest(new { Error = "Số trang và kích thước trang phải lớn hơn 0" });
                }

                _logger.LogInformation("Getting bookings for user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                var result = await _userService.GetUserBookingsAsync(User, status, date, sort, page, pageSize);
                var response = new PaginatedResponse<BookingResponseDto>
                {
                    TotalItems = result.TotalItems,
                    Page = result.Page,
                    PageSize = result.PageSize,
                    Items = result.Items.Select(b => new BookingResponseDto
                    {
                        BookingId = b.BookingId,
                        FieldId = b.SubField.Field.FieldId, // Lấy FieldId từ SubField
                        FieldName = b.SubField.Field.FieldName, // Lấy FieldName từ SubField
                        SubFieldId = b.SubFieldId, // Thêm SubFieldId vào DTO
                        SubFieldName = b.SubField.SubFieldName, // Thêm SubFieldName vào DTO
                        BookingDate = b.BookingDate,
                        StartTime = b.StartTime,
                        EndTime = b.EndTime,
                        Status = b.Status,
                        PaymentStatus = b.PaymentStatus,
                        CreatedAt = b.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
                    }).ToList()
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpDelete("profile")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> DeactivateProfile()
        {
            try
            {
                _logger.LogInformation("Deactivating profile for user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _userService.DeactivateUserAsync(User);
                return Ok(new { Success = true, Message = "Xóa tài khoản thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating profile for user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("favorite-fields")]
        [ProducesResponseType(typeof(PaginatedResponse<FavoriteFieldResponseDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetFavoriteFields(
            [FromQuery] string sort = "AddedDate:desc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    return BadRequest(new { Error = "Số trang và kích thước trang phải lớn hơn 0" });
                }

                _logger.LogInformation("Getting favorite fields for user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                var result = await _userService.GetFavoriteFieldsAsync(User, sort, page, pageSize);
                var response = new PaginatedResponse<FavoriteFieldResponseDto>
                {
                    TotalItems = result.TotalItems,
                    Page = result.Page,
                    PageSize = result.PageSize,
                    Items = result.Items.Select(ff => new FavoriteFieldResponseDto
                    {
                        FieldId = ff.FieldId,
                        FieldName = ff.Field.FieldName,
                        SportType = ff.Field.Sport.SportName,
                        Location = ff.Field.Address,
                        Phone = ff.Field.Phone,
                        OpenHours = ff.Field.OpenHours,
                        AddedDate = ff.AddedDate.ToString("yyyy-MM-ddTHH:mm:ssZ")
                    }).ToList()
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorite fields for user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("favorite-fields/{fieldId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AddFavoriteField(int fieldId)
        {
            try
            {
                if (fieldId <= 0)
                {
                    return BadRequest(new { Error = "ID sân không hợp lệ" });
                }

                _logger.LogInformation("Adding field {FieldId} to favorites for user: {UserId}", fieldId, User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _userService.AddFavoriteFieldAsync(User, fieldId);
                return Ok(new { Success = true, Message = "Đã thêm sân vào danh sách yêu thích" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding field {FieldId} to favorites for user: {UserId}", fieldId, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpDelete("favorite-fields/{fieldId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RemoveFavoriteField(int fieldId)
        {
            try
            {
                if (fieldId <= 0)
                {
                    return BadRequest(new { Error = "ID sân không hợp lệ" });
                }

                _logger.LogInformation("Removing field {FieldId} from favorites for user: {UserId}", fieldId, User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _userService.RemoveFavoriteFieldAsync(User, fieldId);
                return Ok(new { Success = true, Message = "Đã xóa sân khỏi danh sách yêu thích" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing field {FieldId} from favorites for user: {UserId}", fieldId, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}