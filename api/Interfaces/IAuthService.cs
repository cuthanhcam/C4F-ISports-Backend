using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Auth;

namespace api.Interfaces
{
    public interface IAuthService
    {
        Task<(string Token, string RefreshToken)> RegisterAsync(RegisterDto registerDto);
        Task<(string Token, string RefreshToken)> LoginAsync(LoginDto loginDto);
        Task<(string Token, string RefreshToken)> RefreshTokenAsync(string refreshToken);
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task LogoutAsync(string refreshToken);
        Task<bool> VerifyTokenAsync(string token);
        Task<bool> VerifyEmailAsync(string email, string token);
    }
}