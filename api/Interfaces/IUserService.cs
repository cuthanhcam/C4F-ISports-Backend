using System.Security.Claims;
using api.Dtos.User;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace api.Interfaces
{
    public interface IUserService
    {
        /// <summary>
        /// Lấy thông tin hồ sơ của người dùng hiện tại.
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        /// <returns>Thông tin hồ sơ người dùng.</returns>
        Task<UserProfileDto> GetProfileAsync(ClaimsPrincipal user);

        /// <summary>
        /// Cập nhật thông tin hồ sơ của người dùng hiện tại.
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        /// <param name="updateProfileDto">Thông tin cần cập nhật.</param>
        /// <returns>Thông tin hồ sơ đã cập nhật.</returns>
        Task<UserProfileDto> UpdateProfileAsync(ClaimsPrincipal user, UpdateProfileDto updateProfileDto);

        /// <summary>
        /// Xóa hồ sơ của người dùng hiện tại (soft delete).
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        Task DeleteProfileAsync(ClaimsPrincipal user);

        /// <summary>
        /// Lấy điểm loyalty của người dùng hiện tại.
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        /// <returns>Thông tin điểm loyalty.</returns>
        Task<LoyaltyPointsDto> GetLoyaltyPointsAsync(ClaimsPrincipal user);

        /// <summary>
        /// Lấy danh sách sân yêu thích của người dùng hiện tại.
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        /// <param name="sort">Sắp xếp theo trường (FieldName:asc/desc).</param>
        /// <param name="page">Số trang (mặc định: 1).</param>
        /// <param name="pageSize">Số mục mỗi trang (mặc định: 10).</param>
        /// <returns>Danh sách sân yêu thích với phân trang.</returns>
        Task<(IList<FavoriteFieldDto> Data, int Total, int Page, int PageSize)> GetFavoriteFieldsAsync(
            ClaimsPrincipal user, string? sort, int page, int pageSize);

        /// <summary>
        /// Thêm sân vào danh sách yêu thích của người dùng.
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        /// <param name="addFavoriteFieldDto">Thông tin sân cần thêm.</param>
        /// <returns>ID của mục yêu thích đã thêm.</returns>
        Task<int> AddFavoriteFieldAsync(ClaimsPrincipal user, AddFavoriteFieldDto addFavoriteFieldDto);

        /// <summary>
        /// Xóa sân khỏi danh sách yêu thích của người dùng.
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        /// <param name="fieldId">ID của sân cần xóa.</param>
        Task RemoveFavoriteFieldAsync(ClaimsPrincipal user, int fieldId);

        /// <summary>
        /// Lấy lịch sử đặt sân của người dùng hiện tại.
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        /// <param name="status">Lọc theo trạng thái (Confirmed, Pending, Cancelled).</param>
        /// <param name="startDate">Ngày bắt đầu lọc (tùy chọn).</param>
        /// <param name="endDate">Ngày kết thúc lọc (tùy chọn).</param>
        /// <param name="sort">Sắp xếp theo trường (BookingDate:asc/desc).</param>
        /// <param name="page">Số trang (mặc định: 1).</param>
        /// <param name="pageSize">Số mục mỗi trang (mặc định: 10).</param>
        /// <returns>Danh sách lịch sử đặt sân với phân trang.</returns>
        Task<(IList<BookingHistoryDto> Data, int Total, int Page, int PageSize)> GetBookingHistoryAsync(
            ClaimsPrincipal user, string? status, DateTime? startDate, DateTime? endDate, string? sort, int page, int pageSize);    

        /// <summary>
        /// Lấy lịch sử tìm kiếm của người dùng hiện tại.
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        /// <param name="startDate">Ngày bắt đầu lọc (tùy chọn).</param>
        /// <param name="endDate">Ngày kết thúc lọc (tùy chọn).</param>
        /// <param name="page">Số trang (mặc định: 1).</param>
        /// <param name="pageSize">Số mục mỗi trang (mặc định: 10).</param>
        /// <returns>Danh sách lịch sử tìm kiếm với phân trang.</returns>
        Task<(IList<SearchHistoryDto> Data, int Total, int Page, int PageSize)> GetSearchHistoryAsync(
            ClaimsPrincipal user, DateTime? startDate, DateTime? endDate, int page, int pageSize);

        /// <summary>
        /// Xóa toàn bộ lịch sử tìm kiếm của người dùng hiện tại (soft delete).
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        Task ClearSearchHistoryAsync(ClaimsPrincipal user);

        /// <summary>
        /// Lấy danh sách đánh giá của người dùng hiện tại.
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        /// <param name="sort">Sắp xếp theo trường (CreatedAt:asc/desc).</param>
        /// <param name="page">Số trang (mặc định: 1).</param>
        /// <param name="pageSize">Số mục mỗi trang (mặc định: 10).</param>
        /// <returns>Danh sách đánh giá với phân trang.</returns>
        Task<(IList<UserReviewDto> Data, int Total, int Page, int PageSize)> GetUserReviewsAsync(
            ClaimsPrincipal user, string? sort, int page, int pageSize);   
    }
}