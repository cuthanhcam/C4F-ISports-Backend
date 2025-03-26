using System;
using System.Linq;
using System.Security.Claims;
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

        public async Task<User> GetUserProfileAsync(ClaimsPrincipal user)
        {
            return await GetCurrentUserAsync(user);
        }

        public async Task UpdateUserProfileAsync(ClaimsPrincipal user, UpdateProfileDto updateProfileDto)
        {
            var dbUser = await GetCurrentUserAsync(user);

            // Cập nhật thông tin mới
            dbUser.FullName = updateProfileDto.FullName;
            dbUser.Phone = updateProfileDto.Phone;
            dbUser.Gender = updateProfileDto.Gender;
            dbUser.DateOfBirth = updateProfileDto.DateOfBirth;
            dbUser.AvatarUrl = updateProfileDto.AvatarUrl;

            _unitOfWork.Users.Update(dbUser);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PaginatedResponse<Booking>> GetUserBookingsAsync(ClaimsPrincipal user, string status, DateTime? date, string sort, int page, int pageSize)
        {
            var dbUser = await GetCurrentUserAsync(user);
            var query = _unitOfWork.Bookings.GetAll()
                .Include(b => b.Field)
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

            // Sắp xếp
            if (!string.IsNullOrEmpty(sort))
            {
                var sortParts = sort.Split(':');
                var sortField = sortParts[0];
                var sortDirection = sortParts.Length > 1 ? sortParts[1].ToLower() : "asc";

                query = sortField switch
                {
                    "BookingDate" => sortDirection == "desc" ? query.OrderByDescending(b => b.BookingDate) : query.OrderBy(b => b.BookingDate),
                    "CreatedAt" => sortDirection == "desc" ? query.OrderByDescending(b => b.CreatedAt) : query.OrderBy(b => b.CreatedAt),
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
            var dbUser = await GetCurrentUserAsync(user);
            var account = dbUser.Account;
            account.IsActive = false;
            _unitOfWork.Accounts.Update(account);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PaginatedResponse<FavoriteField>> GetFavoriteFieldsAsync(ClaimsPrincipal user, string sort, int page, int pageSize)
        {
            var dbUser = await GetCurrentUserAsync(user);
            var query = _unitOfWork.FavoriteFields.GetAll()
                .Include(ff => ff.Field)
                    .ThenInclude(f => f.Sport)
                .Where(ff => ff.UserId == dbUser.UserId)
                .AsQueryable();

            // Sắp xếp
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
            var dbUser = await GetCurrentUserAsync(user);
            
            // Check if field exists
            var field = await _unitOfWork.Fields.GetAll().FirstOrDefaultAsync(f => f.FieldId == fieldId);
            if (field == null)
            {
                throw new Exception("Field does not exist.");
            }

            // Check if already in favorites
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
            var dbUser = await GetCurrentUserAsync(user);
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