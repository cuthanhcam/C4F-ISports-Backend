using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using api.Dtos.Field;
using api.Interfaces;
using Microsoft.AspNetCore.Http;

namespace api.Controllers
{
    /// <summary>
    /// Controller xử lý các yêu cầu quản lý sân thể thao.
    /// </summary>
    [Route("api/fields")]
    [ApiController]
    public class FieldController : ControllerBase
    {
        private readonly IFieldService _fieldService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FieldController> _logger;

        public FieldController(
            IFieldService fieldService,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<FieldController> logger)
        {
            _fieldService = fieldService;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách sân thể thao với bộ lọc và phân trang.
        /// </summary>
        /// <param name="filterDto">Bộ lọc tìm kiếm sân (vị trí, giá, loại sân, v.v.).</param>
        /// <returns>Danh sách sân thể thao phù hợp.</returns>
        /// <response code="200">Trả về danh sách sân với phân trang.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetFields([FromQuery] FieldFilterDto filterDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi lấy danh sách sân: {Errors}", ModelState);
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => new { field = e.ErrorMessage.Contains("Page") ? "page" : e.ErrorMessage.Contains("Latitude") ? "latitude" : "unknown", message = e.ErrorMessage });
                    return BadRequest(new { error = "Invalid input", details = errors });
                }

                _logger.LogInformation("Lấy danh sách sân với bộ lọc: {@Filter}", filterDto);
                var result = await _fieldService.GetFieldsAsync(filterDto);
                _logger.LogInformation("Lấy thành công {Count} sân, trang {Page}/{TotalPages}",
                    result.Data.Count, result.Page, (int)Math.Ceiling((double)result.Total / result.PageSize));

