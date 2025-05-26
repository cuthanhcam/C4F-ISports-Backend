using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos.Auth;
using api.Models;

namespace api.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Đăng ký tài khoản mới cho người dùng.
        /// </summary>
        /// <param name="registerDto">Thông tin đăng ký tài khoản.</param>
        /// <returns>ID tài khoản, token và refresh token.</returns>
        Task<(int AccountId, string Token, string RefreshToken)> RegisterAsync(RegisterDto registerDto);

        /// <summary>
        /// Đăng nhập vào hệ thống.
        /// </summary>
        /// <param name="loginDto">Thông tin đăng nhập.</param>
        /// <returns>Token, refresh token và vai trò của người dùng.</returns>
        Task<(string Token, string RefreshToken, string Role)> LoginAsync(LoginDto loginDto);

        /// <summary>
        /// Làm mới token truy cập bằng refresh token.
        /// </summary>
        /// <param name="refreshToken">Refresh token hiện tại.</param>
        /// <returns>Token mới và refresh token mới.</returns>
        Task<(string Token, string RefreshToken)> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Gửi email đặt lại mật khẩu khi người dùng quên mật khẩu.
        /// </summary>
        /// <param name="email">Email của tài khoản cần đặt lại mật khẩu.</param>
        Task ForgotPasswordAsync(string email);

        /// <summary>
        /// Đặt lại mật khẩu cho tài khoản.
        /// </summary>
        /// <param name="resetPasswordDto">Thông tin đặt lại mật khẩu bao gồm token và mật khẩu mới.</param>
        Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto);

        /// <summary>
        /// Đăng xuất khỏi hệ thống.
        /// </summary>
        /// <param name="refreshToken">Refresh token cần vô hiệu hóa.</param>
        Task LogoutAsync(string refreshToken);

        /// <summary>
        /// Lấy thông tin tài khoản của người dùng hiện tại.
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        /// <returns>Thông tin tài khoản.</returns>
        Task<Account> GetCurrentUserAsync(ClaimsPrincipal user);

        /// <summary>
        /// Thay đổi mật khẩu của người dùng hiện tại.
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        /// <param name="changePasswordDto">Thông tin mật khẩu cũ và mật khẩu mới.</param>
        Task ChangePasswordAsync(ClaimsPrincipal user, ChangePasswordDto changePasswordDto);

        /// <summary>
        /// Xác thực email của người dùng.
        /// </summary>
        /// <param name="email">Email cần xác thực.</param>
        /// <param name="token">Token xác thực.</param>
        /// <returns>Kết quả xác thực (true nếu thành công).</returns>
        Task<bool> VerifyEmailAsync(string email, string token);

        /// <summary>
        /// Gửi lại email xác thực cho người dùng.
        /// </summary>
        /// <param name="email">Email cần gửi lại xác thực.</param>
        Task ResendVerificationEmailAsync(string email);

        /// <summary>
        /// Kiểm tra tính hợp lệ của token truy cập.
        /// </summary>
        /// <param name="token">Token cần kiểm tra.</param>
        /// <returns>Kết quả kiểm tra (IsValid) và vai trò của người dùng (Role).</returns>
        Task<(bool IsValid, string Role)> VerifyTokenAsync(string token);

        /// <summary>
        /// Khôi phục tài khoản đã bị xóa.
        /// </summary>
        /// <param name="email">Email của tài khoản cần khôi phục.</param>
        /// <param name="token">Token xác thực để khôi phục tài khoản.</param>
        /// <returns>Không có giá trị trả về.</returns>
        Task RestoreAccountAsync(string email, string token);
    }
}