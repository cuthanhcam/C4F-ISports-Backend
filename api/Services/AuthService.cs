using api.Dtos.Auth;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration, IEmailSender emailSender, ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<(string Token, string RefreshToken)> RegisterAsync(RegisterDto registerDto)
        {
            _logger.LogInformation("Starting registration for {Email}", registerDto.Email);

            if (registerDto.Role == "Admin")
            {
                _logger.LogWarning("Attempt to register Admin role by {Email}", registerDto.Email);
                throw new ArgumentException("Không thể đăng ký tài khoản với vai trò Admin.");
            }

            _logger.LogInformation("Checking for existing account: {Email}", registerDto.Email);
            var existingAccount = (await _unitOfWork.Repository<Account>().FindAsync(a => a.Email == registerDto.Email)).FirstOrDefault();
            if (existingAccount != null)
            {
                _logger.LogWarning("Email already exists: {Email}", registerDto.Email);
                throw new ArgumentException("Email đã tồn tại.");
            }

            var account = new Account
            {
                Email = registerDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = registerDto.Role,
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                VerificationToken = GenerateSecureToken(),
                VerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
            };

            // Sử dụng execution strategy cho giao dịch
            var strategy = _unitOfWork.Context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    _logger.LogInformation("Transaction started for {Email}", registerDto.Email);

                    _logger.LogInformation("Adding account for {Email}", registerDto.Email);
                    await _unitOfWork.Repository<Account>().AddAsync(account);
                    await _unitOfWork.SaveChangesAsync();

                    if (registerDto.Role == "User")
                    {
                        var user = new User
                        {
                            AccountId = account.AccountId,
                            FullName = registerDto.FullName,
                            Phone = registerDto.Phone,
                            Gender = null,
                            DateOfBirth = null,
                            CreatedAt = DateTime.UtcNow
                        };
                        _logger.LogInformation("Adding user for {Email}", registerDto.Email);
                        await _unitOfWork.Repository<User>().AddAsync(user);
                    }
                    else if (registerDto.Role == "Owner")
                    {
                        var owner = new Owner
                        {
                            AccountId = account.AccountId,
                            FullName = registerDto.FullName,
                            Phone = registerDto.Phone,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _logger.LogInformation("Adding owner for {Email}", registerDto.Email);
                        await _unitOfWork.Repository<Owner>().AddAsync(owner);
                    }

                    _logger.LogInformation("Saving changes for {Email}", registerDto.Email);
                    await _unitOfWork.SaveChangesAsync();

                    var verificationLink = $"{_configuration["AppUrl"]}/api/auth/verify-email?email={Uri.EscapeDataString(account.Email)}&token={Uri.EscapeDataString(account.VerificationToken)}";
                    var emailSubject = "Xác thực tài khoản C4F ISports";
                    var emailBody = $"<h3>Xin chào {registerDto.FullName},</h3>" +
                                    $"<p>Vui lòng nhấp vào liên kết sau để xác thực email của bạn:</p>" +
                                    $"<a href='{verificationLink}'>Xác thực ngay</a>" +
                                    $"<p>Liên kết này có hiệu lực trong 24 giờ.</p>" +
                                    $"<p>Trân trọng,<br/>Đội ngũ C4F ISports</p>";

                    _logger.LogInformation("Email sending skipped for {Email}", account.Email);
                    await _emailSender.SendEmailAsync(account.Email, emailSubject, emailBody);

                    _logger.LogInformation("Generating tokens for {Email}", account.Email);
                    var (token, refreshToken) = await GenerateTokensAsync(account);

                    _logger.LogInformation("Committing transaction for {Email}", registerDto.Email);
                    await _unitOfWork.CommitTransactionAsync();
                    _logger.LogInformation("User registered successfully: {Email}", registerDto.Email);

                    return (token, refreshToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during registration for {Email}. Rolling back transaction.", registerDto.Email);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            });
        }

        public async Task<(string Token, string RefreshToken)> LoginAsync(LoginDto loginDto)
        {
            var account = (await _unitOfWork.Repository<Account>().FindAsync(a => a.Email == loginDto.Email)).FirstOrDefault();
            if (account == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, account.Password))
            {
                _logger.LogWarning("Invalid login attempt for {Email}", loginDto.Email);
                throw new UnauthorizedAccessException("Email hoặc mật khẩu không chính xác.");
            }

            if (!account.IsActive)
            {
                _logger.LogWarning("Unverified account login attempt for {Email}", loginDto.Email);
                throw new UnauthorizedAccessException("Tài khoản chưa được xác thực.");
            }

            account.LastLogin = DateTime.UtcNow;
            _unitOfWork.Repository<Account>().Update(account);
            await _unitOfWork.SaveChangesAsync();

            var (token, refreshToken) = await GenerateTokensAsync(account);
            _logger.LogInformation("User logged in: {Email}", loginDto.Email);
            return (token, refreshToken);
        }

        public async Task<(string Token, string RefreshToken)> RefreshTokenAsync(string refreshToken)
        {
            var token = (await _unitOfWork.Repository<RefreshToken>().FindAsync(t => t.Token == refreshToken && t.Expires > DateTime.UtcNow && !t.Revoked.HasValue)).FirstOrDefault();
            if (token == null)
            {
                _logger.LogWarning("Invalid or expired refresh token");
                throw new SecurityTokenException("Refresh token không hợp lệ hoặc đã hết hạn.");
            }

            var account = await _unitOfWork.Repository<Account>().GetByIdAsync(token.AccountId);
            if (account == null)
            {
                _logger.LogWarning("Account not found for refresh token");
                throw new SecurityTokenException("Tài khoản không tồn tại.");
            }

            token.Revoked = DateTime.UtcNow;
            _unitOfWork.Repository<RefreshToken>().Update(token);
            var (newToken, newRefreshToken) = await GenerateTokensAsync(account);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Token refreshed for {Email}", account.Email);
            return (newToken, newRefreshToken);
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var account = (await _unitOfWork.Repository<Account>().FindAsync(a => a.Email == email)).FirstOrDefault();
            if (account == null)
            {
                _logger.LogWarning("Forgot password requested for non-existent email: {Email}", email);
                throw new ArgumentException("Email không tồn tại.");
            }

            account.ResetToken = GenerateSecureToken();
            account.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            _unitOfWork.Repository<Account>().Update(account);
            await _unitOfWork.SaveChangesAsync();

            var resetLink = $"{_configuration["FEUrl"]}/auth/reset-password?email={Uri.EscapeDataString(account.Email)}&token={Uri.EscapeDataString(account.ResetToken)}";
            var emailSubject = "Đặt lại mật khẩu C4F ISports";
            var emailBody = $"<h3>Xin chào {account.Email},</h3>" +
                            $"<p>Bạn đã yêu cầu đặt lại mật khẩu. Nhấp vào liên kết sau để tiếp tục:</p>" +
                            $"<a href='{resetLink}'>Đặt lại mật khẩu</a>" +
                            $"<p>Liên kết này có hiệu lực trong 1 giờ.</p>" +
                            $"<p>Trân trọng,<br/>Đội ngũ C4F ISports</p>";
            await _emailSender.SendEmailAsync(account.Email, emailSubject, emailBody);
            _logger.LogInformation("Password reset email sent to {Email}", email);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var account = (await _unitOfWork.Repository<Account>().FindAsync(a => a.Email == resetPasswordDto.Email && a.ResetToken == resetPasswordDto.Token)).FirstOrDefault();
            if (account == null || account.ResetTokenExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired reset token for {Email}", resetPasswordDto.Email);
                throw new SecurityTokenException("Token không hợp lệ hoặc đã hết hạn.");
            }

            account.Password = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
            account.ResetToken = null;
            account.ResetTokenExpiry = null;
            _unitOfWork.Repository<Account>().Update(account);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Password reset for {Email}", resetPasswordDto.Email);
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var token = (await _unitOfWork.Repository<RefreshToken>().FindAsync(t => t.Token == refreshToken)).FirstOrDefault();
            if (token == null)
            {
                _logger.LogWarning("Invalid refresh token for logout");
                throw new ArgumentException("Refresh token không hợp lệ.");
            }

            token.Revoked = DateTime.UtcNow;
            _unitOfWork.Repository<RefreshToken>().Update(token);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("User logged out");
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
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JwtSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["JwtSettings:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                }, out _);
                _logger.LogInformation("Token verified successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token verification failed");
                return false;
            }
        }

        public async Task<bool> VerifyEmailAsync(string email, string token)
        {
            var account = (await _unitOfWork.Repository<Account>().FindAsync(a => a.Email == email && a.VerificationToken == token)).FirstOrDefault();
            if (account == null || account.VerificationTokenExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired verification token for {Email}", email);
                return false;
            }

            if (account.IsActive)
            {
                _logger.LogWarning("Email already verified for {Email}", email);
                return true;
            }

            account.IsActive = true;
            account.VerificationToken = null;
            account.VerificationTokenExpiry = null;
            _unitOfWork.Repository<Account>().Update(account);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Email verified for {Email}", email);
            return true;
        }

        public async Task ResendVerificationEmailAsync(string email)
        {
            var account = (await _unitOfWork.Repository<Account>().FindAsync(a => a.Email == email)).FirstOrDefault();
            if (account == null)
            {
                _logger.LogWarning("Resend verification requested for non-existent email: {Email}", email);
                throw new ArgumentException("Email không tồn tại.");
            }

            if (account.IsActive)
            {
                _logger.LogWarning("Resend verification requested for already verified email: {Email}", email);
                throw new InvalidOperationException("Tài khoản đã được kích hoạt.");
            }

            account.VerificationToken = GenerateSecureToken();
            account.VerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
            _unitOfWork.Repository<Account>().Update(account);
            await _unitOfWork.SaveChangesAsync();

            var verificationLink = $"{_configuration["AppUrl"]}/api/auth/verify-email?email={Uri.EscapeDataString(account.Email)}&token={Uri.EscapeDataString(account.VerificationToken)}";
            var emailSubject = "Xác thực lại tài khoản C4F ISports";
            var emailBody = $"<h3>Xin chào {account.Email},</h3>" +
                            $"<p>Bạn đã yêu cầu gửi lại email xác thực. Vui lòng nhấp vào liên kết sau để xác thực tài khoản:</p>" +
                            $"<a href='{verificationLink}'>Xác thực ngay</a>" +
                            $"<p>Liên kết này có hiệu lực trong 24 giờ.</p>" +
                            $"<p>Trân trọng,<br/>Đội ngũ C4F ISports</p>";
            await _emailSender.SendEmailAsync(account.Email, emailSubject, emailBody);
            _logger.LogInformation("Verification email resent to {Email}", email);
        }

        public async Task<Account> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdClaim) || !int.TryParse(accountIdClaim, out int accountId))
            {
                _logger.LogWarning("Invalid user token");
                throw new UnauthorizedAccessException("Token người dùng không hợp lệ.");
            }

            var account = await _unitOfWork.Repository<Account>().GetByIdAsync(accountId);
            if (account == null)
            {
                _logger.LogWarning("User not found for AccountId: {AccountId}", accountId);
                throw new UnauthorizedAccessException("Tài khoản không tồn tại.");
            }

            _logger.LogInformation("Current user retrieved: {Email}", account.Email);
            return account;
        }

        public async Task ChangePasswordAsync(ClaimsPrincipal user, ChangePasswordDto changePasswordDto)
        {
            var account = await GetCurrentUserAsync(user);
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.OldPassword, account.Password))
            {
                _logger.LogWarning("Incorrect old password for {Email}", account.Email);
                throw new ArgumentException("Mật khẩu cũ không đúng.");
            }

            account.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            account.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Account>().Update(account);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Password changed for {Email}", account.Email);
        }

        private async Task<(string Token, string RefreshToken)> GenerateTokensAsync(Account account)
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
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:TokenExpiryMinutes"])),
                signingCredentials: creds);

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = new RefreshToken
            {
                AccountId = account.AccountId,
                Token = GenerateSecureToken(),
                Expires = DateTime.UtcNow.AddDays(int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"])),
                Created = DateTime.UtcNow
            };
            await _unitOfWork.Repository<RefreshToken>().AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Tokens generated for {Email}", account.Email);
            return (jwtToken, refreshToken.Token);
        }

        private string GenerateSecureToken()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }
    }
}