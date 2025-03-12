using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Auth;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _emailSender = emailSender;
        }

        public async Task<(string Token, string RefreshToken)> RegisterAsync(RegisterDto registerDto)
        {
            if (_unitOfWork.Accounts.GetAll().Any(a => a.Email == registerDto.Email))
            {
                throw new Exception("Email already exists.");
            }

            var account = new Account
            {
                Email = registerDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = registerDto.Role,
                IsActive = false, // Chưa xác thực email
                CreatedAt = DateTime.UtcNow,
                VerificationToken = Guid.NewGuid().ToString(),
                VerificationTokenExpiry = DateTime.UtcNow.AddHours(24) // Token hết hạn sau 24 giờ
            };

            await _unitOfWork.Accounts.AddAsync(account);
            await _unitOfWork.SaveChangesAsync();

            if (registerDto.Role == "User")
            {
                var user = new User
                {
                    AccountId = account.AccountId,
                    FullName = registerDto.FullName,
                    Email = registerDto.Email,
                    Phone = registerDto.Phone,
                    DateOfBirth = DateTime.UtcNow,
                    Gender = "Unknown"
                };
                await _unitOfWork.Users.AddAsync(user);
            }
            else if (registerDto.Role == "Owner")
            {
                var owner = new Owner
                {
                    AccountId = account.AccountId,
                    FullName = registerDto.FullName,
                    Email = registerDto.Email,
                    Phone = registerDto.Phone
                };
                await _unitOfWork.Owners.AddAsync(owner);
            }

            await _unitOfWork.SaveChangesAsync();

            // Gửi email xác thực
            var verificationLink = $"https://localhost:5231/api/auth/verify-email?email={account.Email}&token={account.VerificationToken}";
            var emailSubject = "Xác thực tài khoản C4F ISports";
            var emailBody = $"<h3>Xin chào {registerDto.FullName},</h3>" +
                            $"<p>Vui lòng nhấp vào liên kết sau để xác thực email của bạn:</p>" +
                            $"<a href='{verificationLink}'>Xác thực ngay</a>" +
                            $"<p>Liên kết này có hiệu lực trong 24 giờ.</p>" +
                            $"<p>Trân trọng,<br/>Đội ngũ C4F ISports</p>";
            await _emailSender.SendEmailAsync(account.Email, emailSubject, emailBody);

            var token = GenerateJwtToken(account);
            var refreshToken = await GenerateRefreshTokenAsync(account);
            return (token, refreshToken.Token);
        }

        public async Task<(string Token, string RefreshToken)> LoginAsync(LoginDto loginDto)
        {
            var account = _unitOfWork.Accounts.GetAll()
                .FirstOrDefault(a => a.Email == loginDto.Email);

            if (account == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, account.Password))
            {
                throw new Exception("Invalid email or password.");
            }

            if (!account.IsActive)
            {
                throw new Exception("Account not verified. Please check your email.");
            }

            account.LastLogin = DateTime.UtcNow;
            _unitOfWork.Accounts.Update(account);
            await _unitOfWork.SaveChangesAsync();

            var token = GenerateJwtToken(account);
            var refreshToken = await GenerateRefreshTokenAsync(account);
            return (token, refreshToken.Token);
        }

        public async Task<(string Token, string RefreshToken)> RefreshTokenAsync(string refreshToken)
        {
            var token = _unitOfWork.RefreshTokens.GetAll()
                .FirstOrDefault(t => t.Token == refreshToken && t.Expires > DateTime.UtcNow && !t.Revoked.HasValue);

            if (token == null)
            {
                throw new Exception("Invalid or expired refresh token.");
            }

            var account = await _unitOfWork.Accounts.GetByIdAsync(token.AccountId);
            if (account == null)
            {
                throw new Exception("Account not found.");
            }

            var newToken = GenerateJwtToken(account);
            var newRefreshToken = await GenerateRefreshTokenAsync(account);
            token.Revoked = DateTime.UtcNow;
            token.ReplacedByToken = newRefreshToken.Token;
            _unitOfWork.RefreshTokens.Update(token);
            await _unitOfWork.SaveChangesAsync();

            return (newToken, newRefreshToken.Token);
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var account = _unitOfWork.Accounts.GetAll().FirstOrDefault(a => a.Email == email);

            if (account == null)
            {
                throw new Exception("Email not found.");
            }

            var resetToken = Guid.NewGuid().ToString();
            account.ResetToken = resetToken;
            account.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            _unitOfWork.Accounts.Update(account);
            await _unitOfWork.SaveChangesAsync();

            // Gửi email reset password
            var resetLink = $"https://localhost:5231/api/auth/reset-password?email={account.Email}&token={resetToken}";
            var emailSubject = "Đặt lại mật khẩu C4F ISports";
            var emailBody = $"<h3>Xin chào {account.Email},</h3>" +
                            $"<p>Bạn đã yêu cầu đặt lại mật khẩu. Nhấp vào liên kết sau để tiếp tục:</p>" +
                            $"<a href='{resetLink}'>Đặt lại mật khẩu</a>" +
                            $"<p>Liên kết này có hiệu lực trong 1 giờ.</p>" +
                            $"<p>Trân trọng,<br/>Đội ngũ C4F ISports</p>";
            await _emailSender.SendEmailAsync(account.Email, emailSubject, emailBody);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var account = _unitOfWork.Accounts.GetAll()
                .FirstOrDefault(a => a.Email == resetPasswordDto.Email && a.ResetToken == resetPasswordDto.Token);

            if (account == null || account.ResetTokenExpiry < DateTime.UtcNow)
            {
                throw new Exception("Invalid or expired reset token.");
            }

            account.Password = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
            account.ResetToken = null;
            account.ResetTokenExpiry = null;
            _unitOfWork.Accounts.Update(account);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var token = _unitOfWork.RefreshTokens.GetAll().FirstOrDefault(t => t.Token == refreshToken);
            if (token != null)
            {
                token.Revoked = DateTime.UtcNow;
                _unitOfWork.RefreshTokens.Update(token);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<bool> VerifyTokenAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true
                }, out _);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> VerifyEmailAsync(string email, string token)
        {
            var account = _unitOfWork.Accounts.GetAll()
                .FirstOrDefault(a => a.Email == email && a.VerificationToken == token);

            if (account == null || account.VerificationTokenExpiry < DateTime.UtcNow)
            {
                throw new Exception("Invalid or expired verification token.");
            }

            if (account.IsActive)
            {
                throw new Exception("Email already verified.");
            }

            account.IsActive = true;
            account.VerificationToken = null;
            account.VerificationTokenExpiry = null;
            _unitOfWork.Accounts.Update(account);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private string GenerateJwtToken(Account account)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Role, account.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:TokenExpiryMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<RefreshToken> GenerateRefreshTokenAsync(Account account)
        {
            var refreshToken = new RefreshToken
            {
                AccountId = account.AccountId,
                Token = Guid.NewGuid().ToString(),
                Expires = DateTime.UtcNow.AddDays(int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"])),
                Created = DateTime.UtcNow
            };
            await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();
            return refreshToken;
        }
    }
}