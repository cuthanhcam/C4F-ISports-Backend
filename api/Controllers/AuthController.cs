using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Google;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Khởi động đăng nhập Google OAuth2
        /// </summary>
        /// <returns>Chuyển hướng đến trang đăng nhập Google</returns>
        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            _logger.LogInformation("Starting Google OAuth2 login...");
            var props = new AuthenticationProperties { RedirectUri = "/api/auth/google-callback" };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Xử lý callback từ Google OAuth2
        /// </summary>
        /// <returns>Thông tin người dùng hoặc lỗi nếu thất bại</returns>
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                _logger.LogError("Google authentication failed.");
                return Unauthorized("Google authentication failed.");
            }

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;

            _logger.LogInformation("Google authentication successful for {Email}.", email);

            // TODO: Tạo hoặc cập nhật user trong DB
            // TODO: Sinh JWT token
            var token = "fake-jwt-token"; // Thay bằng hàm sinh token thực tế
            return Ok(new { Token = token, Email = email, Name = name });
        }
    }
}