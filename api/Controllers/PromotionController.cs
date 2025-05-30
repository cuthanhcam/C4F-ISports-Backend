using api.Dtos.Promotion;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace api.Controllers
{
    /// <summary>
    /// Controller xử lý các yêu cầu quản lý khuyến mãi.
    /// </summary>
    [Route("api/promotions")]
    [ApiController]
    public class PromotionController : ControllerBase
    {
        private readonly IPromotionService _promotionService;
        private readonly ILogger<PromotionController> _logger;

        public PromotionController(IPromotionService promotionService, ILogger<PromotionController> logger)
        {
            _promotionService = promotionService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách khuyến mãi đang hoạt động.
        /// </summary>
        /// <param name="fieldId">ID sân (tùy chọn).</param>
        /// <param name="code">Mã khuyến mãi (tùy chọn).</param>
        /// <param name="page">Số trang (mặc định: 1).</param>
        /// <param name="pageSize">Số mục mỗi trang (mặc định: 10).</param>
        /// <returns>Danh sách khuyến mãi.</returns>
        /// <response code="200">Trả về danh sách khuyến mãi.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPromotions(
            [FromQuery] int? fieldId,
            [FromQuery] string? code,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    _logger.LogWarning("Tham số phân trang không hợp lệ: page={Page}, pageSize={PageSize}", page, pageSize);
                    return BadRequest(new { error = "Dữ liệu không hợp lệ", details = new[] { new { field = "pagination", message = "Page và pageSize phải là số dương." } } });
                }

                if (fieldId.HasValue && fieldId < 1)
                {
                    _logger.LogWarning("FieldId không hợp lệ: {FieldId}", fieldId);
                    return BadRequest(new { error = "Dữ liệu không hợp lệ", details = new[] { new { field = "fieldId", message = "FieldId phải là số dương." } } });
                }

                var (data, total, pageResult, pageSizeResult) = await _promotionService.GetPromotionsAsync(fieldId, code, page, pageSize);
                _logger.LogInformation("Lấy danh sách khuyến mãi thành công");
                return Ok(new { data, total, page = pageResult, pageSize = pageSizeResult, message = "Lấy danh sách khuyến mãi thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi lấy danh sách khuyến mãi");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Lỗi hệ thống", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        /// <summary>
        /// Lấy chi tiết một khuyến mãi theo ID.
        /// </summary>
        /// <param name="promotionId">ID khuyến mãi.</param>
        /// <returns>Thông tin chi tiết khuyến mãi.</returns>
        /// <response code="200">Trả về thông tin khuyến mãi.</response>
        /// <response code="404">Không tìm thấy khuyến mãi.</response>
        [HttpGet("{promotionId}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPromotionById(int promotionId)
        {
            try
            {
                if (promotionId < 1)
                {
                    _logger.LogWarning("PromotionId không hợp lệ: {PromotionId}", promotionId);
                    return BadRequest(new { error = "Dữ liệu không hợp lệ", details = new[] { new { field = "promotionId", message = "PromotionId phải là số dương." } } });
                }

                var promotion = await _promotionService.GetPromotionByIdAsync(promotionId);
                _logger.LogInformation("Lấy chi tiết khuyến mãi thành công với ID: {PromotionId}", promotionId);
                return Ok(promotion);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi lấy chi tiết khuyến mãi");
                return NotFound(new { error = "Không tìm thấy", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi lấy chi tiết khuyến mãi");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Lỗi hệ thống", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        /// <summary>
        /// Tạo khuyến mãi mới (chỉ dành cho Owner).
        /// </summary>
        /// <param name="createPromotionDto">Thông tin khuyến mãi cần tạo.</param>
        /// <returns>Thông tin khuyến mãi đã tạo.</returns>
        /// <response code="201">Tạo khuyến mãi thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Không có quyền tạo khuyến mãi.</response>
        [HttpPost]
        [Authorize(Roles = "Owner")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionDto createPromotionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi tạo khuyến mãi: {Errors}", ModelState);
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => new { field = e.ErrorMessage.Contains("FieldId") ? "fieldId" : e.ErrorMessage.Contains("Code") ? "code" : "unknown", message = e.ErrorMessage });
                    return BadRequest(new { error = "Dữ liệu không hợp lệ", details = errors });
                }

                var promotion = await _promotionService.CreatePromotionAsync(User, createPromotionDto);
                _logger.LogInformation("Tạo khuyến mãi thành công với ID: {PromotionId}", promotion.PromotionId);
                return CreatedAtAction(nameof(GetPromotionById), new { promotionId = promotion.PromotionId }, promotion);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi tạo khuyến mãi");
                return Unauthorized(new { error = "Không có quyền", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi tạo khuyến mãi");
                return BadRequest(new { error = "Dữ liệu không hợp lệ", details = new[] { new { field = "input", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi tạo khuyến mãi");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Lỗi hệ thống", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        /// <summary>
        /// Cập nhật khuyến mãi (chỉ dành cho Owner).
        /// </summary>
        /// <param name="promotionId">ID khuyến mãi.</param>
        /// <param name="updatePromotionDto">Thông tin cần cập nhật.</param>
        /// <returns>Thông tin khuyến mãi đã cập nhật.</returns>
        /// <response code="200">Cập nhật khuyến mãi thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Không có quyền cập nhật.</response>
        /// <response code="404">Không tìm thấy khuyến mãi.</response>
        [HttpPut("{promotionId}")]
        [Authorize(Roles = "Owner")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePromotion(int promotionId, [FromBody] UpdatePromotionDto updatePromotionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi cập nhật khuyến mãi: {Errors}", ModelState);
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => new { field = e.ErrorMessage.Contains("Code") ? "code" : "unknown", message = e.ErrorMessage });
                    return BadRequest(new { error = "Dữ liệu không hợp lệ", details = errors });
                }

                if (promotionId < 1)
                {
                    _logger.LogWarning("PromotionId không hợp lệ: {PromotionId}", promotionId);
                    return BadRequest(new { error = "Dữ liệu không hợp lệ", details = new[] { new { field = "id", message = "PromotionId phải là số dương." } } });
                }

                var promotion = await _promotionService.UpdatePromotionAsync(User, promotionId, updatePromotionDto);
                _logger.LogInformation("Cập nhật khuyến mãi thành công với ID: {PromotionId}", promotionId);
                return Ok(promotion);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi cập nhật khuyến mãi");
                return Unauthorized(new { error = "Không có quyền", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi cập nhật khuyến mãi");
                return NotFound(new { error = "Không tìm thấy", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi cập nhật khuyến mãi");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Lỗi hệ thống", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        /// <summary>
        /// Xóa khuyến mãi (chỉ dành cho Owner).
        /// </summary>
        /// <param name="id">ID khuyến mãi.</param>
        /// <returns>Thông báo xóa thành công.</returns>
        /// <response code="200">Xóa khuyến mãi thành công.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Không có quyền xóa.</response>
        /// <response code="404">Không tìm thấy khuyến mãi.</response>
        [HttpDelete("{promotionId}")]
        [Authorize(Roles = "Owner")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePromotion(int promotionId)
        {
            try
            {
                if (promotionId < 1)
                {
                    _logger.LogWarning("PromotionId không hợp lệ: {PromotionId}", promotionId);
                    return BadRequest(new { error = "Dữ liệu không hợp lệ", details = new[] { new { field = "id", message = "PromotionId phải là số dương." } } });
                }

                await _promotionService.DeletePromotionAsync(User, promotionId);
                _logger.LogInformation("Xóa khuyến mãi thành công với ID: {PromotionId}", promotionId);
                return Ok(new { message = "Khuyến mãi được xóa thành công." });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi xóa khuyến mãi");
                return Unauthorized(new { error = "Không có quyền", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi xóa khuyến mãi");
                return NotFound(new { error = "Không tìm thấy", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi xóa khuyến mãi");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Lỗi hệ thống", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        /// <summary>
        /// Áp dụng mã khuyến mãi cho đơn đặt sân.
        /// </summary>
        /// <param name="applyPromotionDto">Thông tin mã khuyến mãi và đơn đặt sân.</param>
        /// <returns>Kết quả sau khi áp dụng khuyến mãi.</returns>
        /// <response code="200">Áp dụng khuyến mãi thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="404">Không tìm thấy đơn đặt sân hoặc mã khuyến mãi.</response>
        [HttpPost("apply")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApplyPromotion([FromBody] ApplyPromotionDto applyPromotionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi áp dụng khuyến mãi: {Errors}", ModelState);
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => new { field = e.ErrorMessage.Contains("BookingId") ? "bookingId" : e.ErrorMessage.Contains("Code") ? "code" : "unknown", message = e.ErrorMessage });
                    return BadRequest(new { error = "Dữ liệu không hợp lệ", details = errors });
                }

                var response = await _promotionService.ApplyPromotionAsync(User, applyPromotionDto);
                _logger.LogInformation("Áp dụng mã khuyến mãi thành công: {Code} cho BookingId: {BookingId}", applyPromotionDto.Code, applyPromotionDto.BookingId);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi áp dụng mã khuyến mãi");
                return Unauthorized(new { error = "Không có quyền", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi áp dụng mã khuyến mãi");
                return NotFound(new { error = "Không tìm thấy", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi áp dụng mã khuyến mãi");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Lỗi hệ thống", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }
    }
}
