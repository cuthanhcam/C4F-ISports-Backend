using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.Interfaces;
using api.Dtos.Field;
using api.Dtos;
using System.Security.Claims;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FieldController : ControllerBase
    {
        private readonly IFieldService _fieldService;

        public FieldController(IFieldService fieldService)
        {
            _fieldService = fieldService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<FieldDto>>> GetFields([FromQuery] FieldFilterDto filter)
        {
            var fields = await _fieldService.GetFieldsAsync(filter);
            return Ok(fields);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FieldDto>> GetFieldById(int id)
        {
            var field = await _fieldService.GetFieldByIdAsync(id);
            if (field == null)
                return NotFound("Không tìm thấy sân");
            return Ok(field);
        }

        [HttpGet("search")]
        public async Task<ActionResult<PaginatedResponse<FieldDto>>> SearchFields([FromQuery] FieldSearchDto search)
        {
            var fields = await _fieldService.SearchFieldsAsync(search);
            return Ok(fields);
        }

        [HttpGet("{id}/available-slots")]
        public async Task<ActionResult<List<TimeSlotDto>>> GetAvailableSlots(int id, [FromQuery] DateTime date)
        {
            var slots = await _fieldService.GetAvailableSlotsAsync(id, date);
            if (slots == null)
                return NotFound("Không tìm thấy sân");
            return Ok(slots);
        }

        [Authorize(Roles = "Owner")]
        [HttpPost]
        public async Task<ActionResult<FieldDto>> CreateField([FromBody] CreateFieldDto createFieldDto)
        {
            var field = await _fieldService.CreateFieldAsync(User, createFieldDto);
            return CreatedAtAction(nameof(GetFieldById), new { id = field.FieldId }, field);
        }

        [Authorize(Roles = "Owner")]
        [HttpPut("{id}")]
        public async Task<ActionResult<FieldDto>> UpdateField(int id, [FromBody] UpdateFieldDto updateFieldDto)
        {
            var field = await _fieldService.UpdateFieldAsync(User, id, updateFieldDto);
            if (field == null)
                return NotFound("Không tìm thấy sân");
            return Ok(field);
        }

        [Authorize(Roles = "Owner")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteField(int id)
        {
            await _fieldService.DeleteFieldAsync(User, id);
            return NoContent();
        }

        [HttpGet("{id}/reviews")]
        public async Task<ActionResult<PaginatedResponse<FieldReviewDto>>> GetFieldReviews(
            int id, [FromQuery] int? rating, [FromQuery] string sort, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var reviews = await _fieldService.GetFieldReviewsAsync(id, rating, sort, page, pageSize);
            return Ok(reviews);
        }

        [HttpGet("nearby")]
        public async Task<ActionResult<PaginatedResponse<FieldDto>>> GetNearbyFields(
            [FromQuery] decimal latitude, [FromQuery] decimal longitude, [FromQuery] decimal radius,
            [FromQuery] string sort, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var fields = await _fieldService.GetNearbyFieldsAsync(latitude, longitude, radius, sort, page, pageSize);
            return Ok(fields);
        }

        [Authorize(Roles = "Owner")]
        [HttpPost("{id}/images")]
        public async Task<ActionResult<string>> UploadFieldImage(int id, [FromBody] string imageBase64)
        {
            var imageUrl = await _fieldService.UploadFieldImageAsync(User, id, imageBase64);
            return Ok(new { imageUrl });
        }

        #region TODO: Uncomment and implement the following methods as needed
        // [HttpGet("owner")]
        // public async Task<ActionResult<PaginatedResponse<FieldDto>>> GetOwnerFields(
        //     [FromQuery] string status,
        //     [FromQuery] string sort,
        //     [FromQuery] int page = 1,
        //     [FromQuery] int pageSize = 10)
        // {
        //     var user = User;
        //     var fields = await _fieldService.GetOwnerFieldsAsync(user, status, sort, page, pageSize);
        //     return Ok(fields);
        // }

        // [HttpPost("{fieldId}/report")]
        // public async Task<ActionResult> ReportField(int fieldId, FieldReportDto reportDto)
        // {
        //     await _fieldService.ReportFieldAsync(fieldId, reportDto);
        //     return NoContent();
        // }

        // [HttpGet("suggested")]
        // public async Task<ActionResult<PaginatedResponse<FieldDto>>> GetSuggestedFields(
        //     [FromQuery] decimal? latitude,
        //     [FromQuery] decimal? longitude,
        //     [FromQuery] int page = 1,
        //     [FromQuery] int pageSize = 10)
        // {
        //     var fields = await _fieldService.GetSuggestedFieldsAsync(latitude, longitude, page, pageSize);
        //     return Ok(fields);
        // }
        #endregion
    }
} 