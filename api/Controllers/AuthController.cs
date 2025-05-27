using api.Dtos.Auth;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IConfiguration configuration, IUnitOfWork unitOfWork, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _authService = authService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Đăng ký tài khoản mới (User hoặc Owner).
        /// </summary>
        /// <param name="registerDto">Thông tin đăng ký (Email, Password, Role, FullName, Phone).</param>
        /// <returns>JWT và Refresh Token.</returns>
        /// <response code="201">Đăng ký thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="409">Email đã được đăng ký.</response>
        /// <response code="500">Lỗi hệ thống khi gửi email xác minh.</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for Register: {Errors}", ModelState);
                    return BadRequest(new { error = "Dữ liệu đầu vào không hợp lệ", details = ModelState });
                }

                var (accountId, token, refreshToken) = await _authService.RegisterAsync(registerDto);
                _logger.LogInformation("Account created: {Email}", registerDto.Email);
                return StatusCode(StatusCodes.Status201Created, new
                {
                    AccountId = accountId,
                    Token = token,
                    RefreshToken = refreshToken,
                    Message = "Tài khoản đã được đăng ký thành công. Vui lòng xác minh email của bạn."
                });
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Email đã tồn tại"))
            {
                _logger.LogWarning(ex, "Email already exists: {Email}", registerDto.Email);
                return Conflict(new { error = "Email đã được đăng ký", details = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error during registration: {Email}", registerDto.Email);
                return BadRequest(new { error = "Dữ liệu đầu vào không hợp lệ", details = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Failed to send email"))
            {
                _logger.LogError(ex, "Email sending failed for {Email}", registerDto.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Lỗi hệ thống", details = "Không thể gửi email xác minh." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration: {Email}", registerDto.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Lỗi hệ thống", details = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        /// <summary>
        /// Đăng nhập vào hệ thống.
        /// </summary>
        /// <param name="loginDto">Thông tin đăng nhập (Email, Password).</param>
        /// <returns>JWT và Refresh Token.</returns>
        /// <response code="200">Đăng nhập thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Email hoặc mật khẩu không đúng.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for Login: {Email}", loginDto.Email);
                    return BadRequest(new { error = "Dữ liệu đầu vào không hợp lệ", details = ModelState });
                }

                var (token, refreshToken, role) = await _authService.LoginAsync(loginDto);
                _logger.LogInformation("User logged in: {Email}", loginDto.Email);
                int expiresIn = int.Parse(_configuration["JwtSettings:TokenExpiryMinutes"] ?? throw new InvalidOperationException("TokenExpiryMinutes chưa được cấu hình.")) * 60;
                return Ok(new
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresIn = expiresIn,
                    Role = role,
                    Message = "Đăng nhập thành công"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized login attempt for {Email}", loginDto.Email);
                return Unauthorized(new { error = "Không được phép", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login: {Email}", loginDto.Email);
                return BadRequest(new { error = "Dữ liệu đầu vào không hợp lệ", details = ex.Message });
            }
        }

        /// <summary>
        /// Làm mới JWT bằng Refresh Token.
        /// </summary>
        /// <param name="refreshTokenDto">Refresh Token hiện tại.</param>
        /// <returns>JWT mới và Refresh Token mới.</returns>
        /// <response code="200">Token đã được làm mới thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Refresh Token đã hết hạn hoặc không hợp lệ.</response>
        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for RefreshToken");
                    return BadRequest(new { error = "Dữ liệu đầu vào không hợp lệ", details = ModelState });
                }

                var (token, newRefreshToken) = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
                _logger.LogInformation("Token refreshed successfully");
                int expiresIn = int.Parse(_configuration["JwtSettings:TokenExpiryMinutes"] ?? throw new InvalidOperationException("TokenExpiryMinutes chưa được cấu hình.")) * 60;
                return Ok(new
                {
                    Token = token,
                    RefreshToken = newRefreshToken,
                    ExpiresIn = expiresIn,
                    Message = "Token đã được làm mới thành công"
                });
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Invalid or expired refresh token");
                return Unauthorized(new { error = "Không được phép", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return BadRequest(new { error = "Dữ liệu đầu vào không hợp lệ", details = ex.Message });
            }
        }

        /// <summary>
        /// Yêu cầu đặt lại mật khẩu.
        /// </summary>
        /// <param name="forgotPasswordDto">Email của tài khoản.</param>
        /// <returns>Thông báo gửi link đặt lại mật khẩu.</returns>
        /// <response code="200">Email đặt lại mật khẩu đã được gửi.</response>
        /// <response code="400">Email không hợp lệ.</response>
        /// <response code="404">Email không tồn tại trong hệ thống.</response>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for ForgotPassword: {Email}", forgotPasswordDto.Email);
                    return BadRequest(new { error = "Dữ liệu đầu vào không hợp lệ", details = ModelState });
                }

                await _authService.ForgotPasswordAsync(forgotPasswordDto.Email);
                _logger.LogInformation("Password reset link sent to {Email}", forgotPasswordDto.Email);
                return Ok(new { Message = "Email đặt lại mật khẩu đã được gửi" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Email not found for ForgotPassword: {Email}", forgotPasswordDto.Email);
                return NotFound(new { error = "Email không tồn tại", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password for {Email}", forgotPasswordDto.Email);
                return BadRequest(new { error = "Dữ liệu đầu vào không hợp lệ", details = ex.Message });
            }
        }

        /// <summary>
        /// Đặt lại mật khẩu bằng token.
        /// </summary>
        /// <param name="resetPasswordDto">Email, Token, và mật khẩu mới.</param>
        /// <returns>Thông báo đặt lại mật khẩu thành công.</returns>
        /// <response code="200">Mật khẩu đặt lại thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="404">Token không hợp lệ hoặc đã hết hạn.</response>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for ResetPassword");
                    return BadRequest(new { error = "Dữ liệu đầu vào không hợp lệ", details = ModelState });
                }

                await _authService.ResetPasswordAsync(resetPasswordDto);
                _logger.LogInformation("Password reset for {Email}", resetPasswordDto.Email);
                return Ok(new { Message = "Mật khẩu đã được đặt lại thành công" });
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Invalid or expired reset token for {Email}", resetPasswordDto.Email);
                return NotFound(new { error = "Token không hợp lệ hoặc đã hết hạn", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset for {Email}", resetPasswordDto.Email);
                return BadRequest(new { error = "Dữ liệu đầu vào không hợp lệ", details = ex.Message });
            }
        }

        /// <summary>
        /// Đăng xuất bằng cách hủy Refresh Token.
        /// </summary>
        /// <param name="refreshTokenDto">Refresh Token cần hủy.</param>
        /// <returns>Thông báo đăng xuất thành công.</returns>
        /// <response code="200">Đăng xuất thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Refresh Token không hợp lệ hoặc đã hết hạn.</response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for Logout");
                    return BadRequest(new { error = "Dữ liệu đầu vào không hợp lệ", details = ModelState });
                }

                await _authService.LogoutAsync(refreshTokenDto.RefreshToken);
                _logger.LogInformation("User logged out");
                return Ok(new { Message = "Đăng xuất thành công" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid refresh token for logout");
                return Unauthorized(new { error = "Không được phép", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return BadRequest(new { error = "Dữ liệu đầu vào không hợp lệ", details = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin người dùng hiện tại.
        /// </summary>
        /// <returns>Thông tin người dùng (Email, Role, FullName, Phone).</returns>
        /// <response code="200">Thông tin người dùng đã được lấy thành công.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var account = await _authService.GetCurrentUserAsync(User);
                object userInfo = null;
                if (account.Role == "User")
                {
                    userInfo = await _unitOfWork.Repository<User>().FindSingleAsync(u => u.AccountId == account.AccountId);
                    if (userInfo == null)
                        _logger.LogDebug("No User found for AccountId: {AccountId}", account.AccountId);
                }
                else if (account.Role == "Owner")
                {
                    userInfo = await _unitOfWork.Repository<Owner>().FindSingleAsync(o => o.AccountId == account.AccountId);
                    if (userInfo == null)
                        _logger.LogDebug("No Owner found for AccountId: {AccountId}", account.AccountId);
                }

                _logger.LogInformation("Current user retrieved: {Email}", account.Email);
                return Ok(new
                {
                    AccountId = account.AccountId,
                    Email = account.Email,
                    Role = account.Role,
                    FullName = userInfo switch
                    {
                        User u => u.FullName,
                        Owner o => o.FullName,
                        _ => null
                    },
                    Phone = userInfo switch
                    {
                        User u => u.Phone,
                        Owner o => o.Phone,
                        _ => null
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user");
                return Unauthorized(new { error = "Không được phép", details = ex.Message });
            }
        }

        /// <summary>
        /// Thay đổi mật khẩu của người dùng hiện tại.
        /// </summary>
        /// <param name="changePasswordDto">Mật khẩu cũ và mới.</param>
        /// <returns>Thông báo thay đổi mật khẩu thành công.</returns>
        /// <response code="200">Mật khẩu đã được thay đổi thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Mật khẩu cũ không đúng hoặc chưa đăng nhập.</response>
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
                    _logger.LogWarning("Invalid model state: {Errors}", ModelState);
                    return BadRequest(new { error = "Dữ liệu đầu vào không hợp lệ", details = ModelState });
                }

                await _authService.ChangePasswordAsync(User, changePasswordDto);
                _logger.LogInformation("Password updated successfully");
                return Ok(new { Message = "Mật khẩu đã được thay đổi thành công" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid current password");
                return BadRequest(new { error = "Dữ liệu đầu vào không hợp lệ", details = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access during password change");
                return Unauthorized(new { error = "Không được phép", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change");
                return BadRequest(new { error = "Yêu cầu không hợp lệ", details = ex.Message });
            }
        }

        /// <summary>
        /// Xác minh email bằng email và token.
        /// </summary>
        /// <param name="email">Email của tài khoản.</param>
        /// <param name="token">Token xác minh.</param>
        /// <returns>Chuyển hướng đến frontend với trạng thái xác minh hoặc JSON nếu gọi qua API.</returns>
        /// <response code="200">Email xác minh thành công (JSON).</response>
        /// <response code="400">Email hoặc token không hợp lệ (JSON).</response>
        /// <response code="302">Chuyển hướng đến trang xác minh email.</response>
        [HttpGet("verify-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || !new EmailAddressAttribute().IsValid(email) || string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Invalid email or token for VerifyEmail: {Email}", email);
                    if (Request.Headers["Accept"].ToString().Contains("application/json"))
                    {
                        return BadRequest(new { error = "Email hoặc token không hợp lệ", details = "Vui lòng kiểm tra email và token." });
                    }
                    string errorMessage = Uri.EscapeDataString("Email hoặc token không hợp lệ");
                    return Redirect($"{_configuration["FEUrl"]}/auth/verify-email?status=error&message={errorMessage}");
                }

                var result = await _authService.VerifyEmailAsync(email, token);
                if (Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    if (!result)
                    {
                        _logger.LogWarning("Email verification failed for {Email}", email);
                        return BadRequest(new { error = "Xác minh email thất bại", details = "Token không hợp lệ hoặc đã hết hạn" });
                    }
                    _logger.LogInformation("Email verified successfully for {Email}", email);
                    return Ok(new { message = "Email đã được xác minh thành công" });
                }

                if (!result)
                {
                    string errorMessage = Uri.EscapeDataString("Xác minh email thất bại");
                    _logger.LogWarning("Email verification failed for {Email}", email);
                    return Redirect($"{_configuration["FEUrl"]}/auth/verify-email?status=failed&message={errorMessage}");
                }

                string successMessage = Uri.EscapeDataString("Email đã được xác minh thành công");
                _logger.LogInformation("Email verified successfully for {Email}", email);
                return Redirect($"{_configuration["FEUrl"]}/auth/verified?status=success&message={successMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification for {Email}", email);
                if (Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Lỗi hệ thống", details = ex.Message });
                }
                string errorMessage = Uri.EscapeDataString($"Lỗi: {ex.Message}");
                return Redirect($"{_configuration["FEUrl"]}/auth/failed?status=error&message={errorMessage}");
            }
        }

        /// <summary>
        /// Gửi lại email xác minh.
        /// </summary>
        /// <param name="forgotPasswordDto">Email của tài khoản.</param>
        /// <returns>Thông báo gửi email xác minh.</returns>
        /// <response code="200">Email xác minh đã được gửi.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="404">Email không tồn tại trong hệ thống.</response>
        [HttpPost("resend-verification-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResendVerificationEmail([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for ResendVerificationEmail: {Email}", forgotPasswordDto.Email);
                    return BadRequest(new { error = "Dữ liệu đầu vào không hợp lệ", details = ModelState });
                }

                await _authService.ResendVerificationEmailAsync(forgotPasswordDto.Email);
                _logger.LogInformation("Verification or restoration email sent to {Email}", forgotPasswordDto.Email);
                return Ok(new { Message = "Email xác minh hoặc khôi phục đã được gửi" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Email not found for ResendVerificationEmail: {Email}", forgotPasswordDto.Email);
                return NotFound(new { error = "Email không tồn tại", details = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Account already verified or not verifiable for ResendVerificationEmail: {Email}", forgotPasswordDto.Email);
                return BadRequest(new { error = "Tài khoản không thể xác minh/khôi phục", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during resend verification email for {Email}", forgotPasswordDto.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Lỗi hệ thống", details = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        [HttpPost("restore-account")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RestoreAccount([FromBody] RestoreAccountDto restoreAccountDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for RestoreAccount: {Email}", restoreAccountDto.Email);
                    return BadRequest(new { error = "Dữ liệu đầu vào không hợp lệ", details = ModelState });
                }

                await _authService.RestoreAccountAsync(restoreAccountDto.Email, restoreAccountDto.Token);
                _logger.LogInformation("Account restored successfully for {Email}", restoreAccountDto.Email);
                return Ok(new { Message = "Tài khoản đã được khôi phục thành công" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid restore attempt for {Email}", restoreAccountDto.Email);
                return BadRequest(new { error = "Yêu cầu không hợp lệ", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during account restoration for {Email}", restoreAccountDto.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Lỗi hệ thống", details = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của JWT.
        /// </summary>
        /// <param name="verifyTokenDto">JWT cần kiểm tra.</param>
        /// <returns>Kết quả kiểm tra (IsValid: true/false).</returns>
        /// <response code="200">Token hợp lệ.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Token không hợp lệ hoặc đã hết hạn.</response>
        [HttpPost("verify-token")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> VerifyToken([FromBody] VerifyTokenDto verifyTokenDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for VerifyToken");
                    return BadRequest(new { error = "Dữ liệu đầu vào không hợp lệ", details = ModelState });
                }

                var (isValid, role) = await _authService.VerifyTokenAsync(verifyTokenDto.Token);
                _logger.LogInformation("Token verification result: {IsValid}", isValid);
                return Ok(new
                {
                    IsValid = isValid,
                    Role = role,
                    Message = isValid ? "Token hợp lệ" : "Token không hợp lệ"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token verification");
                return Unauthorized(new { error = "Không được phép", details = ex.Message });
            }
        }
    }
}