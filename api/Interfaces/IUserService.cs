using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos.User;

namespace api.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetProfileAsync(ClaimsPrincipal user);
        Task<UserProfileDto> UpdateProfileAsync(ClaimsPrincipal user, UpdateProfileDto updateProfileDto);
        Task DeleteProfileAsync(ClaimsPrincipal user);
        Task<(IList<BookingHistoryDto> Data, int Total, int Page, int PageSize)> GetBookingHistoryAsync(ClaimsPrincipal user, string? status, string? sort, int page, int pageSize);
        Task<(IList<FavoriteFieldDto> Data, int Total, int Page, int PageSize)> GetFavoriteFieldsAsync(ClaimsPrincipal user, string? sort, int page, int pageSize);
        Task<int> AddFavoriteFieldAsync(ClaimsPrincipal user, AddFavoriteFieldDto addFavoriteFieldDto);
        Task RemoveFavoriteFieldAsync(ClaimsPrincipal user, int fieldId);
        Task<(IList<UserReviewDto> Data, int Total, int Page, int PageSize)> GetUserReviewsAsync(ClaimsPrincipal user, string? sort, int page, int pageSize);
    }
}