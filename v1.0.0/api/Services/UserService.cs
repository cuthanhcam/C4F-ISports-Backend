using System;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using api.Data;
using api.Dtos;
using api.Dtos.User;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private async Task<User> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdClaim) || !int.TryParse(accountIdClaim, out int accountId))
            {
                throw new Exception("Token người dùng không hợp lệ.");
            }

            var dbUser = await _unitOfWork.Users.GetAll()
                .Include(u => u.Account)
                .FirstOrDefaultAsync(u => u.AccountId == accountId);
            if (dbUser == null)
            {
                throw new Exception("Không tìm thấy người dùng.");
            }

            return dbUser;
        }

        private async Task<Account> GetCurrentAccountAsync(ClaimsPrincipal user)
        {
            var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdClaim) || !int.TryParse(accountIdClaim, out int accountId))
            {
                throw new Exception("Token người dùng không hợp lệ.");
            }

            var account = await _unitOfWork.Accounts.GetAll()
                .FirstOrDefaultAsync(a => a.AccountId == accountId);
            if (account == null)
            {
                throw new Exception("Không tìm thấy tài khoản.");
            }

            return account;
        }

        public async Task<UserProfileResponseDto> GetUserProfileAsync(ClaimsPrincipal user)
        {
            var account = await GetCurrentAccountAsync(user);
            var role = account.Role;

            if (role == "User")
            {
                var dbUser = await _unitOfWork.Users.GetAll()
                    .Include(u => u.Account)
                    .FirstOrDefaultAsync(u => u.AccountId == account.AccountId);
                if (dbUser == null)
                {
                    throw new Exception("Không tìm thấy thông tin người dùng.");
                }

                return new UserProfileResponseDto
                {
                    Email = dbUser.Email,
                    Role = account.Role,
                    FullName = dbUser.FullName,
                    Phone = dbUser.Phone,
                    Gender = dbUser.Gender,
                    DateOfBirth = dbUser.DateOfBirth?.ToString("yyyy-MM-dd"),
                    AvatarUrl = dbUser.AvatarUrl
                };
            }
            else if (role == "Owner")
            {
                var dbOwner = await _unitOfWork.Owners.GetAll()
                    .Include(o => o.Account)
                    .FirstOrDefaultAsync(o => o.AccountId == account.AccountId);
                if (dbOwner == null)
                {
                    throw new Exception("Không tìm thấy thông tin chủ sân.");
                }

                return new UserProfileResponseDto
                {
                    Email = dbOwner.Email,
                    Role = account.Role,
                    FullName = dbOwner.FullName,
                    Phone = dbOwner.Phone,
                    Gender = null, // Owner không có trường Gender
                    DateOfBirth = null, // Owner không có trường DateOfBirth
                    // AvatarUrl = null // Owner không có trường AvatarUrl 
                };
            }
            else
            {
                throw new Exception("Vai trò không được hỗ trợ.");
            }
        }

        public async Task UpdateUserProfileAsync(ClaimsPrincipal user, UpdateProfileDto updateProfileDto)
        {
            var account = await GetCurrentAccountAsync(user);
            var role = account.Role;

            // Kiểm tra định dạng số điện thoại
            if (!string.IsNullOrEmpty(updateProfileDto.Phone) && !Regex.IsMatch(updateProfileDto.Phone, @"^[0-9]{10}$"))
            {
                throw new Exception("Số điện thoại không hợp lệ, phải là 10 chữ số.");
            }

            if (role == "User")
            {
                var dbUser = await _unitOfWork.Users.GetAll()
                    .FirstOrDefaultAsync(u => u.AccountId == account.AccountId);
                if (dbUser == null)
                {
                    throw new Exception("Không tìm thấy người dùng.");
                }

                // Cập nhật các trường bắt buộc
                dbUser.FullName = updateProfileDto.FullName;
                dbUser.Phone = updateProfileDto.Phone;
                dbUser.Gender = updateProfileDto.Gender;
                dbUser.DateOfBirth = updateProfileDto.DateOfBirth;

                _unitOfWork.Users.Update(dbUser);
            }
            else if (role == "Owner")
            {
                var dbOwner = await _unitOfWork.Owners.GetAll()
                    .FirstOrDefaultAsync(o => o.AccountId == account.AccountId);
                if (dbOwner == null)
                {
                    throw new Exception("Không tìm thấy chủ sân.");
                }

                dbOwner.FullName = updateProfileDto.FullName;
                dbOwner.Phone = updateProfileDto.Phone;
                dbOwner.UpdatedAt = DateTime.UtcNow; // Cập nhật thời gian chỉnh sửa

                _unitOfWork.Owners.Update(dbOwner);
            }
            else
            {
                throw new Exception("Vai trò không được hỗ trợ.");
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateUserAvatarAsync(ClaimsPrincipal user, UpdateAvatarDto updateAvatarDto, CloudinaryService cloudinaryService)
        {
            var account = await GetCurrentAccountAsync(user);
            var role = account.Role;

            if (role == "User")
            {
                var dbUser = await _unitOfWork.Users.GetAll()
                    .FirstOrDefaultAsync(u => u.AccountId == account.AccountId);
                if (dbUser == null)
                {
                    throw new Exception("Không tìm thấy người dùng.");
                }

                var avatarUrl = await cloudinaryService.UploadImageAsync(updateAvatarDto.AvatarFile);
                dbUser.AvatarUrl = avatarUrl;

                _unitOfWork.Users.Update(dbUser);
            }
            else if (role == "Owner")
            {
                throw new Exception("Chủ sân hiện không hỗ trợ cập nhật avatar.");
            }
            else
            {
                throw new Exception("Vai trò không được hỗ trợ.");
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PaginatedResponse<Booking>> GetUserBookingsAsync(ClaimsPrincipal user, string status, DateTime? date, string sort, int page, int pageSize)
        {
            var dbUser = await GetCurrentUserAsync(user); // Chỉ áp dụng cho User
            var query = _unitOfWork.Bookings.GetAll()
                .Include(b => b.SubField)
                .Where(b => b.UserId == dbUser.UserId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.Status == status);
            }

            if (date.HasValue)
            {
                query = query.Where(b => b.BookingDate.Date == date.Value.Date);
            }

            if (!string.IsNullOrEmpty(sort))
            {
                var sortParts = sort.Split(':');
                var sortField = sortParts[0];
                var sortDirection = sortParts.Length > 1 ? sortParts[1].ToLower() : "asc";

                query = sortField switch
                {
                    "BookingDate" => sortDirection == "desc" ? query.OrderByDescending(b => b.BookingDate) : query.OrderBy(b => b.BookingDate),
                    "CreatedAt" => sortDirection == "desc" ? query.OrderByDescending(b => b.CreatedAt) : query.OrderBy(b => b.CreatedAt), // Sửa lỗi typo
                    _ => query.OrderBy(b => b.BookingDate)
                };
            }
            else
            {
                query = query.OrderBy(b => b.BookingDate);
            }

            var totalItems = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedResponse<Booking>
            {
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                Items = items
            };
        }

        public async Task DeactivateUserAsync(ClaimsPrincipal user)
        {
            var account = await GetCurrentAccountAsync(user);
            account.IsActive = false;
            _unitOfWork.Accounts.Update(account);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PaginatedResponse<FavoriteField>> GetFavoriteFieldsAsync(ClaimsPrincipal user, string sort, int page, int pageSize)
        {
            var dbUser = await GetCurrentUserAsync(user); // Chỉ áp dụng cho User
            var query = _unitOfWork.FavoriteFields.GetAll()
                .Include(ff => ff.Field)
                    .ThenInclude(f => f.Sport)
                .Where(ff => ff.UserId == dbUser.UserId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(sort))
            {
                var sortParts = sort.Split(':');
                var sortField = sortParts[0];
                var sortDirection = sortParts.Length > 1 ? sortParts[1].ToLower() : "asc";

                query = sortField switch
                {
                    "AddedDate" => sortDirection == "desc" ? query.OrderByDescending(ff => ff.AddedDate) : query.OrderBy(ff => ff.AddedDate),
                    "FieldName" => sortDirection == "desc" ? query.OrderByDescending(ff => ff.Field.FieldName) : query.OrderBy(ff => ff.Field.FieldName),
                    _ => query.OrderBy(ff => ff.AddedDate)
                };
            }
            else
            {
                query = query.OrderBy(ff => ff.AddedDate);
            }

            var totalItems = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedResponse<FavoriteField>
            {
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                Items = items
            };
        }

        public async Task AddFavoriteFieldAsync(ClaimsPrincipal user, int fieldId)
        {
            var dbUser = await GetCurrentUserAsync(user); // Chỉ áp dụng cho User

            var field = await _unitOfWork.Fields.GetAll().FirstOrDefaultAsync(f => f.FieldId == fieldId);
            if (field == null)
            {
                throw new Exception("Field does not exist.");
            }

            var existingFavorite = await _unitOfWork.FavoriteFields.GetAll()
                .FirstOrDefaultAsync(f => f.FieldId == fieldId && f.UserId == dbUser.UserId);
            if (existingFavorite != null)
            {
                throw new Exception("Field already in favorites.");
            }

            var favorite = new FavoriteField
            {
                UserId = dbUser.UserId,
                FieldId = fieldId,
                AddedDate = DateTime.UtcNow
            };

            await _unitOfWork.FavoriteFields.AddAsync(favorite);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RemoveFavoriteFieldAsync(ClaimsPrincipal user, int fieldId)
        {
            var dbUser = await GetCurrentUserAsync(user); // Chỉ áp dụng cho User
            var favorite = await _unitOfWork.FavoriteFields.GetAll()
                .FirstOrDefaultAsync(ff => ff.UserId == dbUser.UserId && ff.FieldId == fieldId);

            if (favorite == null)
            {
                throw new Exception("Field not found in favorites.");
            }

            _unitOfWork.FavoriteFields.Delete(favorite);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}