using api.Dtos.Review;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace api.Controllers
{
    [Route("api/reviews")]
    [ApiController]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(IReviewService reviewService, ILogger<ReviewController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }
        
        [HttpPost]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto createReviewDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi tạo đánh giá: {Errors}", ModelState);
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => new { field = e.ErrorMessage.Contains("FieldId") ? "fieldId" : e.ErrorMessage.Contains("BookingId") ? "bookingId" : e.ErrorMessage.Contains("Rating") ? "rating" : "comment", message = e.ErrorMessage });
                    return BadRequest(new { error = "Invalid input", details = errors });
                }

                var review = await _reviewService.CreateReviewAsync(User, createReviewDto);
                _logger.LogInformation("Tạo đánh giá thành công, ReviewId: {ReviewId}", review.ReviewId);
                return StatusCode(StatusCodes.Status201Created, review);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi tạo đánh giá");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi tạo đánh giá");
                return ex.Message.Contains("Booking không tồn tại") || ex.Message.Contains("Sân không tồn tại")
                    ? NotFound(new { error = "Resource not found", message = ex.Message })
                    : BadRequest(new { error = "Invalid input", details = new[] { new { field = "review", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi tạo đánh giá");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "An unexpected error occurred." });
            }
        }

        [HttpPut("{reviewId}")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] UpdateReviewDto updateReviewDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi cập nhật đánh giá: {Errors}", ModelState);
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => new { field = e.ErrorMessage.Contains("Rating") ? "rating" : "comment", message = e.ErrorMessage });
                    return BadRequest(new { error = "Invalid input", details = errors });
                }

                var review = await _reviewService.UpdateReviewAsync(User, reviewId, updateReviewDto);
                _logger.LogInformation("Cập nhật đánh giá thành công, ReviewId: {ReviewId}", review.ReviewId);
                return Ok(review);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi cập nhật đánh giá");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi cập nhật đánh giá");
                return BadRequest(new { error = "Invalid input", details = new[] { new { field = "review", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi cập nhật đánh giá");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "An unexpected error occurred." });
            }
        }

        [HttpDelete("{reviewId}")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            try
            {
                await _reviewService.DeleteReviewAsync(User, reviewId);
                _logger.LogInformation("Xóa đánh giá thành công, ReviewId: {ReviewId}", reviewId);
                return Ok(new { message = "Review deleted successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi xóa đánh giá");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi xóa đánh giá");
                return BadRequest(new { error = "Invalid input", details = new[] { new { field = "review", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi xóa đánh giá");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "An unexpected error occurred." });
            }
        }
    }
}


