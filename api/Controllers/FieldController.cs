using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos.Field;
using api.Dtos.Field.SubFieldDtos;
using api.Dtos.Field.FieldImageDtos;
using api.Dtos.Field.FieldServiceDtos;
using api.Dtos.Field.FieldAmenityDtos;
using api.Dtos.Field.FieldDescriptionDtos;
using api.Dtos.Field.FieldPricingDtos;
using api.Dtos.Field.FieldAvailabilityDtos;
using api.Dtos.Field.AddressValidationDtos;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace api.Controllers
{
    /// <summary>
    /// Controller xử lý các yêu cầu liên quan đến sân thể thao (fields).
    /// </summary>
    [Route("api/fields")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fields")]
    public class FieldController : ControllerBase
    {
        private readonly IFieldService _fieldService;
        private readonly ISubFieldService _subFieldService;
        private readonly ILogger<FieldController> _logger;

        public FieldController(IFieldService fieldService, ISubFieldService subFieldService, ILogger<FieldController> logger)
        {
            _fieldService = fieldService ?? throw new ArgumentNullException(nameof(fieldService));
            _subFieldService = subFieldService ?? throw new ArgumentNullException(nameof(subFieldService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Tạo một sân thể thao mới.
        /// </summary>
        /// <param name="createFieldDto">Thông tin sân cần tạo.</param>
        /// <returns>Thông tin sân vừa tạo.</returns>
        /// <response code="201">Sân được tạo thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc không có quyền Owner.</response>
        [HttpPost]
        [Authorize(Policy = "OwnerOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateField([FromBody] CreateFieldDto createFieldDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for CreateField: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                var field = await _fieldService.CreateFieldAsync(User, createFieldDto);
                _logger.LogInformation("Field created successfully: FieldId={FieldId}", field.FieldId);
                return CreatedAtAction(nameof(GetFieldById), new { fieldId = field.FieldId }, new { Message = "Tạo sân thành công", Data = field });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error during CreateField");
                return BadRequest(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access during CreateField");
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during CreateField");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Đã xảy ra lỗi hệ thống khi tạo sân." });
            }
        }

        /// <summary>
        /// Cập nhật thông tin sân thể thao.
        /// </summary>
        /// <param name="fieldId">ID của sân cần cập nhật.</param>
        /// <param name="updateFieldDto">Thông tin cập nhật.</param>
        /// <returns>Thông tin sân đã cập nhật.</returns>
        /// <response code="200">Cập nhật sân thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc không có quyền Owner.</response>
        /// <response code="404">Sân không tồn tại.</response>
        [HttpPut("{fieldId}")]
        [Authorize(Policy = "OwnerOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateField(int fieldId, [FromBody] UpdateFieldDto updateFieldDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for UpdateField: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                var field = await _fieldService.UpdateFieldAsync(User, fieldId, updateFieldDto);
                _logger.LogInformation("Field updated successfully: FieldId={FieldId}", fieldId);
                return Ok(new { Message = "Cập nhật sân thành công", Data = field });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error during UpdateField: FieldId={FieldId}", fieldId);
                return BadRequest(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Field not found: FieldId={FieldId}", fieldId);
                return NotFound(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access during UpdateField: FieldId={FieldId}", fieldId);
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during UpdateField: FieldId={FieldId}", fieldId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Đã xảy ra lỗi hệ thống khi cập nhật sân." });
            }
        }

        /// <summary>
        /// Xóa một sân thể thao.
        /// </summary>
        /// <param name="fieldId">ID của sân cần xóa.</param>
        /// <returns>Thông báo xóa thành công.</returns>
        /// <response code="200">Xóa sân thành công.</response>
        /// <response code="401">Chưa đăng nhập hoặc không có quyền Owner.</response>
        /// <response code="404">Sân không tồn tại.</response>
        [HttpDelete("{fieldId}")]
        [Authorize(Policy = "OwnerOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteField(int fieldId)
        {
            try
            {
                await _fieldService.DeleteFieldAsync(User, fieldId);
                _logger.LogInformation("Field deleted successfully: FieldId={FieldId}", fieldId);
                return Ok(new { Message = "Xóa sân thành công" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Field not found: FieldId={FieldId}", fieldId);
                return NotFound(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access during DeleteField: FieldId={FieldId}", fieldId);
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during DeleteField: FieldId={FieldId}", fieldId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Đã xảy ra lỗi hệ thống khi xóa sân." });
            }
        }

        /// <summary>
        /// Lấy thông tin sân theo ID.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <returns>Thông tin sân.</returns>
        /// <response code="200">Trả về thông tin sân.</response>
        /// <response code="404">Sân không tồn tại.</response>
        [HttpGet("{fieldId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFieldById(int fieldId)
        {
            try
            {
                var field = await _fieldService.GetFieldByIdAsync(fieldId);
                _logger.LogInformation("Field retrieved successfully: FieldId={FieldId}", fieldId);
                return Ok(new { Data = field });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Field not found: FieldId={FieldId}", fieldId);
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during GetFieldById: FieldId={FieldId}", fieldId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Đã xảy ra lỗi hệ thống khi lấy thông tin sân." });
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả sân thể thao.
        /// </summary>
        /// <returns>Danh sách sân.</returns>
        /// <response code="200">Trả về danh sách sân.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllFields()
        {
            try
            {
                var fields = await _fieldService.GetAllFieldsAsync();
                _logger.LogInformation("All fields retrieved successfully");
                return Ok(new { Data = fields });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during GetAllFields");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Đã xảy ra lỗi hệ thống khi lấy danh sách sân." });
            }
        }

        /*
        /// <summary>
        /// Thêm ảnh cho sân thể thao.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="image">File ảnh cần tải lên.</param>
        /// <returns>Thông tin ảnh vừa thêm.</returns>
        /// <response code="201">Thêm ảnh thành công.</response>
        /// <response code="400">File ảnh không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc không có quyền Owner.</response>
        /// <response code="404">Sân không tồn tại.</response>
        [HttpPost("{fieldId}/images")]
        [Authorize(Policy = "OwnerOnly")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(FieldImageDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddFieldImage(int fieldId, [FromForm] IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    _logger.LogWarning("Invalid image file for AddFieldImage: FieldId={FieldId}", fieldId);
                    return BadRequest(new { Error = "File ảnh không hợp lệ" });
                }

                var fieldImage = await _fieldService.AddFieldImageAsync(User, fieldId, image);
                _logger.LogInformation("Field image added successfully: FieldId={FieldId}, FieldImageId={FieldImageId}, PublicId={PublicId}, ImageUrl={ImageUrl}", 
                    fieldId, fieldImage.FieldImageId, fieldImage.PublicId, fieldImage.ImageUrl);
                return CreatedAtAction(nameof(GetFieldById), new { fieldId }, new { Message = "Thêm ảnh thành công", Data = fieldImage });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error during AddFieldImage: FieldId={FieldId}", fieldId);
                return BadRequest(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Field not found: FieldId={FieldId}", fieldId);
                return NotFound(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access during AddFieldImage: FieldId={FieldId}", fieldId);
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during AddFieldImage: FieldId={FieldId}", fieldId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Đã xảy ra lỗi hệ thống khi thêm ảnh sân." });
            }
        }
        */

        /// <summary>
        /// Xóa ảnh của sân thể thao.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="fieldImageId">ID của ảnh cần xóa.</param>
        /// <returns>Thông báo xóa thành công.</returns>
        /// <response code="200">Xóa ảnh thành công.</response>
        /// <response code="401">Chưa đăng nhập hoặc không có quyền Owner.</response>
        /// <response code="404">Sân hoặc ảnh không tồn tại.</response>
        [HttpDelete("{fieldId}/images/{fieldImageId}")]
        [Authorize(Policy = "OwnerOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteFieldImage(int fieldId, int fieldImageId)
        {
            try
            {
                await _fieldService.DeleteFieldImageAsync(User, fieldId, fieldImageId);
                _logger.LogInformation("Field image deleted successfully: FieldId={FieldId}, FieldImageId={FieldImageId}", fieldId, fieldImageId);
                return Ok(new { Message = "Xóa ảnh thành công" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Field or image not found: FieldId={FieldId}, FieldImageId={FieldImageId}", fieldId, fieldImageId);
                return NotFound(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access during DeleteFieldImage: FieldId={FieldId}, FieldImageId={FieldImageId}", fieldId, fieldImageId);
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during DeleteFieldImage: FieldId={FieldId}, FieldImageId={FieldImageId}", fieldId, fieldImageId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Đã xảy ra lỗi hệ thống khi xóa ảnh sân." });
            }
        }

        /// <summary>
        /// Tạo dịch vụ cho sân thể thao.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="createFieldServiceDto">Thông tin dịch vụ cần tạo.</param>
        /// <returns>Thông tin dịch vụ vừa tạo.</returns>
        /// <response code="201">Tạo dịch vụ thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc không có quyền Owner.</response>
        /// <response code="404">Sân không tồn tại.</response>
        [HttpPost("{fieldId}/services")]
        [Authorize(Policy = "OwnerOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateFieldService(int fieldId, [FromBody] CreateFieldServiceDto createFieldServiceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for CreateFieldService: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                var fieldService = await _fieldService.CreateFieldServiceAsync(User, fieldId, createFieldServiceDto);
                _logger.LogInformation("Field service created successfully: FieldId={FieldId}, FieldServiceId={FieldServiceId}", fieldId, fieldService.FieldServiceId);
                return CreatedAtAction(nameof(GetFieldById), new { fieldId }, new { Message = "Tạo dịch vụ thành công", Data = fieldService });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error during CreateFieldService: FieldId={FieldId}", fieldId);
                return BadRequest(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Field not found: FieldId={FieldId}", fieldId);
                return NotFound(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access during CreateFieldService: FieldId={FieldId}", fieldId);
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during CreateFieldService: FieldId={FieldId}", fieldId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Đã xảy ra lỗi hệ thống khi tạo dịch vụ sân." });
            }
        }

        /// <summary>
        /// Cập nhật dịch vụ của sân thể thao.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="fieldServiceId">ID của dịch vụ cần cập nhật.</param>
        /// <param name="updateFieldServiceDto">Thông tin cập nhật.</param>
        /// <returns>Thông tin dịch vụ đã cập nhật.</returns>
        /// <response code="200">Cập nhật dịch vụ thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc không có quyền Owner.</response>
        /// <response code="404">Sân hoặc dịch vụ không tồn tại.</response>
        [HttpPut("{fieldId}/services/{fieldServiceId}")]
        [Authorize(Policy = "OwnerOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateFieldService(int fieldId, int fieldServiceId, [FromBody] UpdateFieldServiceDto updateFieldServiceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for UpdateFieldService: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                var fieldService = await _fieldService.UpdateFieldServiceAsync(User, fieldId, fieldServiceId, updateFieldServiceDto);
                _logger.LogInformation("Field service updated successfully: FieldId={FieldId}, FieldServiceId={FieldServiceId}", fieldId, fieldServiceId);
                return Ok(new { Message = "Cập nhật dịch vụ thành công", Data = fieldService });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error during UpdateFieldService: FieldId={FieldId}, FieldServiceId={FieldServiceId}", fieldId, fieldServiceId);
                return BadRequest(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Field or service not found: FieldId={FieldId}, FieldServiceId={FieldServiceId}", fieldId, fieldServiceId);
                return NotFound(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access during UpdateFieldService: FieldId={FieldId}, FieldServiceId={FieldServiceId}", fieldId, fieldServiceId);
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during UpdateFieldService: FieldId={FieldId}, FieldServiceId={FieldServiceId}", fieldId, fieldServiceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Đã xảy ra lỗi hệ thống khi cập nhật dịch vụ sân." });
            }
        }

        /// <summary>
        /// Tạo tiện ích cho sân thể thao.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="createFieldAmenityDto">Thông tin tiện ích cần tạo.</param>
        /// <returns>Thông tin tiện ích vừa tạo.</returns>
        /// <response code="201">Tạo tiện ích thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc không có quyền Owner.</response>
        /// <response code="404">Sân không tồn tại.</response>
        [HttpPost("{fieldId}/amenities")]
        [Authorize(Policy = "OwnerOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateFieldAmenity(int fieldId, [FromBody] CreateFieldAmenityDto createFieldAmenityDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for CreateFieldAmenity: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                var fieldAmenity = await _fieldService.CreateFieldAmenityAsync(User, fieldId, createFieldAmenityDto);
                _logger.LogInformation("Field amenity created successfully: FieldId={FieldId}, FieldAmenityId={FieldAmenityId}", fieldId, fieldAmenity.FieldAmenityId);
                return CreatedAtAction(nameof(GetFieldById), new { fieldId }, new { Message = "Tạo tiện ích thành công", Data = fieldAmenity });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error during CreateFieldAmenity: FieldId={FieldId}", fieldId);
                return BadRequest(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Field not found: FieldId={FieldId}", fieldId);
                return NotFound(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access during CreateFieldAmenity: FieldId={FieldId}", fieldId);
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during CreateFieldAmenity: FieldId={FieldId}", fieldId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Đã xảy ra lỗi hệ thống khi tạo tiện ích sân." });
            }
        }

        /// <summary>
        /// Cập nhật tiện ích của sân thể thao.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="fieldAmenityId">ID của tiện ích cần cập nhật.</param>
        /// <param name="updateFieldAmenityDto">Thông tin cập nhật.</param>
        /// <returns>Thông tin tiện ích đã cập nhật.</returns>
        /// <response code="200">Cập nhật tiện ích thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc không có quyền Owner.</response>
        /// <response code="404">Sân hoặc tiện ích không tồn tại.</response>
        [HttpPut("{fieldId}/amenities/{fieldAmenityId}")]
        [Authorize(Policy = "OwnerOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateFieldAmenity(int fieldId, int fieldAmenityId, [FromBody] UpdateFieldAmenityDto updateFieldAmenityDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for UpdateFieldAmenity: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                var fieldAmenity = await _fieldService.UpdateFieldAmenityAsync(User, fieldId, fieldAmenityId, updateFieldAmenityDto);
                _logger.LogInformation("Field amenity updated successfully: FieldId={FieldId}, FieldAmenityId={FieldAmenityId}", fieldId, fieldAmenityId);
                return Ok(new { Message = "Cập nhật tiện ích thành công", Data = fieldAmenity });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error during UpdateFieldAmenity: FieldId={FieldId}, FieldAmenityId={FieldAmenityId}", fieldId, fieldAmenityId);
                return BadRequest(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Field or amenity not found: FieldId={FieldId}, FieldAmenityId={FieldAmenityId}", fieldId, fieldAmenityId);
                return NotFound(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access during UpdateFieldAmenity: FieldId={FieldId}, FieldAmenityId={FieldAmenityId}", fieldId, fieldAmenityId);
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during UpdateFieldAmenity: FieldId={FieldId}, FieldAmenityId={FieldAmenityId}", fieldId, fieldAmenityId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Đã xảy ra lỗi hệ thống khi cập nhật tiện ích sân." });
            }
        }

        /// <summary>
        /// Tạo mô tả cho sân thể thao.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="createFieldDescriptionDto">Thông tin mô tả cần tạo.</param>
        /// <returns>Thông tin mô tả vừa tạo.</returns>
        /// <response code="201">Tạo mô tả thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc không có quyền Owner.</response>
        /// <response code="404">Sân không tồn tại.</response>
        [HttpPost("{fieldId}/descriptions")]
        [Authorize(Policy = "OwnerOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateFieldDescription(int fieldId, [FromBody] CreateFieldDescriptionDto createFieldDescriptionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for CreateFieldDescription: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                var fieldDescription = await _fieldService.CreateFieldDescriptionAsync(User, fieldId, createFieldDescriptionDto);
                _logger.LogInformation("Field description created successfully: FieldId={FieldId}, FieldDescriptionId={FieldDescriptionId}", fieldId, fieldDescription.FieldDescriptionId);
                return CreatedAtAction(nameof(GetFieldById), new { fieldId }, new { Message = "Tạo mô tả thành công", Data = fieldDescription });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error during CreateFieldDescription: FieldId={FieldId}", fieldId);
                return BadRequest(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Field not found: FieldId={FieldId}", fieldId);
                return NotFound(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access during CreateFieldDescription: FieldId={FieldId}", fieldId);
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during CreateFieldDescription: FieldId={FieldId}", fieldId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Đã xảy ra lỗi hệ thống khi tạo mô tả sân." });
            }
        }

        /// <summary>
        /// Cập nhật mô tả của sân thể thao.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="fieldDescriptionId">ID của mô tả cần cập nhật.</param>
        /// <param name="updateFieldDescriptionDto">Thông tin cập nhật.</param>
        /// <returns>Thông tin mô tả đã cập nhật.</returns>
        /// <response code="200">Cập nhật mô tả thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc không có quyền Owner.</response>
        /// <response code="404">Sân hoặc mô tả không tồn tại.</response>
        [HttpPut("{fieldId}/descriptions/{fieldDescriptionId}")]
        [Authorize(Policy = "OwnerOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateFieldDescription(int fieldId, int fieldDescriptionId, [FromBody] UpdateFieldDescriptionDto updateFieldDescriptionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for UpdateFieldDescription: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                var fieldDescription = await _fieldService.UpdateFieldDescriptionAsync(User, fieldId, fieldDescriptionId, updateFieldDescriptionDto);
                _logger.LogInformation("Field description updated successfully: FieldId={FieldId}, FieldDescriptionId={FieldDescriptionId}", fieldId, fieldDescriptionId);
                return Ok(new { Message = "Cập nhật mô tả thành công", Data = fieldDescription });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error during UpdateFieldDescription: FieldId={FieldId}, FieldDescriptionId={FieldDescriptionId}", fieldId, fieldDescriptionId);
                return BadRequest(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Field or description not found: FieldId={FieldId}, FieldDescriptionId={FieldDescriptionId}", fieldId, fieldDescriptionId);
                return NotFound(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access during UpdateFieldDescription: FieldId={FieldId}, FieldDescriptionId={FieldDescriptionId}", fieldId, fieldDescriptionId);
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during UpdateFieldDescription: FieldId={FieldId}, FieldDescriptionId={FieldDescriptionId}", fieldId, fieldDescriptionId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Đã xảy ra lỗi hệ thống khi cập nhật mô tả sân." });
            }
        }

        /// <summary>
        /// Tạo giá cho sân thể thao.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="createFieldPricingDto">Thông tin giá cần tạo.</param>
        /// <returns>Thông tin giá vừa tạo.</returns>
        /// <response code="201">Tạo giá thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc không có quyền Owner.</response>
        /// <response code="404">Sân không tồn tại.</response>
        [HttpPost("{fieldId}/pricing")]
        [Authorize(Policy = "OwnerOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateFieldPricing(int fieldId, [FromBody] CreateFieldPricingDto createFieldPricingDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for CreateFieldPricing: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                var fieldPricing = await _fieldService.CreateFieldPricingAsync(User, fieldId, createFieldPricingDto);
                _logger.LogInformation("Field pricing created successfully: FieldId={FieldId}, FieldPricingId={FieldPricingId}", fieldId, fieldPricing.FieldPricingId);
                return CreatedAtAction(nameof(GetFieldById), new { fieldId }, new { Message = "Tạo giá thành công", Data = fieldPricing });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error during CreateFieldPricing: FieldId={FieldId}", fieldId);
                return BadRequest(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Field not found: FieldId={FieldId}", fieldId);
                return NotFound(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access during CreateFieldPricing: FieldId={FieldId}", fieldId);
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during CreateFieldPricing: FieldId={FieldId}", fieldId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Đã xảy ra lỗi hệ thống khi tạo giá sân." });
            }
        }

        /// <summary>
        /// Cập nhật giá của sân thể thao.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="fieldPricingId">ID của giá cần cập nhật.</param>
        /// <param name="updateFieldPricingDto">Thông tin cập nhật.</param>
        /// <returns>Thông tin giá đã cập nhật.</returns>
        /// <response code="200">Cập nhật giá thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc không có quyền Owner.</response>
        /// <response code="404">Sân hoặc giá không tồn tại.</response>
        [HttpPut("{fieldId}/pricing/{fieldPricingId}")]
        [Authorize(Policy = "OwnerOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateFieldPricing(int fieldId, int fieldPricingId, [FromBody] UpdateFieldPricingDto updateFieldPricingDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for UpdateFieldPricing: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                var fieldPricing = await _fieldService.UpdateFieldPricingAsync(User, fieldId, fieldPricingId, updateFieldPricingDto);
                _logger.LogInformation("Field pricing updated successfully: FieldId={FieldId}, FieldPricingId={FieldPricingId}", fieldId, fieldPricingId);
                return Ok(new { Message = "Cập nhật giá thành công", Data = fieldPricing });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error during UpdateFieldPricing: FieldId={FieldId}, FieldPricingId={FieldPricingId}", fieldId, fieldPricingId);
                return BadRequest(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Field or pricing not found: FieldId={FieldId}, FieldPricingId={FieldPricingId}", fieldId, fieldPricingId);
                return NotFound(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access during UpdateFieldPricing: FieldId={FieldId}, FieldPricingId={FieldPricingId}", fieldId, fieldPricingId);
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during UpdateFieldPricing: FieldId={FieldId}, FieldPricingId={FieldPricingId}", fieldId, fieldPricingId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Đã xảy ra lỗi hệ thống khi cập nhật giá sân." });
            }
        }

        /// <summary>
        /// Tạo sân con (subfield) cho sân thể thao.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="createSubFieldDto">Thông tin sân con cần tạo.</param>
        /// <returns>Thông tin sân con vừa tạo.</returns>
        /// <response code="201">Tạo sân con thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc không có quyền Owner.</response>
        /// <response code="404">Sân không tồn tại.</response>
        [HttpPost("{fieldId}/subfields")]
        [Authorize(Policy = "OwnerOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateSubField(int fieldId, [FromBody] CreateSubFieldDto createSubFieldDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for CreateSubField: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                var subField = await _subFieldService.CreateSubFieldAsync(User, fieldId, createSubFieldDto);
                _logger.LogInformation("SubField created successfully: FieldId={FieldId}, SubFieldId={SubFieldId}", fieldId, subField.SubFieldId);
                return CreatedAtAction(nameof(GetFieldById), new { fieldId }, new { Message = "Tạo sân con thành công", Data = subField });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error during CreateSubField: FieldId={FieldId}", fieldId);
                return BadRequest(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Field not found: FieldId={FieldId}", fieldId);
                return NotFound(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access during CreateSubField: FieldId={FieldId}", fieldId);
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during CreateSubField: FieldId={FieldId}", fieldId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Đã xảy ra lỗi hệ thống khi tạo sân con." });
            }
        }

        /// <summary>
        /// Cập nhật sân con của sân thể thao.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="subFieldId">ID của sân con cần cập nhật.</param>
        /// <param name="updateSubFieldDto">Thông tin cập nhật.</param>
        /// <returns>Thông tin sân con đã cập nhật.</returns>
        /// <response code="200">Cập nhật sân con thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc không có quyền Owner.</response>
        /// <response code="404">Sân hoặc sân con không tồn tại.</response>
        [HttpPut("{fieldId}/subfields/{subFieldId}")]
        [Authorize(Policy = "OwnerOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSubField(int fieldId, int subFieldId, [FromBody] UpdateSubFieldDto updateSubFieldDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for UpdateSubField: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                var subField = await _subFieldService.UpdateSubFieldAsync(User, fieldId, subFieldId, updateSubFieldDto);
                _logger.LogInformation("SubField updated successfully: FieldId={FieldId}, SubFieldId={SubFieldId}", fieldId, subFieldId);
                return Ok(new { Message = "Cập nhật sân con thành công", Data = subField });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error during UpdateSubField: FieldId={FieldId}, SubFieldId={SubFieldId}", fieldId, subFieldId);
                return BadRequest(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Field or subfield not found: FieldId={FieldId}, SubFieldId={SubFieldId}", fieldId, subFieldId);
                return NotFound(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access during UpdateSubField: FieldId={FieldId}, SubFieldId={SubFieldId}", fieldId, subFieldId);
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during UpdateSubField: FieldId={FieldId}, SubFieldId={SubFieldId}", fieldId, subFieldId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Đã xảy ra lỗi hệ thống khi cập nhật sân con." });
            }
        }

        /// <summary>
        /// Xóa sân con của sân thể thao.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="subFieldId">ID của sân con cần xóa.</param>
        /// <returns>Thông báo xóa thành công.</returns>
        /// <response code="200">Xóa sân con thành công.</response>
        /// <response code="401">Chưa đăng nhập hoặc không có quyền Owner.</response>
        /// <response code="404">Sân hoặc sân con không tồn tại.</response>
        [HttpDelete("{fieldId}/subfields/{subFieldId}")]
        [Authorize(Policy = "OwnerOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSubField(int fieldId, int subFieldId)
        {
            try
            {
                await _subFieldService.DeleteSubFieldAsync(User, fieldId, subFieldId);
                _logger.LogInformation("SubField deleted successfully: FieldId={FieldId}, SubFieldId={SubFieldId}", fieldId, subFieldId);
                return Ok(new { Message = "Xóa sân con thành công" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Field or subfield not found: FieldId={FieldId}, SubFieldId={SubFieldId}", fieldId, subFieldId);
                return NotFound(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access during DeleteSubField: FieldId={FieldId}, SubFieldId={SubFieldId}", fieldId, subFieldId);
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during DeleteSubField: FieldId={FieldId}, SubFieldId={SubFieldId}", fieldId, subFieldId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Đã xảy ra lỗi hệ thống khi xóa sân con." });
            }
        }

        /// <summary>
        /// Lấy thông tin lịch trống của sân thể thao.
        /// </summary>
        /// <param name="fieldId">ID của sân (tùy chọn).</param>
        /// <param name="date">Ngày cần kiểm tra (tùy chọn).</param>
        /// <returns>Danh sách lịch trống.</returns>
        /// <response code="200">Trả về lịch trống.</response>
        /// <response code="400">Tham số không hợp lệ.</response>
        [HttpGet("availability")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetFieldAvailability([FromQuery] int? fieldId, [FromQuery] DateTime? date)
        {
            try
            {
                var availability = await _fieldService.GetFieldAvailabilityAsync(fieldId, date);
                _logger.LogInformation("Field availability retrieved successfully: FieldId={FieldId}, Date={Date}", fieldId, date);
                return Ok(new { Data = availability });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error during GetFieldAvailability: FieldId={FieldId}, Date={Date}", fieldId, date);
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during GetFieldAvailability: FieldId={FieldId}, Date={Date}", fieldId, date);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Đã xảy ra lỗi hệ thống khi lấy lịch trống." });
            }
        }

        /// <summary>
        /// Xác thực địa chỉ của sân thể thao.
        /// </summary>
        /// <param name="addressDto">Thông tin địa chỉ cần xác thực.</param>
        /// <returns>Kết quả xác thực địa chỉ.</returns>
        /// <response code="200">Xác thực địa chỉ thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc không có quyền Owner.</response>
        [HttpPost("validate-address")]
        [Authorize(Policy = "OwnerOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ValidateAddress([FromBody] ValidateAddressDto addressDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for ValidateAddress: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                var result = await _fieldService.ValidateAddressAsync(addressDto);
                _logger.LogInformation("Address validated successfully");
                return Ok(new { Message = "Xác thực địa chỉ thành công", Data = result });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error during ValidateAddress");
                return BadRequest(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access during ValidateAddress");
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during ValidateAddress");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Đã xảy ra lỗi hệ thống khi xác thực địa chỉ." });
            }
        }
    }
}