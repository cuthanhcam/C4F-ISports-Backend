using api.Dtos.Auth;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
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
                var (token, refreshToken) = await _authService.LoginAsync(loginDto);
                return Ok(new { Token = token, RefreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            try
            {
                var (token, newRefreshToken) = await _authService.RefreshTokenAsync(refreshToken);
                return Ok(new { Token = token, RefreshToken = newRefreshToken });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromQuery] string email)
        {
            try
            {
                await _authService.ForgotPasswordAsync(email);
                return Ok("Password reset link has been sent to your email.");
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
                await _authService.ResetPasswordAsync(resetPasswordDto);
                return Ok("Password has been reset successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            try
            {
                await _authService.LogoutAsync(refreshToken);
                return Ok("Logged out successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("verify-token")]
        public async Task<IActionResult> VerifyToken([FromQuery] string token)
        {
            try
            {
                var isValid = await _authService.VerifyTokenAsync(token);
                return Ok(new { IsValid = isValid });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
        {
            try
            {
                var result = await _authService.VerifyEmailAsync(email, token);
                return Ok(new { Success = true, Message = "Email verified" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}