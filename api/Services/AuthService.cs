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
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<(string Token, string RefreshToken)> RegisterAsync(RegisterDto registerDto)
        {
            if (_unitOfWork.Accounts.GetAll().Any(a => a.Email == registerDto.Email))
            // await _unitOfWork.Accounts.GetAll().AnyAsync(a => a.Email == registerDto.Email)
            {
                throw new Exception("Email already exists.");
            }

            var account = new Account
            {
                Email = registerDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = registerDto.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
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
                    DateOfBirth = DateTime.UtcNow // Có thể yêu cầu nhập sau
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

            var token = GenerateJwtToken(account);
            var refreshToken = await GenerateRefreshTokenAsync(account);
            return (token, refreshToken.Token);
        }

        public async Task<(string Token, string RefreshToken)> LoginAsync(LoginDto loginDto)
        {
            var account = _unitOfWork.Accounts.GetAll()
                .FirstOrDefault(a => a.Email == loginDto.Email);
            // await _unitOfWork.Accounts.GetAll().FirstOrDefaultAsync(a => a.Email == loginDto.Email);

            if (account == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, account.Password))
            {
                throw new Exception("Invalid email or password.");
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
            // await _unitOfWork.RefreshTokens.GetAll().FirstOrDefaultAsync(t => t.Token == refreshToken && t.Expires > DateTime.UtcNow && !t.Revoked.HasValue);

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
            // await _unitOfWork.Accounts.GetAll().FirstOrDefaultAsync(a => a.Email == email);

            if (account == null)
            {
                throw new Exception("Email not found.");
            }

            var resetToken = Guid.NewGuid().ToString();
            account.ResetToken = resetToken;
            account.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            _unitOfWork.Accounts.Update(account);
            await _unitOfWork.SaveChangesAsync();

            // TODO: Send email with resetToken

        }

        public async Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var account = _unitOfWork.Accounts.GetAll()
                .FirstOrDefault(a => a.Email == resetPasswordDto.Email && a.ResetToken == resetPasswordDto.Token);
            // await _unitOfWork.Accounts.GetAll().FirstOrDefaultAsync(a => a.Email == resetPasswordDto.Email && a.ResetToken == resetPasswordDto.Token);
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
            // await _unitOfWork.RefreshTokens.GetAll().FirstOrDefaultAsync(t => t.Token == refreshToken);
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

        // Generate JWT Token
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

        // Generate Refresh Token
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