                return Ok(new
                {
                    data = result.Data,
                    total = result.Total,
                    page = result.Page,
                    pageSize = result.PageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi lấy danh sách sân.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Internal server error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của sân theo ID.
        /// </summary>
        /// <param name="id">ID của sân.</param>
        /// <param name="include">Danh sách dữ liệu liên quan cần bao gồm (subfields, services, amenities, images).</param>
        /// <returns>Thông tin chi tiết của sân.</returns>
        /// <response code="200">Trả về thông tin sân.</response>
        /// <response code="404">Sân không tồn tại.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFieldById(int id, [FromQuery] string? include = null)
        {
            try
            {
                _logger.LogInformation("Lấy thông tin sân với ID: {Id}, include: {Include}", id, include);
                var field = await _fieldService.GetFieldByIdAsync(id, include ?? string.Empty);
                _logger.LogInformation("Lấy thông tin sân {Id} thành công.", id);
                return Ok(field);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Sân với ID {Id} không tồn tại.", id);
                return NotFound(new { error = "Resource not found", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi lấy thông tin sân {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Internal server error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Lấy danh sách sân thuộc quyền quản lý của owner đang đăng nhập.
        /// </summary>
        /// <param name="filterDto">Bộ lọc tìm kiếm sân (tên, trạng thái, v.v.).</param>
        /// <returns>Danh sách sân thuộc owner.</returns>
        /// <response code="200">Trả về danh sách sân với phân trang.</response>
        /// <response code="401">Không có quyền truy cập.</response>
        /// <response code="403">Không phải là owner.</response>
        /// <response code="500">Lỗi server.</response>
        [HttpGet("my-fields")]
        [Authorize(Roles = "Owner")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOwnerFields([FromQuery] OwnerFieldFilterDto filterDto)
        {
            try
            {
                var result = await _fieldService.GetOwnerFieldsAsync(filterDto, User);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Không có quyền truy cập: {Message}", ex.Message);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách sân của owner: {Message}", ex.Message);
                return StatusCode(500, new { error = "Lỗi khi lấy danh sách sân: " + ex.Message });
            }
        }

        /// <summary>
        /// Kiểm tra và xác thực địa chỉ của sân.
        /// </summary>
        /// <param name="validateAddressDto">Thông tin địa chỉ cần kiểm tra.</param>
        /// <returns>Kết quả xác thực địa chỉ (tọa độ, địa chỉ định dạng).</returns>
        /// <response code="200">Xác thực địa chỉ thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="429">Vượt quá giới hạn yêu cầu geocoding.</response>
        [HttpPost("validate-address")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> ValidateAddress([FromBody] ValidateAddressDto validateAddressDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi kiểm tra địa chỉ: {Errors}", ModelState);
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => new { field = e.ErrorMessage.Contains("Address") ? "address" : "unknown", message = e.ErrorMessage });
                    return BadRequest(new { error = "Invalid input", details = errors });
                }

                _logger.LogInformation("Kiểm tra địa chỉ: {@Address}", validateAddressDto);
                var result = await _fieldService.ValidateAddressAsync(validateAddressDto);
                _logger.LogInformation("Kiểm tra địa chỉ thành công: {IsValid}", result.IsValid);
                return Ok(result);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Rate limit exceeded"))
            {
                _logger.LogWarning(ex, "Vượt quá giới hạn yêu cầu geocoding.");
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    new { error = "Rate limit exceeded", message = "Geocoding service rate limit exceeded. Please try again later." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi kiểm tra địa chỉ.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Internal server error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Tạo sân thể thao mới (yêu cầu quyền Owner).
        /// </summary>
        /// <param name="createFieldDto">Thông tin sân cần tạo.</param>
        /// <returns>Thông tin sân đã tạo.</returns>
        /// <response code="201">Tạo sân thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Không có quyền tạo sân.</response>
        /// <response code="429">Vượt quá giới hạn yêu cầu geocoding.</response>
        [HttpPost]
        [Authorize(Roles = "Owner")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> CreateField([FromBody] CreateFieldDto createFieldDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi tạo sân: {Errors}", ModelState);
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => new { field = e.ErrorMessage.Contains("FieldName") ? "fieldName" : "unknown", message = e.ErrorMessage });
                    return BadRequest(new { error = "Invalid input", details = errors });
                }

                _logger.LogInformation("Tạo sân mới: {@Field}", createFieldDto);
                var field = await _fieldService.CreateFieldAsync(createFieldDto, User);
                _logger.LogInformation("Tạo sân thành công với ID: {Id}", field.FieldId);
                return CreatedAtAction(nameof(GetFieldById), new { id = field.FieldId }, field);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Không có quyền tạo sân.");
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { error = "Forbidden", message = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Rate limit exceeded"))
            {
                _logger.LogWarning(ex, "Vượt quá giới hạn yêu cầu geocoding.");
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    new { error = "Rate limit exceeded", message = "Geocoding service rate limit exceeded. Please try again later." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Dữ liệu đầu vào không hợp lệ khi tạo sân.");
                return BadRequest(new { error = "Invalid input", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi tạo sân.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Internal server error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Tải ảnh lên cho sân thể thao (yêu cầu quyền Owner).
        /// </summary>
        /// <param name="id">ID của sân.</param>
        /// <param name="uploadFieldImageDto">Thông tin ảnh cần tải lên.</param>
        /// <returns>Thông tin ảnh đã tải lên.</returns>
        /// <response code="201">Tải ảnh thành công.</response>
        /// <response code="400">File không hợp lệ hoặc sân không tồn tại.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Không có quyền tải ảnh.</response>
        /// <response code="404">Sân không tồn tại.</response>
        /// <response code="413">Kích thước file vượt quá giới hạn.</response>
        [HttpPost("{id}/images")]
        [Authorize(Roles = "Owner")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
        public async Task<IActionResult> UploadFieldImage(int id, [FromForm] UploadFieldImageDto uploadFieldImageDto)
        {
            try
            {
                if (!ModelState.IsValid || uploadFieldImageDto.Image == null)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi tải ảnh cho sân ID: {Id}", id);
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => new { field = e.ErrorMessage.Contains("Image") ? "image" : "unknown", message = e.ErrorMessage })
                        .ToList();
                    if (uploadFieldImageDto.Image == null)
                    {
                        errors.Add(new { field = "image", message = "Image file is required" });
                    }
                    return BadRequest(new { error = "Invalid input", details = errors });
                }

                var maxFileSize = _configuration.GetValue<long>("Cloudinary:MaxFileSize", 5 * 1024 * 1024); // Mặc định 5MB
                if (uploadFieldImageDto.Image.Length > maxFileSize)
                {
                    _logger.LogWarning("Kích thước file vượt quá giới hạn cho sân ID: {Id}", id);
                    return StatusCode(StatusCodes.Status413PayloadTooLarge,
                        new { error = "Payload too large", message = "Image size exceeds maximum limit" });
                }

                _logger.LogInformation("Tải ảnh lên cho sân ID: {Id}, isPrimary: {IsPrimary}", id, uploadFieldImageDto.IsPrimary);
                var imageResponse = await _fieldService.UploadFieldImageAsync(id, uploadFieldImageDto, User);
                _logger.LogInformation("Tải ảnh thành công cho sân ID: {Id}", id);
                return StatusCode(StatusCodes.Status201Created, imageResponse);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Không có quyền tải ảnh cho sân ID: {Id}", id);
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { error = "Forbidden", message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Sân ID {Id} không tồn tại.", id);
                return NotFound(new { error = "Not found", message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Dữ liệu không hợp lệ khi tải ảnh cho sân ID: {Id}", id);
                return BadRequest(new { error = "Invalid input", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi tải ảnh cho sân ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Internal server error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Cập nhật thông tin sân thể thao (yêu cầu quyền Owner).
        /// </summary>
        /// <param name="id">ID của sân.</param>
        /// <param name="updateFieldDto">Thông tin sân cần cập nhật.</param>
        /// <returns>Thông tin sân đã cập nhật.</returns>
        /// <response code="200">Cập nhật sân thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Không có quyền cập nhật sân.</response>
        /// <response code="404">Sân không tồn tại.</response>
        /// <response code="429">Vượt quá giới hạn yêu cầu geocoding.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> UpdateField(int id, [FromBody] UpdateFieldDto updateFieldDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi cập nhật sân ID: {Id}. Errors: {Errors}", id, ModelState);
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => new { field = e.ErrorMessage.Contains("FieldName") ? "fieldName" : "unknown", message = e.ErrorMessage });
                    return BadRequest(new { error = "Invalid input", details = errors });
                }

                _logger.LogInformation("Cập nhật sân ID: {Id}, dữ liệu: {@Field}", id, updateFieldDto);
                var field = await _fieldService.UpdateFieldAsync(id, updateFieldDto, User);
                _logger.LogInformation("Cập nhật sân ID: {Id} thành công.", id);
                return Ok(field);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Không có quyền cập nhật sân ID: {Id}.", id);
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { error = "Forbidden", message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Sân ID {Id} không tồn tại.", id);
                return NotFound(new { error = "Resource not found", message = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Rate limit exceeded"))
            {
                _logger.LogWarning(ex, "Vượt quá giới hạn yêu cầu geocoding khi cập nhật sân ID: {Id}.", id);
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    new { error = "Rate limit exceeded", message = "Geocoding service rate limit exceeded. Please try again later." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Dữ liệu không hợp lệ khi cập nhật sân ID: {Id}.", id);
                return BadRequest(new { error = "Invalid input", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi cập nhật sân ID: {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Internal server error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Xóa mềm sân thể thao (yêu cầu quyền Owner).
        /// </summary>
        /// <param name="id">ID của sân.</param>
        /// <returns>Thông tin sân đã xóa.</returns>
        /// <response code="200">Xóa sân thành công.</response>
        /// <response code="400">Sân có đặt lịch đang hoạt động.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Không có quyền xóa sân.</response>
        /// <response code="404">Sân không tồn tại.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteField(int id)
        {
            try
            {
                _logger.LogInformation("Xóa sân ID: {Id}", id);
                var result = await _fieldService.DeleteFieldAsync(id, User);
                _logger.LogInformation("Xóa sân ID: {Id} thành công.", id);
                return Ok(new { message = "Field deleted successfully", data = result });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Không có quyền xóa sân ID: {Id}.", id);
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { error = "Forbidden", message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Sân ID {Id} không tồn tại.", id);
                return NotFound(new { error = "Resource not found", message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Không thể xóa sân ID {Id} do có đặt lịch đang hoạt động.", id);
                return BadRequest(new { error = "Invalid operation", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi xóa sân ID: {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Internal server error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Kiểm tra lịch trống của sân thể thao.
        /// </summary>
        /// <param name="id">ID của sân.</param>
        /// <param name="filterDto">Bộ lọc lịch trống (ngày, loại sân, v.v.).</param>
        /// <returns>Danh sách các slot thời gian trống.</returns>
        /// <response code="200">Trả về lịch trống của sân.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="404">Sân không tồn tại.</response>
        [HttpGet("{id}/availability")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFieldAvailability(int id, [FromQuery] AvailabilityFilterDto filterDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi kiểm tra lịch trống: {Errors}", ModelState);
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => new { field = e.ErrorMessage.Contains("Date") ? "date" : e.ErrorMessage.Contains("StartTime") ? "startTime" : "unknown", message = e.ErrorMessage });
                    return BadRequest(new { error = "Invalid input", details = errors });
                }

                _logger.LogInformation("Kiểm tra lịch trống cho sân ID: {Id}, bộ lọc: {@Filter}", id, filterDto);
                var availability = await _fieldService.GetFieldAvailabilityAsync(id, filterDto);
                _logger.LogInformation("Kiểm tra lịch trống cho sân ID: {Id} thành công.", id);
                return Ok(new { data = availability });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Sân ID {Id} không tồn tại.", id);
                return NotFound(new { error = "Resource not found", message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Dữ liệu không hợp lệ khi kiểm tra lịch trống cho sân ID: {Id}", id);
                return BadRequest(new { error = "Invalid input", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi kiểm tra lịch trống cho sân ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Internal server error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Lấy danh sách đánh giá của sân thể thao.
        /// </summary>
        /// <param name="id">ID của sân.</param>
        /// <param name="filterDto">Bộ lọc đánh giá (điểm số tối thiểu, sắp xếp).</param>
        /// <returns>Danh sách đánh giá với phân trang.</returns>
        /// <response code="200">Trả về danh sách đánh giá.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="404">Sân không tồn tại.</response>
        [HttpGet("{id}/reviews")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFieldReviews(int id, [FromQuery] ReviewFilterDto filterDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi lấy đánh giá: {Errors}", ModelState);
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => new { field = e.ErrorMessage.Contains("MinRating") ? "minRating" : "unknown", message = e.ErrorMessage });
                    return BadRequest(new { error = "Invalid input", details = errors });
                }

                _logger.LogInformation("Lấy đánh giá cho sân ID: {Id}, bộ lọc: {@Filter}", id, filterDto);
                var result = await _fieldService.GetFieldReviewsAsync(id, filterDto);
                _logger.LogInformation("Lấy thành công {Count} đánh giá cho sân ID: {Id}", result.Data.Count, id);
                return Ok(new
                {
                    data = result.Data,
                    total = result.Total,
                    page = result.Page,
                    pageSize = result.PageSize
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Sân ID {Id} không tồn tại.", id);
                return NotFound(new { error = "Resource not found", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi lấy đánh giá cho sân ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Internal server error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Lấy danh sách đặt sân của sân thể thao (yêu cầu quyền Owner).
        /// </summary>
        /// <param name="id">ID của sân.</param>
        /// <param name="filterDto">Bộ lọc đặt sân (trạng thái, ngày, v.v.).</param>
        /// <returns>Danh sách đặt sân với phân trang.</returns>
        /// <response code="200">Trả về danh sách đặt sân.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Không có quyền truy cập.</response>
        /// <response code="404">Sân không tồn tại.</response>
        [HttpGet("{id}/bookings")]
        [Authorize(Roles = "Owner")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFieldBookings(int id, [FromQuery] BookingFilterDto filterDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi lấy danh sách đặt sân: {Errors}", ModelState);
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => new { field = e.ErrorMessage.Contains("Status") ? "status" : "unknown", message = e.ErrorMessage });
                    return BadRequest(new { error = "Invalid input", details = errors });
                }

                _logger.LogInformation("Lấy danh sách đặt sân cho sân ID: {Id}, bộ lọc: {@Filter}", id, filterDto);
                var result = await _fieldService.GetFieldBookingsAsync(id, filterDto, User);
                _logger.LogInformation("Lấy thành công {Count} đặt sân cho sân ID: {Id}", result.Data.Count, id);
                return Ok(new
                {
                    data = result.Data,
                    total = result.Total,
                    page = result.Page,
                    pageSize = result.PageSize
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Không có quyền lấy danh sách đặt sân cho sân ID: {Id}", id);
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { error = "Forbidden", message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Sân ID {Id} không tồn tại.", id);
                return NotFound(new { error = "Resource not found", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi lấy danh sách đặt sân cho sân ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Internal server error", message = "An unexpected error occurred." });
            }
        }
    }
}