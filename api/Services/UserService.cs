using System.Security.Claims;
using api.Dtos.User;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService> _logger;
        private readonly IAuthService _authService;

        public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger, IAuthService authService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _authService = authService;
        }

        public async Task<UserProfileDto> GetProfileAsync(ClaimsPrincipal user)
        {
            _logger.LogInformation("Lấy thông tin hồ sơ người dùng");
            var account = await _authService.GetCurrentUserAsync(user);

            UserProfileDto profile;
            if (account.Role == "User")
            {
                var userEntity = await _unitOfWork.Repository<User>()
                    .FindSingleAsync(u => u.AccountId == account.AccountId);
                if (userEntity == null)
                {
                    _logger.LogWarning("Không tìm thấy thông tin người dùng cho AccountId: {AccountId}", account.AccountId);
                    throw new ArgumentException("Thông tin người dùng không tồn tại.");
                }

                profile = new UserProfileDto
                {
                    UserId = userEntity.UserId,
                    FullName = userEntity.FullName,
                    Email = account.Email,
                    Phone = userEntity.Phone,
                    City = userEntity.City,
                    District = userEntity.District,
                    AvatarUrl = userEntity.AvatarUrl
                };
            }
            else if (account.Role == "Owner")
            {
                var ownerEntity = await _unitOfWork.Repository<Owner>()
                    .FindSingleAsync(o => o.AccountId == account.AccountId);
                if (ownerEntity == null)
                {
                    _logger.LogWarning("Không tìm thấy thông tin chủ sân cho AccountId: {AccountId}", account.AccountId);
                    throw new ArgumentException("Thông tin chủ sân không tồn tại.");
                }

                profile = new UserProfileDto
                {
                    UserId = ownerEntity.OwnerId,
                    FullName = ownerEntity.FullName,
                    Email = account.Email,
                    Phone = ownerEntity.Phone
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

            UserProfileDto updatedProfile;
            if (account.Role == "User")
            {
                var userEntity = await _unitOfWork.Repository<User>()
                    .FindSingleAsync(u => u.AccountId == account.AccountId);
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
                    AvatarUrl = userEntity.AvatarUrl
                };
            }
            else if (account.Role == "Owner")
            {
                var ownerEntity = await _unitOfWork.Repository<Owner>()
                    .FindSingleAsync(o => o.AccountId == account.AccountId);
                if (ownerEntity == null)
                {
                    _logger.LogWarning("Không tìm thấy thông tin chủ sân cho AccountId: {AccountId}", account.AccountId);
                    throw new ArgumentException("Thông tin chủ sân không tồn tại.");
                }

                ownerEntity.FullName = updateProfileDto.FullName;
                ownerEntity.Phone = updateProfileDto.Phone;
                ownerEntity.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<Owner>().Update(ownerEntity);
                await _unitOfWork.SaveChangesAsync();

                updatedProfile = new UserProfileDto
                {
                    UserId = ownerEntity.OwnerId,
                    FullName = ownerEntity.FullName,
                    Email = account.Email,
                    Phone = ownerEntity.Phone
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
            _logger.LogInformation("Xóa tài khoản người dùng");
            var account = await _authService.GetCurrentUserAsync(user);

            // Sử dụng transaction để đảm bảo tính toàn vẹn dữ liệu
            var strategy = _unitOfWork.Context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    if (account.Role == "User")
                    {
                        var userEntity = await _unitOfWork.Repository<User>()
                            .FindSingleAsync(u => u.AccountId == account.AccountId);
                        if (userEntity != null)
                        {
                            _unitOfWork.Repository<User>().Remove(userEntity);
                        }
                    }
                    else if (account.Role == "Owner")
                    {
                        var ownerEntity = await _unitOfWork.Repository<Owner>()
                            .FindSingleAsync(o => o.AccountId == account.AccountId);
                        if (ownerEntity != null)
                        {
                            // Kiểm tra xem owner có sân nào đang hoạt động không
                            var hasActiveFields = await _unitOfWork.Repository<Field>()
                                .FindQueryable(f => f.OwnerId == ownerEntity.OwnerId && f.Status == "Active")
                                .AnyAsync();
                            if (hasActiveFields)
                            {
                                _logger.LogWarning("Không thể xóa tài khoản Owner có sân đang hoạt động: {OwnerId}", ownerEntity.OwnerId);
                                throw new InvalidOperationException("Không thể xóa tài khoản vì có sân đang hoạt động.");
                            }
                            _unitOfWork.Repository<Owner>().Remove(ownerEntity);
                        }
                    }

                    _unitOfWork.Repository<Account>().Remove(account);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                    _logger.LogInformation("Xóa tài khoản thành công cho {Email}", account.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi xóa tài khoản cho {Email}. Rollback transaction.", account.Email);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            });
        }

        public async Task<(IList<BookingHistoryDto> Data, int Total, int Page, int PageSize)> GetBookingHistoryAsync(ClaimsPrincipal user, string? status, string? sort, int page, int pageSize)
        {
            _logger.LogInformation("Lấy lịch sử đặt sân cho người dùng");
            var account = await _authService.GetCurrentUserAsync(user);
            if (account.Role != "User")
            {
                _logger.LogWarning("Vai trò không hợp lệ để xem lịch sử đặt sân: {Role}", account.Role);
                throw new UnauthorizedAccessException("Chỉ người dùng mới có thể xem lịch sử đặt sân.");
            }

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId);
            if (userEntity == null)
            {
                _logger.LogWarning("Không tìm thấy thông tin người dùng cho AccountId: {AccountId}", account.AccountId);
                throw new ArgumentException("Thông tin người dùng không tồn tại.");
            }

            IQueryable<Booking> query = _unitOfWork.Repository<Booking>()
                .FindQueryable(b => b.UserId == userEntity.UserId)
                .Include(b => b.SubField)
                .ThenInclude(sf => sf.Field);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.Status == status);
            }

            // Sắp xếp
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

            // Phân trang
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
                    Status = b.Status
                })
                .ToListAsync();

            _logger.LogInformation("Lấy lịch sử đặt sân thành công cho {Email}", account.Email);
            return (data, total, page, pageSize);
        }

        public async Task<(IList<FavoriteFieldDto> Data, int Total, int Page, int PageSize)> GetFavoriteFieldsAsync(ClaimsPrincipal user, string? sort, int page, int pageSize)
        {
            _logger.LogInformation("Lấy danh sách sân yêu thích");
            var account = await _authService.GetCurrentUserAsync(user);
            if (account.Role != "User")
            {
                _logger.LogWarning("Vai trò không hợp lệ để xem sân yêu thích: {Role}", account.Role);
                throw new UnauthorizedAccessException("Chỉ người dùng mới có thể xem sân yêu thích.");
            }

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId);
            if (userEntity == null)
            {
                _logger.LogWarning("Không tìm thấy thông tin người dùng cho AccountId: {AccountId}", account.AccountId);
                throw new ArgumentException("Thông tin người dùng không tồn tại.");
            }

            // Get base query with includes
            var baseQuery = _unitOfWork.Repository<FavoriteField>()
                .FindQueryable(ff => ff.UserId == userEntity.UserId)
                .Include(ff => ff.Field);

            // Apply sorting to create ordered query
            IQueryable<FavoriteField> query;
            
            // Sắp xếp
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

            // Phân trang
            var total = await query.CountAsync();
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ff => new FavoriteFieldDto
                {
                    FieldId = ff.FieldId,
                    FieldName = ff.Field.FieldName,
                    Address = ff.Field.Address,
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
            if (account.Role != "User")
            {
                _logger.LogWarning("Vai trò không hợp lệ để thêm sân yêu thích: {Role}", account.Role);
                throw new UnauthorizedAccessException("Chỉ người dùng mới có thể thêm sân yêu thích.");
            }

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId);
            if (userEntity == null)
            {
                _logger.LogWarning("Không tìm thấy thông tin người dùng cho AccountId: {AccountId}", account.AccountId);
                throw new ArgumentException("Thông tin người dùng không tồn tại.");
            }

            var field = await _unitOfWork.Repository<Field>().GetByIdAsync(addFavoriteFieldDto.FieldId);
            if (field == null)
            {
                _logger.LogWarning("Không tìm thấy sân với FieldId: {FieldId}", addFavoriteFieldDto.FieldId);
                throw new ArgumentException("Sân không tồn tại.");
            }

            var existingFavorite = await _unitOfWork.Repository<FavoriteField>()
                .FindSingleAsync(ff => ff.UserId == userEntity.UserId && ff.FieldId == addFavoriteFieldDto.FieldId);
            if (existingFavorite != null)
            {
                _logger.LogWarning("Sân đã có trong danh sách yêu thích: {FieldId}", addFavoriteFieldDto.FieldId);
                throw new ArgumentException("Sân đã có trong danh sách yêu thích.");
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
            if (account.Role != "User")
            {
                _logger.LogWarning("Vai trò không hợp lệ để xóa sân yêu thích: {Role}", account.Role);
                throw new UnauthorizedAccessException("Chỉ người dùng mới có thể xóa sân yêu thích.");
            }

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId);
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
                throw new ArgumentException("Sân không có trong danh sách yêu thích.");
            }

            _unitOfWork.Repository<FavoriteField>().Remove(favoriteField);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Xóa sân yêu thích thành công cho {Email}, FieldId: {FieldId}", account.Email, fieldId);
        }

        public async Task<(IList<UserReviewDto> Data, int Total, int Page, int PageSize)> GetUserReviewsAsync(ClaimsPrincipal user, string? sort, int page, int pageSize)
        {
            _logger.LogInformation("Lấy danh sách đánh giá của người dùng");
            var account = await _authService.GetCurrentUserAsync(user);
            if (account.Role != "User")
            {
                _logger.LogWarning("Vai trò không hợp lệ để xem đánh giá: {Role}", account.Role);
                throw new UnauthorizedAccessException("Chỉ người dùng mới có thể xem đánh giá.");
            }

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId);
            if (userEntity == null)
            {
                _logger.LogWarning("Không tìm thấy thông tin người dùng cho AccountId: {AccountId}", account.AccountId);
                throw new ArgumentException("Thông tin người dùng không tồn tại.");
            }

            var baseQuery = _unitOfWork.Repository<Review>()
                .FindQueryable(r => r.UserId == userEntity.UserId)
                .Include(r => r.Field);

            // Sắp xếp
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

            // Phân trang
            var total = await query.CountAsync();
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new UserReviewDto
                {
                    ReviewId = r.ReviewId,
                    FieldName = r.Field.FieldName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            _logger.LogInformation("Lấy danh sách đánh giá thành công cho {Email}", account.Email);
            return (data, total, page, pageSize);
        }
    }
}