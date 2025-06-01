using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos.Review;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReviewService> _logger;
        private readonly IAuthService _authService;

        public ReviewService(
            IUnitOfWork unitOfWork,
            ILogger<ReviewService> logger,
            IAuthService authService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _authService = authService;
        }

        public async Task<ReviewResponseDto> CreateReviewAsync(ClaimsPrincipal user, CreateReviewDto createReviewDto)
        {
            _logger.LogInformation("Tạo đánh giá mới cho sân {FieldId}", createReviewDto.FieldId);
            var account = await _authService.GetCurrentUserAsync(user);

            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản người dùng");
                throw new UnauthorizedAccessException("Invalid or missing token");
            }

            if (account.Role != "User")
            {
                _logger.LogWarning("Vai trò không hợp lệ để tạo đánh giá: {Role}", account.Role);
                throw new UnauthorizedAccessException("Chỉ người dùng có thể tạo đánh giá.");
            }

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
            if (userEntity == null)
            {
                _logger.LogWarning("Không tìm thấy thông tin người dùng cho AccountId: {AccountId}", account.AccountId);
                throw new ArgumentException("Thông tin người dùng không tồn tại.");
            }

            var booking = await _unitOfWork.Repository<Booking>()
        .FindSingleAsync(b => b.BookingId == createReviewDto.BookingId && b.UserId == userEntity.UserId && b.DeletedAt == null);
            if (booking == null)
            {
                _logger.LogWarning("Không tìm thấy booking với BookingId: {BookingId}", createReviewDto.BookingId);
                throw new ArgumentException("Booking không tồn tại hoặc không thuộc về người dùng này.");
            }

            if (booking.Status != "Confirmed")
            {
                _logger.LogWarning("Booking không hợp lệ để đánh giá: Status={Status}", booking.Status);
                throw new ArgumentException("Chỉ các booking đã xác nhận mới có thể được đánh giá.");
            }

            var field = await _unitOfWork.Repository<Field>()
                .FindSingleAsync(f => f.FieldId == createReviewDto.FieldId && f.DeletedAt == null);
            if (field == null)
            {
                _logger.LogWarning("Không tìm thấy sân với FieldId: {FieldId}", createReviewDto.FieldId);
                throw new ArgumentException("Sân không tồn tại.");
            }

            // Kiểm tra xem đã có đánh giá cho booking này chưa
            var existingReview = await _unitOfWork.Repository<Review>()
                .FindSingleAsync(r => r.UserId == userEntity.UserId && r.FieldId == createReviewDto.FieldId && r.BookingId == createReviewDto.BookingId);
            if (existingReview != null)
            {
                _logger.LogWarning("Đánh giá đã tồn tại cho BookingId: {BookingId}", createReviewDto.BookingId);
                throw new ArgumentException("Bạn đã đánh giá cho booking này.");
            }

            var review = new Review
            {
                UserId = userEntity.UserId,
                FieldId = createReviewDto.FieldId,
                BookingId = createReviewDto.BookingId,
                Rating = createReviewDto.Rating,
                Comment = createReviewDto.Comment,
                CreatedAt = DateTime.UtcNow,
                IsVisible = true
            };

            await _unitOfWork.Repository<Review>().AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            // Cập nhật AverageRating của sân
            await UpdateFieldAverageRatingAsync(createReviewDto.FieldId);

            _logger.LogInformation("Tạo đánh giá thành công cho sân {FieldId}, ReviewId: {ReviewId}", createReviewDto.FieldId, review.ReviewId);

            return new ReviewResponseDto
            {
                ReviewId = review.ReviewId,
                FieldId = review.FieldId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                Message = "Review created successfully"
            };
        }

        public async Task<ReviewResponseDto> UpdateReviewAsync(ClaimsPrincipal user, int reviewId, UpdateReviewDto updateReviewDto)
        {
            _logger.LogInformation("Cập nhật đánh giá {ReviewId}", reviewId);
            var account = await _authService.GetCurrentUserAsync(user);

            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản người dùng");
                throw new UnauthorizedAccessException("Invalid or missing token");
            }

            if (account.Role != "User")
            {
                _logger.LogWarning("Vai trò không hợp lệ để cập nhật đánh giá: {Role}", account.Role);
                throw new UnauthorizedAccessException("Chỉ người dùng có thể cập nhật đánh giá.");
            }

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
            if (userEntity == null)
            {
                _logger.LogWarning("Không tìm thấy thông tin người dùng cho AccountId: {AccountId}", account.AccountId);
                throw new ArgumentException("Thông tin người dùng không tồn tại.");
            }

            var review = await _unitOfWork.Repository<Review>()
                .FindSingleAsync(r => r.ReviewId == reviewId && r.UserId == userEntity.UserId && r.IsVisible && r.DeletedAt == null);
            if (review == null)
            {
                _logger.LogWarning("Không tìm thấy đánh giá với ReviewId: {ReviewId}", reviewId);
                throw new ArgumentException("Đánh giá không tồn tại hoặc không thuộc về người dùng này.");
            }

            review.Rating = updateReviewDto.Rating;
            review.Comment = updateReviewDto.Comment;
            review.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Review>().Update(review);
            await _unitOfWork.SaveChangesAsync();

            // Cập nhật AverageRating của sân
            await UpdateFieldAverageRatingAsync(review.FieldId);

            _logger.LogInformation("Cập nhật đánh giá thành công cho ReviewId: {ReviewId}", reviewId);

            return new ReviewResponseDto
            {
                ReviewId = review.ReviewId,
                FieldId = review.FieldId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                Message = "Review updated successfully"
            };
        }

        public async Task DeleteReviewAsync(ClaimsPrincipal user, int reviewId)
        {
            _logger.LogInformation("Xóa đánh giá {ReviewId}", reviewId);
            var account = await _authService.GetCurrentUserAsync(user);

            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản người dùng");
                throw new UnauthorizedAccessException("Invalid or missing token");
            }

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
            if (userEntity == null && account.Role != "Admin")
            {
                _logger.LogWarning("Không tìm thấy thông tin người dùng cho AccountId: {AccountId}", account.AccountId);
                throw new ArgumentException("Thông tin người dùng không tồn tại.");
            }

            var review = await _unitOfWork.Repository<Review>()
                .FindSingleAsync(r => r.ReviewId == reviewId && r.IsVisible && r.DeletedAt == null);
            if (review == null)
            {
                _logger.LogWarning("Không tìm thấy đánh giá với ReviewId: {ReviewId}", reviewId);
                throw new ArgumentException("Đánh giá không tồn tại.");
            }

            // Kiểm tra quyền xóa (User là tác giả hoặc Admin)
            if (account.Role != "User" && account.Role != "Admin")
            {
                _logger.LogWarning("Vai trò không hợp lệ để xóa đánh giá: {Role}", account.Role);
                throw new UnauthorizedAccessException("Chỉ tác giả của đánh giá hoặc Admin có thể xóa.");
            }

            if (account.Role == "User" && review.UserId != userEntity?.UserId)
            {
                _logger.LogWarning("Người dùng không có quyền xóa đánh giá: ReviewId={ReviewId}, UserId={UserId}", reviewId, userEntity?.UserId);
                throw new UnauthorizedAccessException("Chỉ tác giả của đánh giá hoặc Admin có thể xóa.");
            }

            review.IsVisible = false;
            review.DeletedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Review>().Update(review);
            await _unitOfWork.SaveChangesAsync();

            // Cập nhật AverageRating của sân
            await UpdateFieldAverageRatingAsync(review.FieldId);

            _logger.LogInformation("Xóa đánh giá thành công cho ReviewId: {ReviewId}", reviewId);
        }

        private async Task UpdateFieldAverageRatingAsync(int fieldId)
        {
            var reviews = await _unitOfWork.Repository<Review>()
                .FindQueryable(r => r.FieldId == fieldId && r.IsVisible && r.DeletedAt == null)
                .ToListAsync();

            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            var field = await _unitOfWork.Repository<Field>()
                .FindSingleAsync(f => f.FieldId == fieldId && f.DeletedAt == null);
            if (field != null)
            {
                field.AverageRating = (decimal)averageRating;
                field.LastCalculatedRating = DateTime.UtcNow;
                _unitOfWork.Repository<Field>().Update(field);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}