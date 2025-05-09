using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos.User;
using api.Dtos;
using api.Models;
using api.Services;

namespace api.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileResponseDto> GetUserProfileAsync(ClaimsPrincipal user);
        Task UpdateUserProfileAsync(ClaimsPrincipal user, UpdateProfileDto updateProfileDto);
        Task UpdateUserAvatarAsync(ClaimsPrincipal user, UpdateAvatarDto updateAvatarDto, CloudinaryService cloudinaryService);
        Task<PaginatedResponse<Booking>> GetUserBookingsAsync(ClaimsPrincipal user, string status, DateTime? date, string sort, int page, int pageSize);
        Task DeactivateUserAsync(ClaimsPrincipal user);
        Task<PaginatedResponse<FavoriteField>> GetFavoriteFieldsAsync(ClaimsPrincipal user, string sort, int page, int pageSize);
        Task AddFavoriteFieldAsync(ClaimsPrincipal user, int fieldId);
        Task RemoveFavoriteFieldAsync(ClaimsPrincipal user, int fieldId);
    }
}