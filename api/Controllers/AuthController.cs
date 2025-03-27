using api.Dtos.Auth;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;

        public AuthController(IConfiguration configuration, IAuthService authService)
        {
            _configuration = configuration;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var (token, refreshToken) = await _authService.RegisterAsync(registerDto);
                return Ok(new { Token = token, RefreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var (token, refreshToken) = await _authService.LoginAsync(loginDto);
                return Ok(new { Token = token, RefreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var (token, newRefreshToken) = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
                return Ok(new { Token = token, RefreshToken = newRefreshToken });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Email) || !model.Email.Contains("@"))
                {
                    return BadRequest(new { Error = "Email không hợp lệ" });
                }

                await _authService.ForgotPasswordAsync(model.Email);
                return Ok(new {Mesasge = "Link đặt lại mật khẩu đã được gửi đến email của bạn." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _authService.ResetPasswordAsync(resetPasswordDto);
                return Ok("Mật khẩu đã được đặt lại thành công.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _authService.LogoutAsync(refreshTokenDto.RefreshToken);
                return Ok("Đăng xuất thành công.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("verify-token")]
        public async Task<IActionResult> VerifyToken([FromQuery] string email, [FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new { Error = "Token không hợp lệ" });
                }

                var isValid = await _authService.VerifyTokenAsync(token);
                return Ok(new { IsValid = isValid });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
        {
             try
            {
                if (string.IsNullOrEmpty(email) || !email.Contains("@") || string.IsNullOrEmpty(token))
                {
                    string errorMessage = Uri.EscapeDataString("Email hoặc token không hợp lệ");
                    return Redirect($"{_configuration["FEUrl"]}/auth/verify-email?status=error&message={errorMessage}");
                }


                var result = await _authService.VerifyEmailAsync(email, token);

                if (!result)
                {
                    string failMessage = Uri.EscapeDataString("Xác thực email thất bại");
                    return Redirect($"{_configuration["FEUrl"]}/auth/verify-email?status=error&message={failMessage}");
                }

                string successMessage = Uri.EscapeDataString("Email đã được xác thực thành công");
                return Redirect($"{_configuration["FEUrl"]}/auth/verify-email?status=success&message={successMessage}");
            }
            catch (Exception ex)
            {
                string exceptionMessage = Uri.EscapeDataString($"Lỗi: {ex.Message}");
                return Redirect($"{_configuration["FEUrl"]}/auth/verify-email?status=error&message={exceptionMessage}");
            }

        }

        [HttpPost("resend-verification")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ResendVerificationEmail([FromBody] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                {
                    return BadRequest(new { Error = "Email không hợp lệ" });
                }

                await _authService.ResendVerificationEmailAsync(email);
                return Ok(new { Message = "Email xác thực đã được gửi lại. Vui lòng kiểm tra hộp thư của bạn." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync(User);
                return Ok(new
                {
                    Email = user.Email,
                    Role = user.Role,
                    FullName = user.User?.FullName ?? user.Owner?.FullName,
                    Phone = user.User?.Phone ?? user.Owner?.Phone
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _authService.ChangePasswordAsync(User, changePasswordDto);
                return Ok("Mật khẩu đã được thay đổi thành công.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}