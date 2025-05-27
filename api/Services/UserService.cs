using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos.User;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace api.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService> _logger;
        private readonly IAuthService _authService;
        private readonly IEmailSender _emailSender;

        public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger, IAuthService authService, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _authService = authService;
            _emailSender = emailSender;
        }

        public async Task<UserProfileDto> GetProfileAsync(ClaimsPrincipal user)
        {
            _logger.LogInformation("Lấy thông tin hồ sơ người dùng");
            var account = await _authService.GetCurrentUserAsync(user);

            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản người dùng");
                throw new UnauthorizedAccessException("Invalid or missing token");
            }

            UserProfileDto profile;
            if (account.Role == "User")
            {
                var userEntity = await _unitOfWork.Repository<User>()
                    .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
                if (userEntity == null)
                {
                    _logger.LogWarning("Không tìm thấy thông tin người dùng cho AccountId: {AccountId}", account.AccountId);
                    throw new ArgumentException("Thông tin người dùng không tồn tại.");
                }

                profile = new UserProfileDto
                {
                    UserId = userEntity.UserId,
                    FullName = userEntity.FullName ?? string.Empty,
                    Email = account.Email,
                    Phone = userEntity.Phone ?? string.Empty,
                    City = userEntity.City,
                    District = userEntity.District,
                    AvatarUrl = userEntity.AvatarUrl,
                    DateOfBirth = userEntity.DateOfBirth
                };
            }
            else if (account.Role == "Owner")
            {
                var ownerEntity = await _unitOfWork.Repository<Owner>()
                    .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
                if (ownerEntity == null)
                {
                    _logger.LogWarning("Không tìm thấy thông tin chủ sân cho AccountId: {AccountId}", account.AccountId);
                    throw new ArgumentException("Thông tin chủ sân không tồn tại.");
                }

                profile = new UserProfileDto
                {
                    OwnerId = ownerEntity.OwnerId,
                    FullName = ownerEntity.FullName,
                    Email = account.Email,
                    Phone = ownerEntity.Phone,
                    Description = ownerEntity.Description
                };
            }
            else
            {
                _logger.LogWarning("Vai trò không hợp lệ: {Role}", account.Role);
                throw new UnauthorizedAccessException("Vai trò không hợp lệ.");
            }

            _logger.LogInformation("Lấy thông tin hồ sơ thành công cho {Email}", account.Email);
            return profile;
        }

        public async Task<UserProfileDto> UpdateProfileAsync(ClaimsPrincipal user, UpdateProfileDto updateProfileDto)
        {
            _logger.LogInformation("Cập nhật hồ sơ người dùng");
            var account = await _authService.GetCurrentUserAsync(user);

            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản người dùng");
                throw new UnauthorizedAccessException("Invalid or missing token");
            }

            UserProfileDto updatedProfile;
            if (account.Role == "User")
            {
                var userEntity = await _unitOfWork.Repository<User>()
                    .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
                if (userEntity == null)
                {
                    _logger.LogWarning("Không tìm thấy thông tin người dùng cho AccountId: {AccountId}", account.AccountId);
                    throw new ArgumentException("Thông tin người dùng không tồn tại.");
                }

                userEntity.FullName = updateProfileDto.FullName;
                userEntity.Phone = updateProfileDto.Phone;
                userEntity.City = updateProfileDto.City;
                userEntity.District = updateProfileDto.District;
                userEntity.AvatarUrl = updateProfileDto.AvatarUrl;
                userEntity.DateOfBirth = updateProfileDto.DateOfBirth;
                userEntity.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<User>().Update(userEntity);
                await _unitOfWork.SaveChangesAsync();

                updatedProfile = new UserProfileDto
                {
                    UserId = userEntity.UserId,
                    FullName = userEntity.FullName,
                    Email = account.Email,
                    Phone = userEntity.Phone,
                    City = userEntity.City,
                    District = userEntity.District,
                    AvatarUrl = userEntity.AvatarUrl,
                    DateOfBirth = userEntity.DateOfBirth,
                    Message = "Profile updated successfully"
                };
            }
            else if (account.Role == "Owner")
            {
                var ownerEntity = await _unitOfWork.Repository<Owner>()
                    .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
                if (ownerEntity == null)
                {
                    _logger.LogWarning("Không tìm thấy thông tin chủ sân cho AccountId: {AccountId}", account.AccountId);
                    throw new ArgumentException("Thông tin chủ sân không tồn tại.");
                }

                ownerEntity.FullName = updateProfileDto.FullName;
                ownerEntity.Phone = updateProfileDto.Phone;
                ownerEntity.Description = updateProfileDto.Description;
                ownerEntity.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<Owner>().Update(ownerEntity);
                await _unitOfWork.SaveChangesAsync();

                updatedProfile = new UserProfileDto
                {
                    OwnerId = ownerEntity.OwnerId,
                    FullName = ownerEntity.FullName,
                    Email = account.Email,
                    Phone = ownerEntity.Phone,
                    Description = ownerEntity.Description,
                    Message = "Profile updated successfully"
                };
            }
            else
            {
                _logger.LogWarning("Vai trò không hợp lệ: {Role}", account.Role);
                throw new UnauthorizedAccessException("Vai trò không hợp lệ.");
            }

            _logger.LogInformation("Cập nhật hồ sơ thành công cho {Email}", account.Email);
            return updatedProfile;
        }

        public async Task DeleteProfileAsync(ClaimsPrincipal user)
        {
            _logger.LogInformation("Deleting user profile");
            var account = await _authService.GetCurrentUserAsync(user);

            if (account == null)
            {
                _logger.LogWarning("User account not found");
                throw new UnauthorizedAccessException("Invalid or missing token");
            }

            // Kiểm tra booking hoặc field đang hoạt động
            if (account.Role == "User")
            {
                var userEntity = await _unitOfWork.Repository<User>()
                    .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
                if (userEntity == null)
                {
                    _logger.LogWarning("User not found for AccountId: {AccountId}", account.AccountId);
                    throw new ArgumentException("User information not found.");
                }

                var activeBookings = await _unitOfWork.Repository<Booking>()
                    .FindQueryable(b => b.UserId == userEntity.UserId && b.Status == "Confirmed")
                    .AnyAsync();
                if (activeBookings)
                {
                    _logger.LogWarning("Cannot delete profile due to active bookings for AccountId: {AccountId}", account.AccountId);
                    throw new ArgumentException("Cannot delete profile due to active bookings.");
                }

                // Soft delete user
                userEntity.DeletedAt = DateTime.UtcNow;
                userEntity.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<User>().Update(userEntity);
            }
            else if (account.Role == "Owner")
            {
                var ownerEntity = await _unitOfWork.Repository<Owner>()
                    .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
                if (ownerEntity == null)
                {
                    _logger.LogWarning("Owner not found for AccountId: {AccountId}", account.AccountId);
                    throw new ArgumentException("Owner information not found.");
                }

                var activeFields = await _unitOfWork.Repository<Field>()
                    .FindQueryable(f => f.OwnerId == ownerEntity.OwnerId && f.DeletedAt == null)
                    .AnyAsync();
                if (activeFields)
                {
                    _logger.LogWarning("Cannot delete profile due to active fields for AccountId: {AccountId}", account.AccountId);
                    throw new ArgumentException("Cannot delete profile due to active fields.");
                }

                // Soft delete owner
                ownerEntity.DeletedAt = DateTime.UtcNow;
                ownerEntity.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<Owner>().Update(ownerEntity);
            }
            else
            {
                _logger.LogWarning("Invalid role: {Role}", account.Role);
                throw new UnauthorizedAccessException("Invalid role.");
            }

            // Vô hiệu hóa tài khoản
            account.IsActive = false;
            account.DeletedAt = DateTime.UtcNow;
            account.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Account>().Update(account);

            // Lưu tất cả thay đổi
            await _unitOfWork.SaveChangesAsync();

            // Gửi email thông báo xóa tài khoản
            try
            {
                var emailSubject = "Thông báo xóa tài khoản C4F ISports";
                var emailBody = $"<h3>Xin chào {account.Email},</h3>" +
                                "<p>Tài khoản của bạn đã được xóa thành công.</p>" +
                                "<p>Nếu bạn muốn khôi phục tài khoản, vui lòng sử dụng chức năng khôi phục trong vòng 30 ngày.</p>" +
                                "<p>Trân trọng,<br/>Đội ngũ C4F ISports</p>";
                await _emailSender.SendEmailAsync(account.Email, emailSubject, emailBody);
                _logger.LogInformation("Sent profile deletion notification to {Email}", account.Email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send profile deletion notification to {Email}", account.Email);
            }

            _logger.LogInformation("Profile deleted successfully for {Email}", account.Email);
        }

        public async Task<LoyaltyPointsDto> GetLoyaltyPointsAsync(ClaimsPrincipal user)
        {
            _logger.LogInformation("Lấy điểm loyalty của người dùng");
            var account = await _authService.GetCurrentUserAsync(user);

            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản người dùng");
                throw new UnauthorizedAccessException("Invalid or missing token");
            }

            if (account.Role != "User")
            {
                _logger.LogWarning("Vai trò không hợp lệ để lấy điểm loyalty: {Role}", account.Role);
                throw new UnauthorizedAccessException("Only users can access loyalty points.");
            }

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
            if (userEntity == null)
            {
                _logger.LogWarning("Không tìm thấy thông tin người dùng cho AccountId: {AccountId}", account.AccountId);
                throw new ArgumentException("Thông tin người dùng không tồn tại.");
            }

            var loyaltyPoints = new LoyaltyPointsDto
            {
                UserId = userEntity.UserId,
                LoyaltyPoints = userEntity.LoyaltyPoints
            };

            _logger.LogInformation("Lấy điểm loyalty thành công cho {Email}", account.Email);
            return loyaltyPoints;
        }

        public async Task<(IList<SearchHistoryDto> Data, int Total, int Page, int PageSize)> GetSearchHistoryAsync(
            ClaimsPrincipal user, DateTime? startDate, DateTime? endDate, int page, int pageSize)
        {
            _logger.LogInformation("Lấy lịch sử tìm kiếm của người dùng");
            var account = await _authService.GetCurrentUserAsync(user);

            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản người dùng");
                throw new UnauthorizedAccessException("Invalid or missing token");
            }

            if (account.Role != "User")
            {
                _logger.LogWarning("Vai trò không hợp lệ để lấy lịch sử tìm kiếm: {Role}", account.Role);
                throw new UnauthorizedAccessException("Only users can access search history.");
            }

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
            if (userEntity == null)
            {
                _logger.LogWarning("Không tìm thấy thông tin người dùng cho AccountId: {AccountId}", account.AccountId);
                throw new ArgumentException("Thông tin người dùng không tồn tại.");
            }

            IQueryable<SearchHistory> query = _unitOfWork.Repository<SearchHistory>()
                .FindQueryable(sh => sh.UserId == userEntity.UserId && sh.DeletedAt == null);

            if (startDate.HasValue)
            {
                query = query.Where(sh => sh.SearchDateTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(sh => sh.SearchDateTime <= endDate.Value);
            }

            query = query.OrderByDescending(sh => sh.SearchDateTime);

            var total = await query.CountAsync();
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(sh => new SearchHistoryDto
                {
                    SearchId = sh.SearchId,
                    UserId = sh.UserId,
                    Keyword = sh.Keyword,
                    SearchDateTime = sh.SearchDateTime,
                    FieldId = sh.FieldId,
                    Latitude = sh.Latitude,
                    Longitude = sh.Longitude
                })
                .ToListAsync();

            _logger.LogInformation("Lấy lịch sử tìm kiếm thành công cho {Email}", account.Email);
            return (data, total, page, pageSize);
        }

        public async Task<(IList<BookingHistoryDto> Data, int Total, int Page, int PageSize)> GetBookingHistoryAsync(
            ClaimsPrincipal user, string? status, DateTime? startDate, DateTime? endDate, string? sort, int page, int pageSize)
        {
            _logger.LogInformation("Lấy lịch sử đặt sân cho người dùng");
            var account = await _authService.GetCurrentUserAsync(user);

            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản người dùng");
                throw new UnauthorizedAccessException("Invalid or missing token");
            }

            if (account.Role != "User")
            {
                _logger.LogWarning("Vai trò không hợp lệ để xem lịch sử đặt sân: {Role}", account.Role);
                throw new UnauthorizedAccessException("Only users can access booking history.");
            }

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
            if (userEntity == null)
            {
                _logger.LogWarning("Không tìm thấy thông tin người dùng cho AccountId: {AccountId}", account.AccountId);
                throw new ArgumentException("Thông tin người dùng không tồn tại.");
            }

            IQueryable<Booking> query = _unitOfWork.Repository<Booking>()
                .FindQueryable(b => b.UserId == userEntity.UserId && b.DeletedAt == null)
                .Include(b => b.SubField)
                .ThenInclude(sf => sf.Field);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.Status == status);
            }

            if (startDate.HasValue)
            {
                query = query.Where(b => b.BookingDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(b => b.BookingDate <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(sort))
            {
                var sortParts = sort.Split(':');
                var sortField = sortParts[0];
                var sortDirection = sortParts.Length > 1 ? sortParts[1].ToLower() : "asc";

                if (sortField == "BookingDate")
                {
                    query = sortDirection == "desc"
                        ? query.OrderByDescending(b => b.BookingDate)
                        : query.OrderBy(b => b.BookingDate);
                }
            }
            else
            {
                query = query.OrderByDescending(b => b.BookingDate);
            }

            var total = await query.CountAsync();
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookingHistoryDto
                {
                    BookingId = b.BookingId,
                    FieldName = b.SubField.Field.FieldName,
                    SubFieldName = b.SubField.SubFieldName,
                    BookingDate = b.BookingDate,
                    StartTime = b.StartTime,
                    EndTime = b.EndTime,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status,
                    PaymentStatus = b.PaymentStatus
                })
                .ToListAsync();

            _logger.LogInformation("Lấy lịch sử đặt sân thành công cho {Email}", account.Email);
            return (data, total, page, pageSize);
        }

        public async Task<(IList<FavoriteFieldDto> Data, int Total, int Page, int PageSize)> GetFavoriteFieldsAsync(
            ClaimsPrincipal user, string? sort, int page, int pageSize)
        {
            _logger.LogInformation("Lấy danh sách sân yêu thích");
            var account = await _authService.GetCurrentUserAsync(user);

            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản người dùng");
                throw new UnauthorizedAccessException("Invalid or missing token");
            }

            if (account.Role != "User")
            {
                _logger.LogWarning("Vai trò không hợp lệ để xem sân yêu thích: {Role}", account.Role);
                throw new UnauthorizedAccessException("Only users can access favorite fields.");
            }

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
            if (userEntity == null)
            {
                _logger.LogWarning("Không tìm thấy thông tin người dùng cho AccountId: {AccountId}", account.AccountId);
                throw new ArgumentException("Thông tin người dùng không tồn tại.");
            }

            var baseQuery = _unitOfWork.Repository<FavoriteField>()
                .FindQueryable(ff => ff.UserId == userEntity.UserId)
                .Include(ff => ff.Field);

            IQueryable<FavoriteField> query;
            if (!string.IsNullOrEmpty(sort))
            {
                var sortParts = sort.Split(':');
                var sortField = sortParts[0];
                var sortDirection = sortParts.Length > 1 ? sortParts[1].ToLower() : "asc";

                if (sortField == "FieldName")
                {
                    query = sortDirection == "desc"
                        ? baseQuery.OrderByDescending(ff => ff.Field.FieldName)
                        : baseQuery.OrderBy(ff => ff.Field.FieldName);
                }
                else
                {
                    query = baseQuery.OrderBy(ff => ff.Field.FieldName);
                }
            }
            else
            {
                query = baseQuery.OrderBy(ff => ff.Field.FieldName);
            }

            var total = await query.CountAsync();
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)

                .Select(ff => new FavoriteFieldDto
                {
                    FieldId = ff.FieldId,
                    FieldName = ff.Field.FieldName ?? string.Empty,
                    Address = ff.Field.Address ?? string.Empty,
                    AverageRating = ff.Field.AverageRating
                })
                .ToListAsync();

            _logger.LogInformation("Lấy danh sách sân yêu thích thành công cho {Email}", account.Email);
            return (data, total, page, pageSize);
        }

        public async Task<int> AddFavoriteFieldAsync(ClaimsPrincipal user, AddFavoriteFieldDto addFavoriteFieldDto)
        {
            _logger.LogInformation("Thêm sân vào danh sách yêu thích");
            var account = await _authService.GetCurrentUserAsync(user);

            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản người dùng");
                throw new UnauthorizedAccessException("Invalid or missing token");
            }

            if (account.Role != "User")
            {
                _logger.LogWarning("Vai trò không hợp lệ để thêm sân yêu thích: {Role}", account.Role);
                throw new UnauthorizedAccessException("Only users can add favorite fields.");
            }

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
            if (userEntity == null)
            {
                _logger.LogWarning("Không tìm thấy thông tin người dùng cho AccountId: {AccountId}", account.AccountId);
                throw new ArgumentException("Thông tin người dùng không tồn tại.");
            }

            var field = await _unitOfWork.Repository<Field>()
                .FindSingleAsync(f => f.FieldId == addFavoriteFieldDto.FieldId && f.DeletedAt == null);
            if (field == null)
            {
                _logger.LogWarning("Không tìm thấy sân với FieldId: {FieldId}", addFavoriteFieldDto.FieldId);
                throw new ArgumentException("Field not found");
            }

            var existingFavorite = await _unitOfWork.Repository<FavoriteField>()
                .FindSingleAsync(ff => ff.UserId == userEntity.UserId && ff.FieldId == addFavoriteFieldDto.FieldId);
            if (existingFavorite != null)
            {
                _logger.LogWarning("Sân đã có trong danh sách yêu thích: {FieldId}", addFavoriteFieldDto.FieldId);
                throw new ArgumentException("Field is already in favorites");
            }

            var favoriteField = new FavoriteField
            {
                UserId = userEntity.UserId,
                FieldId = addFavoriteFieldDto.FieldId,
                AddedDate = DateTime.UtcNow
            };

            await _unitOfWork.Repository<FavoriteField>().AddAsync(favoriteField);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Thêm sân yêu thích thành công cho {Email}, FieldId: {FieldId}", account.Email, addFavoriteFieldDto.FieldId);
            return favoriteField.FavoriteId;
        }

        public async Task RemoveFavoriteFieldAsync(ClaimsPrincipal user, int fieldId)
        {
            _logger.LogInformation("Xóa sân khỏi danh sách yêu thích");
            var account = await _authService.GetCurrentUserAsync(user);

            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản người dùng");
                throw new UnauthorizedAccessException("Invalid or missing token");
            }

            if (account.Role != "User")
            {
                _logger.LogWarning("Vai trò không hợp lệ để xóa sân yêu thích: {Role}", account.Role);
                throw new UnauthorizedAccessException("Only users can remove favorite fields.");
            }

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
            if (userEntity == null)
            {
                _logger.LogWarning("Không tìm thấy thông tin người dùng cho AccountId: {AccountId}", account.AccountId);
                throw new ArgumentException("Thông tin người dùng không tồn tại.");
            }

            var favoriteField = await _unitOfWork.Repository<FavoriteField>()
                .FindSingleAsync(ff => ff.UserId == userEntity.UserId && ff.FieldId == fieldId);
            if (favoriteField == null)
            {
                _logger.LogWarning("Sân không có trong danh sách yêu thích: {FieldId}", fieldId);
                throw new ArgumentException("Field is not in favorites");
            }

            _unitOfWork.Repository<FavoriteField>().Remove(favoriteField);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Xóa sân yêu thích thành công cho {Email}, FieldId: {FieldId}", account.Email, fieldId);
        }

        public async Task<(IList<UserReviewDto> Data, int Total, int Page, int PageSize)> GetUserReviewsAsync(
            ClaimsPrincipal user, string? sort, int page, int pageSize)
        {
            _logger.LogInformation("Lấy danh sách đánh giá của người dùng");
            var account = await _authService.GetCurrentUserAsync(user);

            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản người dùng");
                throw new UnauthorizedAccessException("Invalid or missing token");
            }

            if (account.Role != "User")
            {
                _logger.LogWarning("Vai trò không hợp lệ để xem đánh giá: {Role}", account.Role);
                throw new UnauthorizedAccessException("Only users can access reviews.");
            }

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
            if (userEntity == null)
            {
                _logger.LogWarning("Không tìm thấy thông tin người dùng cho AccountId: {AccountId}", account.AccountId);
                throw new ArgumentException("Thông tin người dùng không tồn tại.");
            }

            var baseQuery = _unitOfWork.Repository<Review>()
                .FindQueryable(r => r.UserId == userEntity.UserId && r.IsVisible)
                .Include(r => r.Field);

            IQueryable<Review> query;
            if (!string.IsNullOrEmpty(sort))
            {
                var sortParts = sort.Split(':');
                var sortField = sortParts[0];
                var sortDirection = sortParts.Length > 1 ? sortParts[1].ToLower() : "asc";

                if (sortField == "CreatedAt")
                {
                    query = sortDirection == "desc"
                        ? baseQuery.OrderByDescending(r => r.CreatedAt)
                        : baseQuery.OrderBy(r => r.CreatedAt);
                }
                else
                {
                    query = baseQuery.OrderByDescending(r => r.CreatedAt);
                }
            }
            else
            {
                query = baseQuery.OrderByDescending(r => r.CreatedAt);
            }

            var total = await query.CountAsync();
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new UserReviewDto
                {
                    ReviewId = r.ReviewId,
                    FieldName = r.Field.FieldName ?? string.Empty,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            _logger.LogInformation("Lấy danh sách đánh giá thành công cho {Email}", account.Email);
            return (data, total, page, pageSize);
        }
        public async Task ClearSearchHistoryAsync(ClaimsPrincipal user)
        {
            _logger.LogInformation("Clearing search history for user");
            var account = await _authService.GetCurrentUserAsync(user);

            if (account == null)
            {
                _logger.LogWarning("User account not found");
                throw new UnauthorizedAccessException("Invalid or missing token");
            }

            if (account.Role != "User")
            {
                _logger.LogWarning("Invalid role for clearing search history: {Role}", account.Role);
                throw new UnauthorizedAccessException("Only users can clear search history.");
            }

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
            if (userEntity == null)
            {
                _logger.LogWarning("User not found for AccountId: {AccountId}", account.AccountId);
                throw new ArgumentException("User information not found.");
            }

            var searchHistory = await _unitOfWork.Repository<SearchHistory>()
                .FindQueryable(sh => sh.UserId == userEntity.UserId && sh.DeletedAt == null)
                .ToListAsync();

            foreach (var record in searchHistory)
            {
                record.DeletedAt = DateTime.UtcNow;
                _unitOfWork.Repository<SearchHistory>().Update(record);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Cleared search history successfully for {Email}", account.Email);
        }
    }
}