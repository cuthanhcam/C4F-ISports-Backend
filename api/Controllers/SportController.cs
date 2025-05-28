using api.Dtos.Sport;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace api.Controllers
{
    /// <summary>
    /// Controller xử lý các yêu cầu quản lý danh mục môn thể thao.
    /// </summary>
    [Route("api/sports")]
    [ApiController]
    public class SportController : ControllerBase
    {
        private readonly ISportService _sportService;
        private readonly ILogger<SportController> _logger;

        public SportController(ISportService sportService, ILogger<SportController> logger)
        {
            _sportService = sportService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả các môn thể thao đang hoạt động.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllSports(
            [FromQuery] string? keyword,
            [FromQuery] string? sort,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    _logger.LogWarning("Tham số phân trang không hợp lệ: page={Page}, pageSize={PageSize}", page, pageSize);
                    return BadRequest(new { error = "Invalid input", details = new[] { new { field = "pagination", message = "Page and pageSize must be positive." } } });
                }

                var (data, total, pageResult, pageSizeResult) = await _sportService.GetAllSportsAsync(keyword, sort, page, pageSize);
                _logger.LogInformation("Lấy danh sách môn thể thao thành công");
                return Ok(new { data, total, page = pageResult, pageSize = pageSizeResult });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi lấy danh sách môn thể thao");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một môn thể thao theo ID.
        /// </summary>
        [HttpGet("{sportId}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSportById(int sportId)
        {
            try
            {
                if (sportId < 1)
                {
                    _logger.LogWarning("SportId không hợp lệ: {SportId}", sportId);
                    return BadRequest(new { error = "Invalid input", details = new[] { new { field = "sportId", message = "SportId must be positive." } } });
                }

                var sport = await _sportService.GetSportByIdAsync(sportId);
                _logger.LogInformation("Lấy thông tin môn thể thao thành công, SportId: {SportId}", sportId);
                return Ok(sport);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Môn thể thao không tồn tại, SportId: {SportId}", sportId);
                return NotFound(new { error = "Resource not found", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi lấy thông tin môn thể thao");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Tạo một môn thể thao mới.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateSport([FromBody] CreateSportDto createSportDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi tạo môn thể thao: {Errors}", ModelState);
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => new
                        {
                            field = e.ErrorMessage.Contains("SportName") ? "sportName" :
                                    e.ErrorMessage.Contains("Description") ? "description" : "unknown",
                            message = e.ErrorMessage
                        });
                    return BadRequest(new { error = "Invalid input", details = errors });
                }

                var sport = await _sportService.CreateSportAsync(User, createSportDto);
                _logger.LogInformation("Tạo môn thể thao thành công, SportId: {SportId}", sport.SportId);
                return StatusCode(StatusCodes.Status201Created, sport);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi tạo môn thể thao");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi tạo môn thể thao");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Cập nhật thông tin môn thể thao.
        /// </summary>
        [HttpPut("{sportId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSport(int sportId, [FromBody] UpdateSportDto updateSportDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi cập nhật môn thể thao: {Errors}", ModelState);
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => new
                    {
                        field = e.ErrorMessage.Contains("SportName") ? "sportName" :
                                e.ErrorMessage.Contains("Description") ? "description" : "unknown",
                        message = e.ErrorMessage
                    });
                    return BadRequest(new { error = "Invalid input", details = errors });
                }

                if (sportId < 1)
                {
                    _logger.LogWarning("SportId không hợp lệ: {SportId}", sportId);
                    return BadRequest(new { error = "Invalid input", details = new[] { new { field = "sportId", message = "SportId must be positive." } } });
                }

                var sport = await _sportService.UpdateSportAsync(User, sportId, updateSportDto);
                _logger.LogInformation("Cập nhật môn thể thao thành công, SportId: {SportId}", sportId);
                return Ok(sport);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi cập nhật môn thể thao");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi cập nhật môn thể thao");
                return NotFound(new { error = "Resource not found", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi cập nhật môn thể thao");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "An unexpected error occurred." } );
            }
        }

        /// <summary>
        /// Xóa mềm một môn thể thao.
        /// </summary>
        [HttpDelete("{sportId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSport(int sportId)
        {
            try
            {
                if (sportId < 1)
                {
                    _logger.LogWarning("SportId không hợp lệ: {SportId}", sportId);
                    return BadRequest(new { error = "Invalid input", details = new[] { new { field = "sportId", message = "SportId must be positive." } } });
                }

                await _sportService.DeleteSportAsync(User, sportId);
                _logger.LogInformation("Xóa môn thể thao thành công, SportId: {SportId}", sportId);
                return Ok(new { message = "Sport deleted successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Lỗi xác thực khi xóa môn thể thao");
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi tham số khi xóa môn thể thao");
                return NotFound(new { error = "Resource not found", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi xóa môn thể thao");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Tải lên hoặc cập nhật icon cho môn thể thao.
        /// </summary>
        [HttpPost("{sportId}/icon")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadSportIcon(int sportId, IFormFile file)
        {
            try
            {
                if (sportId < 1)
                {
                    _logger.LogWarning("SportId không hợp lệ: {SportId}", sportId);
                    return BadRequest(new { error = "Invalid input", details = new[] { new { field = "sportId", message = "SportId must be positive." } } });
                }

                if (file == null)
                {
                    _logger.LogWarning("File ảnh không hợp lệ để tải lên icon cho SportId: {SportId}", sportId);
                    return BadRequest(new { error = "Invalid input", details = new[] { new { field = "image", message = "Image file is required." } } });
                }

                var imageUrl = await _sportService.UploadSportIconAsync(User, sportId, file);
                _logger.LogInformation("Upload icon thành công for SportId: {SportId}", sportId);
                return Ok(new { sportId = imageUrl, message = "Sport icon uploaded successfully" });
            }
            catch (ArgumentException ex)
                {
                _logger.LogError(ex, "Lỗi tham số khi tải lên icon cho môn thể thao: {SportId}", sportId);
                return NotFound(new { error = "Resource not found", message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
                {
                _logger.LogError(ex, "Lỗi xác thực khi tải lên icon cho môn thể thao: {SportId}", sportId);
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Lỗi hệ thống khi tải lên icon cho môn thể thao: {SportId}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal Server Error", message = "An unexpected error occurred." });
            }
        }
    }
}