using System.Threading.Tasks;
using api.Dtos.Account;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;

        public AuthService(UserManager<AppUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<UserDto> Register(RegisterDto registerDto)
        {
            // Kiểm tra email và số điện thoại đã tồn tại chưa
            var emailExists = await _userManager.FindByEmailAsync(registerDto.Email);
            var phoneExists = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == registerDto.PhoneNumber);

            if (emailExists != null || phoneExists != null)
            {
                return null; // Email hoặc số điện thoại đã tồn tại
            }

            var user = new AppUser
            {
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                UserName = registerDto.Email // Sử dụng email làm username
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return null; // Đăng ký thất bại
            }

            var token = await _tokenService.CreateToken(user);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Token = token
            };
        }

        public async Task<UserDto> Login(LoginDto loginDto)
        {
            // Tìm user bằng email hoặc số điện thoại
            var user = await _userManager.FindByEmailAsync(loginDto.EmailOrPhone) ??
                       await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == loginDto.EmailOrPhone);

            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return null; // Đăng nhập thất bại
            }

            var token = await _tokenService.CreateToken(user);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Token = token
            };
        }
    }
}