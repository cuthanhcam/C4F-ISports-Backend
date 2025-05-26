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

        public async Task<(int AccountId, string Token, string RefreshToken)> RegisterAsync(RegisterDto registerDto)
        {
            _logger.LogInformation("Starting registration for {Email}", registerDto.Email);

            if (registerDto.Role == "Admin")
            {
                _logger.LogWarning("Attempt to register Admin role by {Email}", registerDto.Email);
                throw new ArgumentException("Không thể đăng ký vai trò Admin.");
            }

            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                _logger.LogWarning("Password and ConfirmPassword do not match for {Email}", registerDto.Email);
                throw new ArgumentException("Mật khẩu và xác nhận mật khẩu không khớp.");
            }

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

            var strategy = _unitOfWork.Context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    await _unitOfWork.Repository<Account>().AddAsync(account);
                    await _unitOfWork.SaveChangesAsync();

                    if (registerDto.Role == "User")
                    {
                        var user = new User
                        {
                            AccountId = account.AccountId,
                            FullName = registerDto.FullName,
                            Phone = registerDto.Phone,
                            CreatedAt = DateTime.UtcNow
                        };
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
                        await _unitOfWork.Repository<Owner>().AddAsync(owner);
                    }

                    await _unitOfWork.SaveChangesAsync();

                    var verificationLink = $"{_configuration["BEUrl"]}/api/auth/verify-email?email={Uri.EscapeDataString(account.Email)}&token={Uri.EscapeDataString(account.VerificationToken)}";
                    var emailSubject = "Xác thực tài khoản C4F ISports";
                    var emailBody = $"<h3>Xin chào {registerDto.FullName},</h3>" +
                                    $"<p>Vui lòng nhấp vào liên kết sau để xác thực email của bạn:</p>" +
                                    $"<a href='{verificationLink}'>Xác thực ngay</a>" +
                                    $"<p>Liên kết này có hiệu lực trong 24 giờ.</p>" +
                                    $"<p>Trân trọng,<br/>Đội ngũ C4F ISports</p>";

                    await _emailSender.SendEmailAsync(account.Email, emailSubject, emailBody);
                    _logger.LogInformation("Verification email sent to {Email}", account.Email);

                    var (token, refreshToken) = await GenerateTokensAsync(account);

                    await _unitOfWork.CommitTransactionAsync();
                    return (account.AccountId, token, refreshToken);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Registration failed for {Email}", registerDto.Email);
                    throw;
                }
            });
        }

        public async Task<(string Token, string RefreshToken, string Role)> LoginAsync(LoginDto loginDto)
        {
            var account = (await _unitOfWork.Repository<Account>().FindAsync(a => a.Email == loginDto.Email)).FirstOrDefault();
            if (account == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, account.Password))
            {
                _logger.LogWarning("Invalid login attempt for {Email}", loginDto.Email);
                throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");
            }

            if (!account.IsActive)
            {
                _logger.LogWarning("Unverified account login attempt for {Email}", loginDto.Email);
                throw new UnauthorizedAccessException("Tài khoản chưa được xác minh.");
            }

            account.LastLogin = DateTime.UtcNow;
            _unitOfWork.Repository<Account>().Update(account);
            await _unitOfWork.SaveChangesAsync();

            var (token, refreshToken) = await GenerateTokensAsync(account);
            _logger.LogInformation("User logged in: {Email}", loginDto.Email);
            return (token, refreshToken, account.Role);
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
                throw new SecurityTokenException("Token đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
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
        
        // public async Task<Account> GetCurrentUserAsync(ClaimsPrincipal user)
        // {
        //     var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //     if (string.IsNullOrEmpty(accountIdClaim) || !int.TryParse(accountIdClaim, out int accountId))
        //     {
        //         _logger.LogWarning("Invalid user token");
        //         throw new UnauthorizedAccessException("Token người dùng không hợp lệ.");
        //     }

        //     var account = await _unitOfWork.Repository<Account>().GetByIdAsync(accountId);
        //     if (account == null)
        //     {
        //         _logger.LogWarning("User not found for AccountId: {AccountId}", accountId);
        //         throw new UnauthorizedAccessException("Tài khoản không tồn tại.");
        //     }

        //     _logger.LogInformation("Current user retrieved: {Email}", account.Email);
        //     return account;
        // }
        public async Task<Account> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdClaim) || !int.TryParse(accountIdClaim, out var accountId))
            {
                _logger.LogWarning("Invalid or missing account ID in token");
                throw new UnauthorizedAccessException("Invalid or missing token");
            }

            var account = await _unitOfWork.Repository<Account>()
                .FindSingleAsync(a => a.AccountId == accountId && a.DeletedAt == null && a.IsActive);
            if (account == null)
            {
                _logger.LogWarning("Account not found or inactive for AccountId: {AccountId}", accountId);
                throw new UnauthorizedAccessException("Account not found or inactive");
            }

            return account;
        }

        public async Task ChangePasswordAsync(ClaimsPrincipal user, ChangePasswordDto changePasswordDto)
        {
            var account = await GetCurrentUserAsync(user);
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, account.Password))
            {
                _logger.LogWarning("Invalid current password for {Email}", account.Email);
                throw new ArgumentException("Mật khẩu hiện tại không đúng.");
            }

            account.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            account.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Account>().Update(account);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Password changed successfully for {Email}", account.Email);
        }    

        public async Task<bool> VerifyEmailAsync(string email, string token)
        {
            var account = (await _unitOfWork.Repository<Account>().FindAsync(a => a.Email == email && a.VerificationToken == token)).FirstOrDefault();
            if (account == null)
            {
                _logger.LogWarning("No account found for email {Email} or token {Token}", email, token);
                return false;
            }
            if (account.VerificationTokenExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning("Verification token expired for {Email}, expiry: {Expiry}", email, account.VerificationTokenExpiry);
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
            _logger.LogInformation("Resending verification or restoration email to {Email}", email);

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Email is empty");
                throw new ArgumentException("Email không tồn tại.");
            }

            var account = (await _unitOfWork.Repository<Account>().FindAsync(a => a.Email == email)).FirstOrDefault();
            if (account == null)
            {
                _logger.LogWarning("Resend verification requested for non-existent email: {Email}", email);
                throw new ArgumentException("Email không tồn tại.");
            }

            // Kiểm tra trạng thái tài khoản
            bool isRestoreRequest = account.DeletedAt != null && !account.IsActive;
            bool isVerificationRequest = !account.IsActive && account.DeletedAt == null;

            if (!isRestoreRequest && !isVerificationRequest)
            {
                _logger.LogWarning("Account already verified or not in a verifiable state for {Email}", email);
                throw new InvalidOperationException("Tài khoản đã được xác minh hoặc không ở trạng thái có thể xác minh/khôi phục.");
            }

            // Tạo token mới
            account.VerificationToken = GenerateSecureToken();
            account.VerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
            _unitOfWork.Repository<Account>().Update(account);
            await _unitOfWork.SaveChangesAsync();

            // Tạo link và nội dung email
            string link, subject, body;
            if (isRestoreRequest)
            {
                link = $"{_configuration["FEUrl"]}/auth/restore?email={Uri.EscapeDataString(account.Email)}&token={Uri.EscapeDataString(account.VerificationToken)}";
                subject = "Khôi phục tài khoản C4F ISports";
                body = $"<h3>Xin chào {account.Email},</h3>" +
                       $"<p>Bạn đã yêu cầu khôi phục tài khoản. Vui lòng nhấp vào liên kết sau để khôi phục:</p>" +
                       $"<a href='{link}'>Khôi phục tài khoản</a>" +
                       $"<p>Liên kết này có hiệu lực trong 24 giờ.</p>" +
                       $"<p>Trân trọng,<br/>Đội ngũ C4F ISports</p>";
            }
            else
            {
                link = $"{_configuration["BEUrl"]}/api/auth/verify-email?email={Uri.EscapeDataString(account.Email)}&token={Uri.EscapeDataString(account.VerificationToken)}";
                subject = "Xác thực lại tài khoản C4F ISports";
                body = $"<h3>Xin chào {account.Email},</h3>" +
                       $"<p>Bạn đã yêu cầu gửi lại email xác thực. Vui lòng nhấp vào liên kết sau để xác thực tài khoản:</p>" +
                       $"<a href='{link}'>Xác thực ngay</a>" +
                       $"<p>Liên kết này có hiệu lực trong 24 giờ.</p>" +
                       $"<p>Trân trọng,<br/>Đội ngũ C4F ISports</p>";
            }

            await _emailSender.SendEmailAsync(account.Email, subject, body);
            _logger.LogInformation("{Type} email sent to {Email}", isRestoreRequest ? "Restoration" : "Verification", email);
        }

        public async Task RestoreAccountAsync(string email, string token)
        {
            _logger.LogInformation("Restoring account for {Email}", email);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Email or token is empty");
                throw new ArgumentException("Email và token là bắt buộc.");
            }

            var account = (await _unitOfWork.Repository<Account>()
                .FindAsync(a => a.Email == email && a.VerificationToken == token && a.VerificationTokenExpiry > DateTime.UtcNow))
                .FirstOrDefault();
            if (account == null || account.DeletedAt == null || account.IsActive)
            {
                _logger.LogWarning("Invalid or expired token, or account not in deleted state for {Email}", email);
                throw new ArgumentException("Token không hợp lệ, đã hết hạn, hoặc tài khoản không ở trạng thái đã xóa.");
            }

            // Khôi phục Account
            account.DeletedAt = null;
            account.IsActive = true;
            account.VerificationToken = null;
            account.VerificationTokenExpiry = null;
            account.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Account>().Update(account);

            // Khôi phục User hoặc Owner
            if (account.Role == "User")
            {
                var userEntity = await _unitOfWork.Repository<User>()
                    .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt != null);
                if (userEntity == null)
                {
                    _logger.LogWarning("User not found for AccountId: {AccountId}", account.AccountId);
                    throw new ArgumentException("Thông tin người dùng không tồn tại.");
                }

                userEntity.DeletedAt = null;
                userEntity.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<User>().Update(userEntity);
            }
            else if (account.Role == "Owner")
            {
                var ownerEntity = await _unitOfWork.Repository<Owner>()
                    .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt != null);
                if (ownerEntity == null)
                {
                    _logger.LogWarning("Owner not found for AccountId: {AccountId}", account.AccountId);
                    throw new ArgumentException("Thông tin chủ sân không tồn tại.");
                }

                ownerEntity.DeletedAt = null;
                ownerEntity.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<Owner>().Update(ownerEntity);
            }
            else
            {
                _logger.LogWarning("Invalid role: {Role} for {Email}", account.Role, email);
                throw new ArgumentException("Vai trò không hợp lệ.");
            }

            await _unitOfWork.SaveChangesAsync();

            // Gửi email thông báo
            try
            {
                var emailSubject = "Thông báo khôi phục tài khoản C4F ISports";
                var emailBody = $"<h3>Xin chào {email},</h3>" +
                                "<p>Tài khoản của bạn đã được khôi phục thành công.</p>" +
                                "<p>Bạn có thể đăng nhập lại bằng thông tin đăng nhập cũ.</p>" +
                                "<p>Trân trọng,<br/>Đội ngũ C4F ISports</p>";
                await _emailSender.SendEmailAsync(email, emailSubject, emailBody);
                _logger.LogInformation("Restoration notification sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send restoration notification to {Email}", email);
            }

            _logger.LogInformation("Account restored successfully for {Email}", email);
        }

        public async Task<(bool IsValid, string Role)> VerifyTokenAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret chưa được cấu hình."));
            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JwtSettings:Issuer"] ?? throw new InvalidOperationException("JWT Issuer chưa được cấu hình."),
                    ValidateAudience = true,
                    ValidAudience = _configuration["JwtSettings:Audience"] ?? throw new InvalidOperationException("JWT Audience chưa được cấu hình."),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                }, out _);

                var role = principal?.FindFirst(ClaimTypes.Role)?.Value;
                _logger.LogInformation("Token verified successfully");
                return (true, role ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token verification failed");
                return (false, string.Empty);
            }
        }

        private async Task<(string Token, string RefreshToken)> GenerateTokensAsync(Account account)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Role, account.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret chưa được cấu hình.")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"] ?? throw new InvalidOperationException("JWT Issuer chưa được cấu hình."),
                audience: _configuration["JwtSettings:Audience"] ?? throw new InvalidOperationException("JWT Audience chưa được cấu hình."),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:TokenExpiryMinutes"] ?? throw new InvalidOperationException("TokenExpiryMinutes chưa được cấu hình."))),
                signingCredentials: creds
            );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = new RefreshToken
            {
                AccountId = account.AccountId,
                Token = GenerateSecureToken(),
                Expires = DateTime.UtcNow.AddDays(int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? throw new InvalidOperationException("RefreshTokenExpiryDays chưa được cấu hình."))),
                Created = DateTime.UtcNow
            };
            await _unitOfWork.Repository<RefreshToken>().AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Tokens generated successfully for {Email}", account.Email);
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