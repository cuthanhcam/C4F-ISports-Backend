using System.Threading.Tasks;
using System.Collections.Generic;
using api.Dtos.Field;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace api.Interfaces
{
    /// <summary>
    /// Interface định nghĩa các phương thức cho dịch vụ quản lý sân thể thao.
    /// </summary>
    public interface IFieldService
    {
        /// <summary>
        /// Lấy danh sách sân thể thao với phân trang và lọc.
        /// </summary>
        /// <param name="filterDto">Thông tin lọc (thành phố, quận, loại sân, v.v.).</param>
        /// <returns>Danh sách sân và thông tin phân trang.</returns>
        Task<(IList<FieldDto> Data, int Total, int Page, int PageSize)> GetFilteredFieldsAsync(FieldFilterDto filterDto);

        /// <summary>
        /// Lấy chi tiết một sân thể thao theo ID.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="include">Danh sách dữ liệu liên quan cần bao gồm (subfields, services, amenities, images).</param>
        /// <returns>Thông tin chi tiết sân.</returns>
        Task<FieldDetailsDto> GetFieldByIdAsync(int fieldId, string? include);

        /// <summary>
        /// Xác thực địa chỉ sân.
        /// </summary>
        /// <param name="validateAddressDto">Thông tin địa chỉ cần xác thực.</param>
        /// <returns>Kết quả xác thực địa chỉ.</returns>
        Task<AddressValidationDto> ValidateAddressAsync(ValidateAddressDto validateAddressDto);

        /// <summary>
        /// Tạo một sân thể thao mới.
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal (Owner).</param>
        /// <param name="createFieldDto">Thông tin sân cần tạo.</param>
        /// <returns>Thông tin sân đã tạo.</returns>
        Task<FieldDetailsDto> CreateFieldAsync(ClaimsPrincipal user, CreateFieldDto createFieldDto);

        /// <summary>
        /// Tải lên ảnh cho sân thể thao.
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal (Owner).</param>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="file">File ảnh tải lên.</param>
        /// <param name="isPrimary">Ảnh có phải là ảnh chính không.</param>
        /// <returns>Thông tin ảnh đã tải lên.</returns>
        Task<FieldImageDto> UploadFieldImageAsync(ClaimsPrincipal user, int fieldId, IFormFile file, bool isPrimary);

        /// <summary>
        /// Cập nhật thông tin sân thể thao.
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal (Owner).</param>
        /// <param name="fieldId">ID của sân cần cập nhật.</param>
        /// <param name="updateFieldDto">Thông tin cập nhật.</param>
        /// <returns>Thông tin sân đã cập nhật.</returns>
        Task<FieldDetailsDto> UpdateFieldAsync(ClaimsPrincipal user, int fieldId, UpdateFieldDto updateFieldDto);

        /// <summary>
        /// Xóa sân thể thao (soft delete).
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal (Owner).</param>
        /// <param name="fieldId">ID của sân cần xóa.</param>
        Task DeleteFieldAsync(ClaimsPrincipal user, int fieldId);

        /// <summary>
        /// Lấy thời gian trống của sân.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="availabilityDto">Thông tin yêu cầu (ngày, subfield, khung giờ).</param>
        /// <returns>Danh sách thời gian trống.</returns>
        Task<IList<SubFieldAvailabilityDto>> GetFieldAvailabilityAsync(int fieldId, AvailabilityFilterDto availabilityDto);

        /// <summary>
        /// Lấy danh sách đánh giá của sân.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="filterDto">Thông tin lọc (điểm tối thiểu, sắp xếp).</param>
        /// <returns>Danh sách đánh giá và thông tin phân trang.</returns>
        Task<(IList<ReviewDto> Data, int Total, int Page, int PageSize)> GetFieldReviewsAsync(int fieldId, ReviewFilterDto filterDto);

        /// <summary>
        /// Lấy danh sách đặt sân của sân.
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal (Owner).</param>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="filterDto">Thông tin lọc (trạng thái, ngày).</param>
        /// <returns>Danh sách đặt sân và thông tin phân trang.</returns>
        Task<(IList<BookingDto> Data, int Total, int Page, int PageSize)> GetFieldBookingsAsync(ClaimsPrincipal user, int fieldId, BookingFilterDto filterDto);
    }
}