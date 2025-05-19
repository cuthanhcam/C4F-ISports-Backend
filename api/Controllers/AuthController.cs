using api.Dtos.Auth;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace api.Controllers
{
    /// <summary>
    /// Controller xử lý các yêu cầu xác thực và quản lý người dùng.
    /// </summary>
    [Route("api/auth")]
    [ApiController]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Đăng ký tài khoản mới (User hoặc Owner).
        /// </summary>
        /// <param name="registerDto">Thông tin đăng ký (Email, Password, Role, FullName, Phone).</param>
        /// <returns>JWT và Refresh Token.</returns>
        /// <response code="200">Đăng ký thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ hoặc email đã tồn tại.</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for Register: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                var (token, refreshToken) = await _authService.RegisterAsync(registerDto);
                _logger.LogInformation("User registered: {Email}", registerDto.Email);
                return Ok(new { Token = token, RefreshToken = refreshToken, Message = "Đăng ký thành công. Vui lòng xác minh email." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Email}", registerDto.Email);
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Đăng nhập vào hệ thống.
        /// </summary>
        /// <param name="loginDto">Thông tin đăng nhập (Email, Password).</param>
        /// <returns>JWT và Refresh Token.</returns>
        /// <response code="200">Đăng nhập thành công.</response>
        /// <response code="400">Email hoặc mật khẩu không đúng.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for Login: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                var (token, refreshToken) = await _authService.LoginAsync(loginDto);
                _logger.LogInformation("User logged in: {Email}", loginDto.Email);
                return Ok(new { Token = token, RefreshToken = refreshToken, ExpiresIn = 3600 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", loginDto.Email);
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Làm mới JWT bằng Refresh Token.
        /// </summary>
        /// <param name="refreshTokenDto">Refresh Token hiện tại.</param>
        /// <returns>JWT mới và Refresh Token mới.</returns>
        /// <response code="200">Làm mới token thành công.</response>
        /// <response code="400">Refresh Token không hợp lệ.</response>
        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for RefreshToken");
                    return BadRequest(ModelState);
                }

                var (token, newRefreshToken) = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
                _logger.LogInformation("Token refreshed successfully");
                return Ok(new { Token = token, RefreshToken = newRefreshToken, ExpiresIn = 3600 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Yêu cầu đặt lại mật khẩu.
        /// </summary>
        /// <param name="forgotPasswordDto">Email của tài khoản.</param>
        /// <returns>Thông báo gửi link đặt lại mật khẩu.</returns>
        /// <response code="200">Link đặt lại mật khẩu đã được gửi.</response>
        /// <response code="400">Email không hợp lệ.</response>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for ForgotPassword: {Email}", forgotPasswordDto.Email);
                    return BadRequest(ModelState);
                }

                await _authService.ForgotPasswordAsync(forgotPasswordDto.Email);
                _logger.LogInformation("Password reset link sent to {Email}", forgotPasswordDto.Email);
                return Ok(new { Message = "Link đặt lại mật khẩu đã được gửi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password for {Email}", forgotPasswordDto.Email);
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Đặt lại mật khẩu bằng token.
        /// </summary>
        /// <param name="resetPasswordDto">Email, Token, và mật khẩu mới.</param>
        /// <returns>Thông báo đặt lại mật khẩu thành công.</returns>
        /// <response code="200">Mật khẩu đã được đặt lại.</response>
        /// <response code="400">Token không hợp lệ.</response>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for ResetPassword: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                await _authService.ResetPasswordAsync(resetPasswordDto);
                _logger.LogInformation("Password reset for {Email}", resetPasswordDto.Email);
                return Ok(new { Message = "Mật khẩu đã được đặt lại thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset for {Email}", resetPasswordDto.Email);
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Đăng xuất bằng cách hủy Refresh Token.
        /// </summary>
        /// <param name="refreshTokenDto">Refresh Token cần hủy.</param>
        /// <returns>Thông báo đăng xuất thành công.</returns>
        /// <response code="200">Đăng xuất thành công.</response>
        /// <response code="400">Refresh Token không hợp lệ.</response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for Logout");
                    return BadRequest(ModelState);
                }

                await _authService.LogoutAsync(refreshTokenDto.RefreshToken);
                _logger.LogInformation("User logged out");
                return Ok(new { Message = "Đăng xuất thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của JWT.
        /// </summary>
        /// <param name="token">JWT cần kiểm tra.</param>
        /// <returns>Kết quả kiểm tra (IsValid: true/false).</returns>
        /// <response code="200">Kết quả kiểm tra token.</response>
        /// <response code="400">Token không hợp lệ.</response>
        [HttpGet("verify-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyToken([FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Empty token provided for VerifyToken");
                    return BadRequest(new { Error = "Token không hợp lệ" });
                }

                var isValid = await _authService.VerifyTokenAsync(token);
                _logger.LogInformation("Token verification result: {IsValid}", isValid);
                return Ok(new { IsValid = isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token verification");
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Xác minh email bằng email và token.
        /// </summary>
        /// <param name="email">Email của tài khoản.</param>
        /// <param name="token">Token xác minh.</param>
        /// <returns>Chuyển hướng đến frontend với trạng thái xác minh.</returns>
        /// <response code="302">Chuyển hướng đến frontend.</response>
        [HttpGet("verify-email")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || !new EmailAddressAttribute().IsValid(email) || string.IsNullOrEmpty(token))
                {
                    string errorMessage = Uri.EscapeDataString("Email hoặc token không hợp lệ");
                    _logger.LogWarning("Invalid email or token for VerifyEmail: {Email}", email);
                    return Redirect($"{_configuration["FEUrl"]}/auth/verify-email?status=error&message={errorMessage}");
                }

                var result = await _authService.VerifyEmailAsync(email, token);
                if (!result)
                {
                    string failMessage = Uri.EscapeDataString("Xác thực email thất bại");
                    _logger.LogWarning("Email verification failed for {Email}", email);
                    return Redirect($"{_configuration["FEUrl"]}/auth/verify-email?status=error&message={failMessage}");
                }

                string successMessage = Uri.EscapeDataString("Email đã được xác thực thành công");
                _logger.LogInformation("Email verified for {Email}", email);
                return Redirect($"{_configuration["FEUrl"]}/auth/verify-email?status=success&message={successMessage}");
            }
            catch (Exception ex)
            {
                string exceptionMessage = Uri.EscapeDataString($"Lỗi: {ex.Message}");
                _logger.LogError(ex, "Error during email verification for {Email}", email);
                return Redirect($"{_configuration["FEUrl"]}/auth/verify-email?status=error&message={exceptionMessage}");
            }
        }

        /// <summary>
        /// Gửi lại email xác minh.
        /// </summary>
        /// <param name="forgotPasswordDto">Email của tài khoản.</param>
        /// <returns>Thông báo gửi email xác minh.</returns>
        /// <response code="200">Email xác minh đã được gửi lại.</response>
        /// <response code="400">Email không hợp lệ.</response>
        [HttpPost("resend-verification")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendVerificationEmail([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for ResendVerificationEmail: {Email}", forgotPasswordDto.Email);
                    return BadRequest(ModelState);
                }

                await _authService.ResendVerificationEmailAsync(forgotPasswordDto.Email);
                _logger.LogInformation("Verification email resent to {Email}", forgotPasswordDto.Email);
                return Ok(new { Message = "Email xác thực đã được gửi lại." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during resend verification email for {Email}", forgotPasswordDto.Email);
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin người dùng hiện tại.
        /// </summary>
        /// <returns>Thông tin người dùng (Email, Role, FullName, Phone).</returns>
        /// <response code="200">Thông tin người dùng.</response>
        /// <response code="401">Chưa đăng nhập.</response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync(User);
                _logger.LogInformation("Current user retrieved: {Email}", user.Email);
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
                _logger.LogError(ex, "Error retrieving current user");
                return Unauthorized(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Thay đổi mật khẩu của người dùng hiện tại.
        /// </summary>
        /// <param name="changePasswordDto">Mật khẩu cũ và mới.</param>
        /// <returns>Thông báo thay đổi mật khẩu thành công.</returns>
        /// <response code="200">Mật khẩu đã được thay đổi.</response>
        /// <response code="400">Mật khẩu cũ không đúng hoặc dữ liệu không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for ChangePassword: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                await _authService.ChangePasswordAsync(User, changePasswordDto);
                _logger.LogInformation("Password changed successfully");
                return Ok(new { Message = "Mật khẩu đã được thay đổi thành công." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid old password");
                return BadRequest(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access during password change");
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change");
                return BadRequest(new { Error = "Đã xảy ra lỗi khi thay đổi mật khẩu." });
            }
        }
    }

    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public required string Email { get; set; }
    }
}