using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Auth;
using api.Interfaces;
using api.Models;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace api.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<(string Token, string RefreshToken)> RegisterAsync(RegisterDto registerDto)
        {
            if (await _context.Accounts.AnyAsync(a => a.Email == registerDto.Email))
            {
                throw new Exception("Email already exists");
            }

            var account = new Account
            {
                Email = registerDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = registerDto.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

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
                _context.Users.Add(user);
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
                _context.Owners.Add(owner);
            }

            await _context.SaveChangesAsync();

            var token = GenerateJwy
        }

        public Task<(string Token, string RefreshToken)> LoginAsync(LoginDto loginDto)
        {
            throw new NotImplementedException();
        }

        public Task<(string Token, string RefreshToken)> RefreshTokenAsync(string refreshToken)
        {
            throw new NotImplementedException();
        }

        public Task ForgotPasswordAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            throw new NotImplementedException();
        }

        public Task LogoutAsync(string refreshToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> VerifyTokenAsync(string token)
        {
            throw new NotImplementedException();
        }

        
    }
}