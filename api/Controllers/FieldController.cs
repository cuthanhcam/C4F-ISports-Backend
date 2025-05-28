using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using api.Dtos.Field;
using api.Interfaces;
using System.Security.Claims;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FieldController : ControllerBase
    {
        private readonly IFieldService _fieldService;
        private readonly ILogger<FieldController> _logger;

        public FieldController(IFieldService fieldService, ILogger<FieldController> logger)
        {
            _fieldService = fieldService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách sân với bộ lọc và phân trang.
        /// </summary>
        /// <param name="filterDto">Bộ lọc để tìm kiếm sân (thành phố, quận, môn thể thao, giá, v.v.).</param>
        /// <returns>Danh sách sân, tổng số, trang hiện tại và kích thước trang.</returns>
        /// <response code="200">Trả về danh sách sân và thông tin phân trang.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="500">Lỗi hệ thống.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFields([FromQuery] FieldFilterDto filterDto)
        {
            try
            {
                _logger.LogInformation("Lấy danh sách sân với bộ lọc: {@Filter}", filterDto);
                var (data, total, page, pageSize) = await _fieldService.GetFilteredFieldsAsync(filterDto);
                _logger.LogInformation("Lấy thành công {Count} sân, trang {Page}/{TotalPages}", 
                    data.Count, page, (int)Math.Ceiling((double)total / pageSize));

                return Ok(new
                {
                    data,
                    total,
                    page,
                    pageSize
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Dữ liệu đầu vào không hợp lệ khi lấy danh sách sân.");
                return BadRequest(new { error = "Dữ liệu không hợp lệ", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi lấy danh sách sân.");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "Lỗi hệ thống", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một sân theo ID.
        /// </summary>
        /// <param name="id">ID của sân.</param>
        /// <param name="include">Danh sách các thông tin bổ sung (services, amenities, images).</param>
        /// <returns>Thông tin chi tiết của sân.</returns>
        /// <response code="200">Trả về thông tin sân.</response>
        /// <response code="404">Sân không tồn tại.</response>
        /// <response code="500">Lỗi hệ thống.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFieldById(int id, [FromQuery] string? include = null)
        {
            try
            {
                _logger.LogInformation("Lấy thông tin sân với ID: {Id}, include: {Include}", id, include);
                var field = await _fieldService.GetFieldByIdAsync(id, include);
                _logger.LogInformation("Lấy thông tin sân {Id} thành công.", id);
                return Ok(field);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Sân với ID {Id} không tồn tại.", id);
                return NotFound(new { error = "Không tìm thấy", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi lấy thông tin sân {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "Lỗi hệ thống", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của địa chỉ sân.
        /// </summary>
        /// <param name="validateAddressDto">Thông tin địa chỉ cần kiểm tra.</param>
        /// <returns>Kết quả kiểm tra địa chỉ (tọa độ, tính hợp lệ).</returns>
        /// <response code="200">Trả về kết quả kiểm tra địa chỉ.</response>
        /// <response code="400">Địa chỉ không hợp lệ.</response>
        /// <response code="500">Lỗi hệ thống.</response>
        [HttpPost("validate-address")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidateAddress([FromBody] ValidateAddressDto validateAddressDto)
        {
            try
            {
                _logger.LogInformation("Kiểm tra địa chỉ: {@Address}", validateAddressDto);
                var result = await _fieldService.ValidateAddressAsync(validateAddressDto);
                _logger.LogInformation("Kiểm tra địa chỉ thành công: {IsValid}", result.IsValid);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Địa chỉ không hợp lệ.");
                return BadRequest(new { error = "Dữ liệu không hợp lệ", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi kiểm tra địa chỉ.");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "Lỗi hệ thống", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        /// <summary>
        /// Tạo một sân mới (yêu cầu quyền Owner).
        /// </summary>
        /// <param name="createFieldDto">Thông tin sân cần tạo.</param>
        /// <returns>Thông tin sân vừa tạo.</returns>
        /// <response code="201">Sân được tạo thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Người dùng không được xác thực.</response>
        /// <response code="403">Người dùng không có quyền Owner.</response>
        /// <response code="500">Lỗi hệ thống.</response>
        [HttpPost]
        [Authorize(Roles = "Owner")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateField([FromBody] CreateFieldDto createFieldDto)
        {
            try
            {
                _logger.LogInformation("Tạo sân mới: {@Field}", createFieldDto);
                var field = await _fieldService.CreateFieldAsync(User, createFieldDto);
                _logger.LogInformation("Tạo sân thành công với ID: {Id}", field.FieldId);
                return CreatedAtAction(nameof(GetFieldById), new { id = field.FieldId }, field);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Không có quyền tạo sân.");
                return StatusCode(StatusCodes.Status403Forbidden, 
                    new { error = "Không được phép", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Dữ liệu đầu vào không hợp lệ khi tạo sân.");
                return BadRequest(new { error = "Dữ liệu không hợp lệ", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi tạo sân.");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "Lỗi hệ thống", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        /// <summary>
        /// Tải ảnh lên cho sân (yêu cầu quyền Owner).
        /// </summary>
        /// <param name="id">ID của sân.</param>
        /// <param name="file">File ảnh cần tải lên.</param>
        /// <param name="isPrimary">Ảnh có phải là ảnh chính không.</param>
        /// <returns>Thông tin ảnh vừa tải lên.</returns>
        /// <response code="200">Ảnh được tải lên thành công.</response>
        /// <response code="400">File ảnh không hợp lệ.</response>
        /// <response code="401">Người dùng không được xác thực.</response>
        /// <response code="403">Người dùng không có quyền Owner hoặc không sở hữu sân.</response>
        /// <response code="404">Sân không tồn tại.</response>
        /// <response code="500">Lỗi hệ thống.</response>
        [HttpPost("{id}/images")]
        [Authorize(Roles = "Owner")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadFieldImage(int id, IFormFile file, [FromQuery] bool isPrimary = false)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Không có file ảnh được cung cấp cho sân ID: {Id}", id);
                    return BadRequest(new 
                    { 
                        error = "Dữ liệu không hợp lệ", 
                        details = new[] { new { field = "file", message = "Vui lòng chọn một file ảnh." } } 
                    });
                }

                _logger.LogInformation("Tải ảnh lên cho sân ID: {Id}, isPrimary: {IsPrimary}", id, isPrimary);
                var imageResponse = await _fieldService.UploadFieldImageAsync(User, id, file, isPrimary);
                _logger.LogInformation("Tải ảnh thành công cho sân ID: {Id}", id);
                return Ok(imageResponse);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Không có quyền tải ảnh cho sân ID: {Id}", id);
                return StatusCode(StatusCodes.Status403Forbidden, 
                    new { error = "Không được phép", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Sân ID {Id} không tồn tại hoặc dữ liệu không hợp lệ.", id);
                return NotFound(new { error = "Không tìm thấy", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi tải ảnh cho sân ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "Lỗi hệ thống", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        /// <summary>
        /// Cập nhật thông tin sân (yêu cầu quyền Owner).
        /// </summary>
        /// <param name="id">ID của sân.</param>
        /// <param name="updateFieldDto">Thông tin sân cần cập nhật.</param>
        /// <returns>Thông tin sân đã cập nhật.</returns>
        /// <response code="200">Sân được cập nhật thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Người dùng không được xác thực.</response>
        /// <response code="403">Người dùng không có quyền Owner hoặc không sở hữu sân.</response>
        /// <response code="404">Sân không tồn tại.</response>
        /// <response code="500">Lỗi hệ thống.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateField(int id, [FromBody] UpdateFieldDto updateFieldDto)
        {
            try
            {
                _logger.LogInformation("Cập nhật sân ID: {Id}, dữ liệu: {@Field}", id, updateFieldDto);
                var field = await _fieldService.UpdateFieldAsync(User, id, updateFieldDto);
                _logger.LogInformation("Cập nhật sân ID: {Id} thành công.", id);
                return Ok(field);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Không có quyền cập nhật sân ID: {Id}", id);
                return StatusCode(StatusCodes.Status403Forbidden, 
                    new { error = "Không được phép", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Sân ID {Id} không tồn tại hoặc dữ liệu không hợp lệ.", id);
                return BadRequest(new { error = "Dữ liệu không hợp lệ", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi cập nhật sân ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "Lỗi hệ thống", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        /// <summary>
        /// Xóa sân (yêu cầu quyền Owner).
        /// </summary>
        /// <param name="id">ID của sân.</param>
        /// <returns>Không có nội dung trả về nếu xóa thành công.</returns>
        /// <response code="204">Sân được xóa thành công.</response>
        /// <response code="401">Người dùng không được xác thực.</response>
        /// <response code="403">Người dùng không có quyền Owner hoặc không sở hữu sân.</response>
        /// <response code="404">Sân không tồn tại.</response>
        /// <response code="400">Sân có đặt lịch đang hoạt động.</response>
        /// <response code="500">Lỗi hệ thống.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteField(int id)
        {
            try
            {
                _logger.LogInformation("Xóa sân ID: {Id}", id);
                await _fieldService.DeleteFieldAsync(User, id);
                _logger.LogInformation("Xóa sân ID: {Id} thành công.", id);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Không có quyền xóa sân ID: {Id}", id);
                return StatusCode(StatusCodes.Status403Forbidden, 
                    new { error = "Không được phép", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Sân ID {Id} không tồn tại hoặc có đặt lịch đang hoạt động.", id);
                return BadRequest(new { error = "Dữ liệu không hợp lệ", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi xóa sân ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "Lỗi hệ thống", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        /// <summary>
        /// Kiểm tra lịch trống của sân.
        /// </summary>
        /// <param name="id">ID của sân.</param>
        /// <param name="filterDto">Bộ lọc để kiểm tra lịch trống (ngày, giờ, sân con).</param>
        /// <returns>Danh sách lịch trống của các sân con.</returns>
        /// <response code="200">Trả về danh sách lịch trống.</response>
        /// <response code="404">Sân không tồn tại.</response>
        /// <response code="500">Lỗi hệ thống.</response>
        [HttpGet("{id}/availability")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFieldAvailability(int id, [FromQuery] AvailabilityFilterDto filterDto)
        {
            try
            {
                _logger.LogInformation("Kiểm tra lịch trống cho sân ID: {Id}, bộ lọc: {@Filter}", id, filterDto);
                var availability = await _fieldService.GetFieldAvailabilityAsync(id, filterDto);
                _logger.LogInformation("Kiểm tra lịch trống cho sân ID: {Id} thành công.", id);
                return Ok(availability);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Sân ID {Id} không tồn tại.", id);
                return NotFound(new { error = "Không tìm thấy", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi kiểm tra lịch trống cho sân ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "Lỗi hệ thống", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        /// <summary>
        /// Lấy danh sách đánh giá của sân.
        /// </summary>
        /// <param name="id">ID của sân.</param>
        /// <param name="filterDto">Bộ lọc để lấy đánh giá (điểm tối thiểu, sắp xếp).</param>
        /// <returns>Danh sách đánh giá, tổng số, trang hiện tại và kích thước trang.</returns>
        /// <response code="200">Trả về danh sách đánh giá.</response>
        /// <response code="404">Sân không tồn tại.</response>
        /// <response code="500">Lỗi hệ thống.</response>
        [HttpGet("{id}/reviews")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFieldReviews(int id, [FromQuery] ReviewFilterDto filterDto)
        {
            try
            {
                _logger.LogInformation("Lấy đánh giá cho sân ID: {Id}, bộ lọc: {@Filter}", id, filterDto);
                var (data, total, page, pageSize) = await _fieldService.GetFieldReviewsAsync(id, filterDto);
                _logger.LogInformation("Lấy thành công {Count} đánh giá cho sân ID: {Id}", data.Count, id);
                return Ok(new
                {
                    data,
                    total,
                    page,
                    pageSize
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Sân ID {Id} không tồn tại.", id);
                return NotFound(new { error = "Không tìm thấy", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi lấy đánh giá cho sân ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "Lỗi hệ thống", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        /// <summary>
        /// Lấy danh sách đặt sân (yêu cầu quyền Owner).
        /// </summary>
        /// <param name="id">ID của sân.</param>
        /// <param name="filterDto">Bộ lọc để lấy danh sách đặt sân (trạng thái, ngày).</param>
        /// <returns>Danh sách đặt sân, tổng số, trang hiện tại và kích thước trang.</returns>
        /// <response code="200">Trả về danh sách đặt sân.</response>
        /// <response code="401">Người dùng không được xác thực.</response>
        /// <response code="403">Người dùng không có quyền Owner hoặc không sở hữu sân.</response>
        /// <response code="404">Sân không tồn tại.</response>
        /// <response code="500">Lỗi hệ thống.</response>
        [HttpGet("{id}/bookings")]
        [Authorize(Roles = "Owner")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFieldBookings(int id, [FromQuery] BookingFilterDto filterDto)
        {
            try
            {
                _logger.LogInformation("Lấy danh sách đặt sân cho sân ID: {Id}, bộ lọc: {@Filter}", id, filterDto);
                var (data, total, page, pageSize) = await _fieldService.GetFieldBookingsAsync(User, id, filterDto);
                _logger.LogInformation("Lấy thành công {Count} đặt sân cho sân ID: {Id}", data.Count, id);
                return Ok(new
                {
                    data,
                    total,
                    page,
                    pageSize
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Không có quyền lấy danh sách đặt sân cho sân ID: {Id}", id);
                return StatusCode(StatusCodes.Status403Forbidden, 
                    new { error = "Không được phép", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Sân ID {Id} không tồn tại.", id);
                return NotFound(new { error = "Không tìm thấy", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi lấy danh sách đặt sân cho sân ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "Lỗi hệ thống", message = "Đã xảy ra lỗi không mong muốn." });
            }
        }
    }
}