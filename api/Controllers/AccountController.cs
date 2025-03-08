using Microsoft.AspNetCore.Mvc;
using api.Dtos.Account;
using api.Interfaces;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var userDto = await _authService.Login(loginDto);

            if (userDto == null)
            {
                return Unauthorized("Email/số điện thoại hoặc mật khẩu không đúng.");
            }

            return Ok(userDto);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var userDto = await _authService.Register(registerDto);

            if (userDto == null)
            {
                return BadRequest("Email hoặc số điện thoại đã tồn tại.");
            }

            return Ok(userDto);
        }
    }
}