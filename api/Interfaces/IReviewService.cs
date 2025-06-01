using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos.Review;

namespace api.Interfaces
{
    public interface IReviewService
    {
        /// <summary>
        /// Tạo đánh giá mới cho một sân.
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        /// <param name="createReviewDto">Thông tin đánh giá cần tạo.</param>
        /// <returns>Thông tin đánh giá đã tạo.</returns>
        Task<ReviewResponseDto> CreateReviewAsync(ClaimsPrincipal user, CreateReviewDto createReviewDto);

        /// <summary>
        /// Cập nhật đánh giá hiện có.
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        /// <param name="reviewId">ID của đánh giá cần cập nhật.</param>
        /// <param name="updateReviewDto">Thông tin đánh giá cần cập nhật.</param>
        /// <returns>Thông tin đánh giá đã cập nhật.</returns>
        Task<ReviewResponseDto> UpdateReviewAsync(ClaimsPrincipal user, int reviewId, UpdateReviewDto updateReviewDto);

        /// <summary>
        /// Xóa đánh giá (soft delete).
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        /// <param name="reviewId">ID của đánh giá cần xóa.</param>
        Task DeleteReviewAsync(ClaimsPrincipal user, int reviewId);
    }
}