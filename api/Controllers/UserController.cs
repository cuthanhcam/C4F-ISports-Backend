using System;
using System.Threading.Tasks;
using api.Dtos.User;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace api.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var user = await _userService.GetUserProfileAsync(User);
                return Ok(new
                {
                    Email = user.Email,
                    Role = user.Account.Role,
                    FullName = user.FullName,
                    Phone = user.Phone,
                    Gender = user.Gender,
                    DateOfBirth = user.DateOfBirth?.ToString("yyyy-MM-dd"),
                    AvatarUrl = user.AvatarUrl
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
        {
            try
            {
                await _userService.UpdateUserProfileAsync(User, updateProfileDto);
                return Ok(new { Success = true, Message = "Profile updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("bookings")]
        public async Task<IActionResult> GetBookings(
            [FromQuery] string status = null,
            [FromQuery] DateTime? date = null,
            [FromQuery] string sort = "BookingDate:desc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _userService.GetUserBookingsAsync(User, status, date, sort, page, pageSize);
                return Ok(new
                {
                    TotalItems = result.TotalItems,
                    Page = result.Page,
                    PageSize = result.PageSize,
                    Items = result.Items.Select(b => new
                    {
                        BookingId = b.BookingId,
                        FieldId = b.FieldId,
                        FieldName = b.Field.FieldName,
                        BookingDate = b.BookingDate.ToString("yyyy-MM-dd"),
                        StartTime = b.StartTime.ToString(@"hh\:mm"),
                        EndTime = b.EndTime.ToString(@"hh\:mm"),
                        TotalPrice = b.TotalPrice,
                        Status = b.Status,
                        PaymentStatus = b.PaymentStatus,
                        CreatedAt = b.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
                    })
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpDelete("profile")]
        public async Task<IActionResult> DeactivateProfile()
        {
            try
            {
                await _userService.DeactivateUserAsync(User);
                return Ok(new { Success = true, Message = "Account deactivated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("favorite-fields")]
        public async Task<IActionResult> GetFavoriteFields(
            [FromQuery] string sort = "AddedDate:desc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _userService.GetFavoriteFieldsAsync(User, sort, page, pageSize);
                return Ok(new
                {
                    TotalItems = result.TotalItems,
                    Page = result.Page,
                    PageSize = result.PageSize,
                    Items = result.Items.Select(ff => new
                    {
                        FieldId = ff.FieldId,
                        FieldName = ff.Field.FieldName,
                        SportType = ff.Field.Sport.SportName,
                        Location = ff.Field.Address,
                        Phone = ff.Field.Phone,
                        OpenHours = ff.Field.OpenHours,
                        AddedDate = ff.AddedDate.ToString("yyyy-MM-ddTHH:mm:ssZ")
                    })
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("favorite-fields/{fieldId}")]
        public async Task<IActionResult> AddFavoriteField(int fieldId)
        {
            try
            {
                await _userService.AddFavoriteFieldAsync(User, fieldId);
                return Ok(new { Success = true, Message = "Field added to favorites" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpDelete("favorite-fields/{fieldId}")]
        public async Task<IActionResult> RemoveFavoriteField(int fieldId)
        {
            try
            {
                await _userService.RemoveFavoriteFieldAsync(User, fieldId);
                return Ok(new { Success = true, Message = "Field removed from favorites" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}