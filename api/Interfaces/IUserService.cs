using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos.User;
using api.Helpers;
using api.Models;

namespace api.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserProfileAsync(ClaimsPrincipal user);
        Task UpdateUserProfileAsync(ClaimsPrincipal user, UpdateProfileDto updateProfileDto);
        Task<PagedResult<Booking>> GetUserBookingsAsync(ClaimsPrincipal user, string status, DateTime? date, string sort, int page, int pageSize);
        Task DeactivateUserAsync(ClaimsPrincipal user);
        Task<PagedResult<FavoriteField>> GetFavoriteFieldsAsync(ClaimsPrincipal user, string sort, int page, int pageSize);
        Task AddFavoriteFieldAsync(ClaimsPrincipal user, int fieldId);
        Task RemoveFavoriteFieldAsync(ClaimsPrincipal user, int fieldId);
    }
}