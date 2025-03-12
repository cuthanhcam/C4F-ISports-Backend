using api.Dtos.Auth;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
            var (token, refreshToken) = await _authService.RegisterAsync(registerDto);
            return Ok(new { Token = token, RefreshToken = refreshToken });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var (token, refreshToken) = await _authService.LoginAsync(loginDto);
            return Ok(new { Token = token, RefreshToken = refreshToken });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var (token, newRefreshToken) = await _authService.RefreshTokenAsync(refreshToken);
            return Ok(new { Token = token, RefreshToken = newRefreshToken });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromQuery] string email)
        {
            await _authService.ForgotPasswordAsync(email);
            return Ok("Password reset link has been sent to your email.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            await _authService.ResetPasswordAsync(resetPasswordDto);
            return Ok("Password has been reset successfully.");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            await _authService.LogoutAsync(refreshToken);
            return Ok("Logged out successfully.");
        }

        [HttpGet("verify-token")]
        public async Task<IActionResult> VerifyToken([FromQuery] string token)
        {
            var isValid = await _authService.VerifyTokenAsync(token);
            return Ok(new { IsValid = isValid });
        }
    }
}